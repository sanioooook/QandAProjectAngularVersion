import { Location } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { screen, waitFor } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { FakeApi } from './testing/fake-api';
import { startApp } from './testing/test-app';

// The whole app (router, stores, i18n, every page) running against an in-memory API.

let api: FakeApi;
const user = userEvent.setup();

beforeEach(() => {
  localStorage.clear();
  sessionStorage.clear();
  localStorage.setItem('qanda.locale', 'en');
  api = new FakeApi();
});

describe('app', () => {
  it('a guest opens a shared link, signs up and comes back to vote', async () => {
    const bob = api.addUser('bob@example.com', 'Bob');
    const survey = api.addSurvey(bob, 'Lunch on Friday?', ['Pizza', 'Sushi']);

    await startApp(`/surveys/${survey.id}`, api);
    await screen.findByRole('heading', { name: 'Lunch on Friday?' });
    expect(screen.getByText('Sign in to vote')).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Vote' })).toBeNull();

    await user.click(screen.getAllByRole('link', { name: 'Sign up' })[1]!);
    await user.type(await screen.findByLabelText('Email'), 'carol@example.com');
    await user.type(screen.getByLabelText('Name'), 'Carol');
    await user.type(screen.getByLabelText('Password'), 'Secret123');
    await user.type(screen.getByLabelText('Repeat password'), 'Secret123');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    await screen.findByRole('heading', { name: 'Lunch on Friday?' });
    await user.click(await screen.findByRole('radio', { name: /Sushi/ }));
    await user.click(screen.getByRole('button', { name: 'Vote' }));

    await screen.findByText('100%');
    expect(screen.getByRole('button', { name: 'Update vote' })).toBeTruthy();
    expect(api.callsTo('PUT /api/surveys')).toHaveLength(1);
    await waitFor(() => expect(api.callsTo('POST /api/auth/register')).toHaveLength(1));
  });

  it('an author signs in, creates and publishes a survey and lands on its page', async () => {
    api.addUser('alice@example.com', 'Alice');

    await startApp('/login', api);
    await user.type(await screen.findByLabelText('Email'), 'alice@example.com');
    await user.type(screen.getByLabelText('Password'), 'Secret123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    await user.click((await screen.findAllByRole('link', { name: 'New survey' }))[0]!);
    await user.type(await screen.findByLabelText('Question'), 'Where do we go?');
    await user.type(screen.getByPlaceholderText('Option 1'), 'Cinema');
    await user.type(screen.getByPlaceholderText('Option 2'), 'Park');
    await user.click(screen.getByRole('button', { name: 'Publish' }));

    await screen.findByRole('heading', { name: 'Where do we go?' });
    expect(screen.getByRole('button', { name: 'Share' })).toBeTruthy();
    expect(api.surveys).toHaveLength(1);
    expect(api.surveys[0]).toMatchObject({ status: 'active', title: 'Where do we go?' });
  });

  it('Enter in the last option adds a new one and moves there', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    api.sessionUserId = alice.id;

    await startApp('/surveys/new', api);
    await user.type(await screen.findByPlaceholderText('Option 1'), 'A{Enter}');
    await user.keyboard('B{Enter}C');

    expect((screen.getByPlaceholderText('Option 3') as HTMLInputElement).value).toBe('C');
    expect(document.activeElement).toBe(screen.getByPlaceholderText('Option 3'));
  });

  it('client-side validation stops a broken survey before any request', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    api.sessionUserId = alice.id;

    await startApp('/surveys/new', api);
    await user.type(await screen.findByPlaceholderText('Option 1'), 'Same');
    await user.type(screen.getByPlaceholderText('Option 2'), 'same');
    await user.click(screen.getByRole('button', { name: 'Publish' }));

    expect(await screen.findByText('Enter the question.')).toBeTruthy();
    expect(screen.getByText('This option already exists.')).toBeTruthy();
    expect(api.callsTo('POST /api/surveys')).toHaveLength(0);
  });

  it('going back to the list reuses the cache instead of asking the server again', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    api.addSurvey(alice, 'Cached survey', ['A', 'B']);
    api.sessionUserId = alice.id;

    await startApp('/surveys', api);
    await user.click(await screen.findByRole('link', { name: /Cached survey/ }));
    await screen.findByRole('heading', { name: 'Cached survey' });
    TestBed.inject(Location).back();
    await user.click(await screen.findByRole('link', { name: /Cached survey/ }));
    await screen.findByRole('heading', { name: 'Cached survey' });

    expect(api.callsTo('GET /api/surveys?')).toHaveLength(1);
    expect(api.callsTo('GET /api/surveys/s')).toHaveLength(1);
  });

  it('switching the language translates the page, is remembered and is saved to the account', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    api.sessionUserId = alice.id;

    await startApp('/surveys', api);
    await screen.findByRole('heading', { name: 'Active surveys' });
    await user.click(screen.getByRole('button', { name: 'Account menu' }));
    await user.click(await screen.findByRole('menuitem', { name: /Language/ }));
    await user.click(await screen.findByRole('menuitem', { name: 'Українська' }));

    await screen.findByRole('heading', { name: 'Активні опитування' });
    expect(document.documentElement.lang).toBe('uk');
    expect(localStorage.getItem('qanda.locale')).toBe('uk');
    await waitFor(() => expect(api.users[0]!.locale).toBe('uk'));
    expect(api.callsTo('PUT /api/account/profile')).toHaveLength(1);
  });

  it('a guest can switch the language too, without touching the server', async () => {
    await startApp('/surveys', api);
    await screen.findByRole('heading', { name: 'Active surveys' });

    await user.click(screen.getByRole('button', { name: 'Language: English' }));
    await user.click(await screen.findByRole('menuitem', { name: 'Русский' }));

    await screen.findByRole('heading', { name: 'Активные опросы' });
    expect(api.callsTo('PUT')).toHaveLength(0);
  });

  it('deadlines are shown in the time zone chosen in the settings, with the offset', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    // 23:59:59.999 on Oct 22 in Kyiv (far ahead, so the survey stays open whenever the test runs).
    const survey = api.addSurvey(alice, 'Zoned', ['A', 'B'], { deadline: '2036-10-22T20:59:59.999Z' });
    api.sessionUserId = alice.id;
    localStorage.setItem('qanda.timeZone', 'America/New_York');

    const { router } = await startApp(`/surveys/${survey.id}`, api);
    expect(await screen.findByText('Oct 22, 2036, 4:59 PM GMT-4')).toBeTruthy();

    await router.navigateByUrl('/account');
    await user.click(await screen.findByRole('combobox', { name: 'Time zone' }));
    await user.click(await screen.findByRole('option', { name: /^Europe\/Kyiv/ }));
    expect(localStorage.getItem('qanda.timeZone')).toBe('Europe/Kyiv');
    await router.navigateByUrl(`/surveys/${survey.id}`);

    expect(await screen.findByText('Oct 22, 2036, 11:59 PM GMT+3')).toBeTruthy();
  });

  it('nothing typed is lost when the session expires while publishing', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    api.sessionUserId = alice.id;
    api.expireSessionOn = 'POST /api/surveys';

    await startApp('/surveys/new', api);
    await user.type(await screen.findByLabelText('Question'), 'Survives a re-login?');
    await user.type(screen.getByPlaceholderText('Option 1'), 'Yes');
    await user.type(screen.getByPlaceholderText('Option 2'), 'No');
    await user.click(screen.getByRole('button', { name: 'Publish' }));

    await screen.findByRole('heading', { name: 'Welcome back' });
    api.expireSessionOn = null;
    await user.type(screen.getByLabelText('Email'), 'alice@example.com');
    await user.type(screen.getByLabelText('Password'), 'Secret123');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(await screen.findByText('Restored what you typed before.')).toBeTruthy();
    expect((screen.getByLabelText('Question') as HTMLInputElement).value).toBe('Survives a re-login?');
    expect((screen.getByPlaceholderText('Option 2') as HTMLInputElement).value).toBe('No');

    await user.click(screen.getByRole('button', { name: 'Publish' }));
    await screen.findByRole('heading', { name: 'Survives a re-login?' });
    expect(sessionStorage.getItem('qanda.surveyForm.new')).toBeNull();
  });

  it('an expired session while voting sends the user to sign in and back to the survey', async () => {
    const alice = api.addUser('alice@example.com', 'Alice');
    const survey = api.addSurvey(alice, 'Expiring', ['Yes', 'No']);
    api.sessionUserId = alice.id;
    api.expireSessionOn = `PUT /api/surveys/${survey.id}/votes`;

    const { router } = await startApp(`/surveys/${survey.id}`, api);
    await user.click(await screen.findByRole('radio', { name: /Yes/ }));
    await user.click(screen.getByRole('button', { name: 'Vote' }));

    await screen.findByRole('heading', { name: 'Welcome back' });
    expect(router.parseUrl(router.url).queryParams['redirect']).toBe(`/surveys/${survey.id}`);
    expect(await screen.findByText('Your session has expired. Please sign in again.')).toBeTruthy();
  });
});
