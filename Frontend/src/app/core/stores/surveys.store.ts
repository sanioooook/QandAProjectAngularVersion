import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withProps, withState } from '@ngrx/signals';
import { Api, ListQuery } from '../api/api';
import type { Paged, SurveyDetails, SurveyInput, SurveyScope, SurveySummary } from '../api/types';

/** Cached data younger than this is served without a request. */
export const CACHE_TTL_MS = 30_000;

interface Entry<T> {
  data: T;
  fetchedAt: number;
}

export interface FetchOptions {
  /** Ignore the cache and always ask the server. */
  force?: boolean;
}

interface SurveysState {
  lists: Record<string, Entry<Paged<SurveySummary>>>;
  details: Record<string, Entry<SurveyDetails>>;
}

export function listKey({ scope, status, page = 1 }: ListQuery): string {
  return `${scope}|${status ?? ''}|${page}`;
}

export function toSummary(survey: SurveyDetails): SurveySummary {
  return {
    id: survey.id,
    title: survey.title,
    author: survey.author,
    createdAt: survey.createdAt,
    publishedAt: survey.publishedAt,
    deadline: survey.deadline,
    status: survey.status,
    optionCount: survey.options.length,
    voterCount: survey.voterCount,
    hasVoted: survey.myVotes.length > 0,
  };
}

const isFresh = <T>(entry: Entry<T> | undefined): entry is Entry<T> =>
  !!entry && Date.now() - entry.fetchedAt < CACHE_TTL_MS;

/**
 * Cache of survey lists and survey details.
 *
 * - navigating between pages reuses fresh data instead of refetching it;
 * - identical concurrent requests share one HTTP call;
 * - mutations return the updated survey from the API, which is written into the cache and patched
 *   into every cached list, so nothing has to be reloaded after a vote; lists whose membership may
 *   have changed are only marked stale and refetched when shown next time.
 */
export const SurveysStore = signalStore(
  { providedIn: 'root' },
  withState<SurveysState>({ lists: {}, details: {} }),
  withProps(() => ({
    _api: inject(Api),
    _inflight: new Map<string, Promise<unknown>>(),
  })),
  withMethods((store) => {
    function dedupe<T>(key: string, load: () => Promise<T>): Promise<T> {
      const pending = store._inflight.get(key);
      if (pending) return pending as Promise<T>;
      const promise = load().finally(() => store._inflight.delete(key));
      store._inflight.set(key, promise);
      return promise;
    }

    function storeSurvey(survey: SurveyDetails): SurveyDetails {
      patchState(store, (state) => ({
        details: { ...state.details, [survey.id]: { data: survey, fetchedAt: Date.now() } },
        lists: mapLists(state.lists, (entry) =>
          entry.data.items.some((item) => item.id === survey.id)
            ? {
                ...entry,
                data: {
                  ...entry.data,
                  items: entry.data.items.map((item) => (item.id === survey.id ? toSummary(survey) : item)),
                },
              }
            : entry,
        ),
      }));
      return survey;
    }

    /** Marks lists of the given scopes stale: they stay visible but are refetched on next use. */
    function invalidateLists(scopes: SurveyScope[]): void {
      patchState(store, (state) => ({
        lists: mapLists(state.lists, (entry, key) =>
          scopes.includes(key.split('|')[0] as SurveyScope) ? { ...entry, fetchedAt: 0 } : entry,
        ),
      }));
    }

    function hasVoted(id: string): boolean {
      return (store.details()[id]?.data.myVotes.length ?? 0) > 0;
    }

    return {
      /** The cached list, if any (reactive: read it inside `computed`). */
      list(query: ListQuery): Paged<SurveySummary> | undefined {
        return store.lists()[listKey(query)]?.data;
      },

      /** The cached survey, if any (reactive: read it inside `computed`). */
      survey(id: string): SurveyDetails | undefined {
        return store.details()[id]?.data;
      },

      fetchList(query: ListQuery, { force = false }: FetchOptions = {}): Promise<Paged<SurveySummary>> {
        const key = listKey(query);
        const cached = store.lists()[key];
        if (!force && isFresh(cached)) return Promise.resolve(cached.data);
        return dedupe(`list:${key}`, async () => {
          const data = await store._api.surveys.list(query);
          patchState(store, (state) => ({ lists: { ...state.lists, [key]: { data, fetchedAt: Date.now() } } }));
          return data;
        });
      },

      fetchSurvey(id: string, { force = false }: FetchOptions = {}): Promise<SurveyDetails> {
        const cached = store.details()[id];
        if (!force && isFresh(cached)) return Promise.resolve(cached.data);
        return dedupe(`survey:${id}`, async () => storeSurvey(await store._api.surveys.get(id)));
      },

      invalidateLists,

      async create(input: SurveyInput): Promise<SurveyDetails> {
        const survey = storeSurvey(await store._api.surveys.create(input));
        invalidateLists(survey.status === 'draft' ? ['mine'] : ['mine', 'active']);
        return survey;
      },

      async update(id: string, input: SurveyInput): Promise<SurveyDetails> {
        const survey = storeSurvey(await store._api.surveys.update(id, input));
        invalidateLists(survey.status === 'draft' ? ['mine'] : ['mine', 'active']);
        return survey;
      },

      async publish(id: string): Promise<SurveyDetails> {
        const survey = storeSurvey(await store._api.surveys.publish(id));
        invalidateLists(['mine', 'active']);
        return survey;
      },

      async remove(id: string): Promise<void> {
        await store._api.surveys.remove(id);
        patchState(store, (state) => {
          const { [id]: _, ...details } = state.details;
          return {
            details,
            lists: mapLists(state.lists, (entry) => {
              const items = entry.data.items.filter((item) => item.id !== id);
              return items.length === entry.data.items.length
                ? entry
                : { data: { ...entry.data, items, total: entry.data.total - 1 }, fetchedAt: 0 };
            }),
          };
        });
      },

      async vote(id: string, optionIds: number[]): Promise<SurveyDetails> {
        const before = hasVoted(id);
        const survey = storeSurvey(await store._api.surveys.vote(id, optionIds));
        if (before !== survey.myVotes.length > 0) invalidateLists(['voted']);
        return survey;
      },

      async addOption(id: string, text: string, voteForIt: boolean): Promise<SurveyDetails> {
        const before = hasVoted(id);
        const survey = storeSurvey(await store._api.surveys.addOption(id, text, voteForIt));
        if (before !== survey.myVotes.length > 0) invalidateLists(['voted']);
        return survey;
      },

      /** Drops everything: cached surveys hold per-user data (my votes, author view, names). */
      reset(): void {
        store._inflight.clear();
        patchState(store, { lists: {}, details: {} });
      },
    };
  }),
);

function mapLists<T>(
  lists: Record<string, Entry<T>>,
  map: (entry: Entry<T>, key: string) => Entry<T>,
): Record<string, Entry<T>> {
  return Object.fromEntries(Object.entries(lists).map(([key, entry]) => [key, map(entry, key)]));
}
