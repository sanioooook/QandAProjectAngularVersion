import { HttpBackend, provideHttpClient, withInterceptors } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { MockBackend, paged, reply, summary, survey } from '../../testing/fixtures';
import { apiInterceptor } from '../api/api.interceptor';
import { CACHE_TTL_MS, SurveysStore } from './surveys.store';

function setup(routes: ConstructorParameters<typeof MockBackend>[0]) {
  const backend = new MockBackend(routes);
  TestBed.configureTestingModule({
    providers: [provideHttpClient(withInterceptors([apiInterceptor])), { provide: HttpBackend, useValue: backend }],
  });
  return { store: TestBed.inject(SurveysStore), backend };
}

describe('SurveysStore', () => {
  afterEach(() => vi.useRealTimers());

  it('serves a fresh list from the cache without a second request', async () => {
    const { store, backend } = setup({ 'GET /api/surveys': reply(200, paged([summary()])) });

    await store.fetchList({ scope: 'active' });
    const again = await store.fetchList({ scope: 'active' });

    expect(backend.calls).toHaveLength(1);
    expect(again.items).toHaveLength(1);
  });

  it('refetches after the TTL or when forced', async () => {
    vi.useFakeTimers();
    const { store, backend } = setup({ 'GET /api/surveys': reply(200, paged([])) });

    await store.fetchList({ scope: 'active' });
    await store.fetchList({ scope: 'active' }, { force: true });
    vi.advanceTimersByTime(CACHE_TTL_MS + 1);
    await store.fetchList({ scope: 'active' });

    expect(backend.calls).toHaveLength(3);
  });

  it('shares one request between concurrent identical calls', async () => {
    const { store, backend } = setup({ 'GET /api/surveys/s1': reply(200, survey()) });

    const [a, b] = await Promise.all([store.fetchSurvey('s1'), store.fetchSurvey('s1')]);

    expect(backend.calls).toHaveLength(1);
    expect(a).toBe(b);
  });

  it('keeps different pages and filters apart', async () => {
    const { store, backend } = setup({
      'GET /api/surveys': [reply(200, paged([summary({ id: 'a' })])), reply(200, paged([summary({ id: 'b' })]))],
    });

    await store.fetchList({ scope: 'mine', page: 1 });
    await store.fetchList({ scope: 'mine', page: 2 });

    expect(backend.calls).toEqual(['GET /api/surveys?scope=mine&page=1&pageSize=20', 'GET /api/surveys?scope=mine&page=2&pageSize=20']);
    expect(store.list({ scope: 'mine', page: 2 })?.items[0]?.id).toBe('b');
  });

  it('a vote updates the survey and patches cached lists without reloading them', async () => {
    const voted = survey({ myVotes: [2], voterCount: 1, totalVotes: 1 });
    const { store, backend } = setup({
      'GET /api/surveys': [reply(200, paged([summary()])), reply(200, paged([])), reply(200, paged([summary({ hasVoted: true })]))],
      'PUT /api/surveys/s1/votes': reply(200, voted),
    });
    await store.fetchList({ scope: 'active' });
    await store.fetchList({ scope: 'voted' });

    await store.vote('s1', [2]);

    expect(store.survey('s1')?.myVotes).toEqual([2]);
    const listed = store.list({ scope: 'active' })!.items[0]!;
    expect(listed.hasVoted).toBe(true);
    expect(listed.voterCount).toBe(1);
    // The active list is still fresh; the "voted" list changed membership and is now stale.
    await store.fetchList({ scope: 'active' });
    expect(backend.calls).toHaveLength(3);
    await store.fetchList({ scope: 'voted' });
    expect(store.list({ scope: 'voted' })!.items).toHaveLength(1);
    expect(backend.calls).toHaveLength(4);
  });

  it('a failed request leaves the cache untouched and is not cached itself', async () => {
    const { store, backend } = setup({
      'GET /api/surveys/s1': [reply(200, survey()), reply(200, survey({ status: 'closed' }))],
      'PUT /api/surveys/s1/votes': reply(409, { code: 'survey_closed' }),
    });
    await store.fetchSurvey('s1');

    await expect(store.vote('s1', [1])).rejects.toMatchObject({ code: 'survey_closed', status: 409 });

    expect(store.survey('s1')?.myVotes).toEqual([]);
    await store.fetchSurvey('s1', { force: true });
    expect(store.survey('s1')?.status).toBe('closed');
    expect(backend.calls).toHaveLength(3);
  });

  it('delete removes the survey from details and every cached list', async () => {
    const { store } = setup({
      'GET /api/surveys': reply(200, paged([summary({ id: 's1' }), summary({ id: 's2' })])),
      'GET /api/surveys/s1': reply(200, survey()),
      'DELETE /api/surveys/s1': reply(204),
    });
    await store.fetchList({ scope: 'mine' });
    await store.fetchSurvey('s1');

    await store.remove('s1');

    expect(store.survey('s1')).toBeUndefined();
    const list = store.list({ scope: 'mine' })!;
    expect(list.items.map((s) => s.id)).toEqual(['s2']);
    expect(list.total).toBe(1);
  });

  it('publishing marks the active list stale so the new survey shows up there', async () => {
    const { store, backend } = setup({
      'GET /api/surveys': reply(200, paged([])),
      'POST /api/surveys/s1/publish': reply(200, survey()),
    });
    await store.fetchList({ scope: 'active' });

    await store.publish('s1');
    await store.fetchList({ scope: 'active' });

    expect(backend.callsTo('GET /api/surveys?')).toHaveLength(2);
  });

  it('reset drops everything', async () => {
    const { store } = setup({ 'GET /api/surveys/s1': reply(200, survey()) });
    await store.fetchSurvey('s1');

    store.reset();

    expect(store.survey('s1')).toBeUndefined();
  });
});
