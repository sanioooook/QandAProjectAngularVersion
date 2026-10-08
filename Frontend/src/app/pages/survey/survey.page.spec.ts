import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { MockBackend, reply, survey } from '../../testing/fixtures';
import { appProviders } from '../../testing/test-app';
import { SurveyPage } from './survey.page';

const user = userEvent.setup();

async function open(backend: MockBackend) {
  await render(SurveyPage, { inputs: { id: 's1' }, providers: appProviders(backend) });
  await screen.findByRole('heading', { name: 'Where do we go?' });
}

describe('SurveyPage', () => {
  beforeEach(() => {
    localStorage.clear();
    localStorage.setItem('qanda.locale', 'en');
  });

  it('does not let the user pick more options than allowed', async () => {
    await open(new MockBackend({ 'GET /api/surveys/s1': reply(200, survey({ maxVotesPerUser: 2 })) }));

    await user.click(screen.getByRole('checkbox', { name: 'Cinema' }));
    await user.click(screen.getByRole('checkbox', { name: 'Park' }));

    expect((screen.getByRole('checkbox', { name: 'Museum' }) as HTMLInputElement).disabled).toBe(true);
    expect(screen.getByText('2 of 2 selected')).toBeTruthy();
  });

  it('sends the selection and shows the result returned by the API', async () => {
    const voted = survey({
      myVotes: [2],
      totalVotes: 1,
      voterCount: 1,
      options: survey().options.map((o) => (o.id === 2 ? { ...o, votes: 1 } : o)),
    });
    let sent: unknown;
    const backend = new MockBackend({
      'GET /api/surveys/s1': reply(200, survey()),
      'PUT /api/surveys/s1/votes': (request) => {
        sent = request.body;
        return reply(200, voted);
      },
    });
    await open(backend);

    await user.click(screen.getByRole('radio', { name: 'Park' }));
    await user.click(screen.getByRole('button', { name: 'Vote' }));

    expect(await screen.findByText('100%')).toBeTruthy();
    expect(sent).toEqual({ optionIds: [2] });
    expect(screen.getByRole('button', { name: 'Update vote' })).toBeTruthy();
  });

  it('a rejected vote shows the reason and reloads the survey', async () => {
    const backend = new MockBackend({
      'GET /api/surveys/s1': [reply(200, survey()), reply(200, survey({ status: 'closed', canVote: false }))],
      'PUT /api/surveys/s1/votes': reply(409, { code: 'survey_closed' }),
    });
    await open(backend);

    await user.click(screen.getByRole('radio', { name: 'Park' }));
    await user.click(screen.getByRole('button', { name: 'Vote' }));

    expect(await screen.findByText('The survey is closed.')).toBeTruthy();
    expect(await screen.findByText('Voting is over. Results are final.')).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Vote' })).toBeNull();
  });

  it('a long description is folded until the reader asks for it', async () => {
    await open(new MockBackend({ 'GET /api/surveys/s1': reply(200, survey({ description: 'Lorem ipsum dolor sit amet. '.repeat(30) })) }));

    const toggle = screen.getByRole('button', { name: 'Show more' });
    expect(toggle.getAttribute('aria-expanded')).toBe('false');
    await user.click(toggle);

    expect(screen.getByRole('button', { name: 'Show less' }).getAttribute('aria-expanded')).toBe('true');
  });

  it('a short description is shown whole, without a toggle', async () => {
    await open(new MockBackend({ 'GET /api/surveys/s1': reply(200, survey({ description: 'Bring snacks.' })) }));

    expect(screen.getByText('Bring snacks.')).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Show more' })).toBeNull();
  });

  it('a guest sees the results and an invitation to sign in instead of a vote button', async () => {
    await open(new MockBackend({ 'GET /api/surveys/s1': reply(200, survey({ canVote: false })) }));

    expect(screen.getByText('Sign in to vote')).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Vote' })).toBeNull();
    expect(screen.getAllByRole('radio').every((input) => (input as HTMLInputElement).disabled)).toBe(true);
  });

  it('a missing survey shows the not-found page', async () => {
    await render(SurveyPage, {
      inputs: { id: 's1' },
      providers: appProviders(new MockBackend({ 'GET /api/surveys/s1': reply(404, { code: 'not_found' }) })),
    });

    expect(await screen.findByRole('heading', { name: 'Page not found' })).toBeTruthy();
  });
});
