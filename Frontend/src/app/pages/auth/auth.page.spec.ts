import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { MockBackend, reply } from '../../testing/fixtures';
import { appProviders } from '../../testing/test-app';
import { AuthPage } from './auth.page';

const user = userEvent.setup();

async function openRegister(backend: MockBackend) {
  await render(AuthPage, { inputs: { mode: 'register' }, providers: appProviders(backend) });
  await screen.findByRole('heading', { name: 'Create an account' });
}

async function fill(email: string, name: string, password: string, repeat: string) {
  await user.type(screen.getByLabelText('Email'), email);
  await user.type(screen.getByLabelText('Name'), name);
  await user.type(screen.getByLabelText('Password'), password);
  await user.type(screen.getByLabelText('Repeat password'), repeat);
  await user.click(screen.getByRole('button', { name: 'Create account' }));
}

describe('AuthPage (register)', () => {
  beforeEach(() => {
    localStorage.clear();
    localStorage.setItem('qanda.locale', 'en');
  });

  it('does not call the API when the form is invalid', async () => {
    const backend = new MockBackend({});
    await openRegister(backend);

    await fill('not-an-email', 'A', 'Secret123', 'Secret124');

    expect(await screen.findByText('Enter a valid email address.')).toBeTruthy();
    expect(screen.getByText('Name must be 2–50 characters.')).toBeTruthy();
    expect(screen.getByText('Passwords do not match')).toBeTruthy();
    expect(backend.callsTo('POST')).toHaveLength(0);
  });

  it.each([
    ['short1', 'Password must be 8–128 characters.'],
    ['onlyletters', 'Password needs at least one letter and one digit.'],
    ['alice@example.com1', ''],
  ])('checks the password %j like the API does', async (password, message) => {
    const backend = new MockBackend({ 'POST /api/auth/register': reply(201, {}) });
    await openRegister(backend);

    await fill('alice@example.com', 'Alice', password, password);

    if (message) {
      expect(await screen.findByText(message)).toBeTruthy();
      expect(backend.callsTo('POST')).toHaveLength(0);
    } else {
      expect(backend.callsTo('POST /api/auth/register')).toHaveLength(1);
    }
  });

  it('shows the server error next to the field', async () => {
    await openRegister(new MockBackend({ 'POST /api/auth/register': reply(409, { code: 'email_taken', errors: { email: ['email_taken'] } }) }));

    await fill('alice@example.com', 'Alice', 'Secret123', 'Secret123');

    expect(await screen.findByText('An account with this email already exists.')).toBeTruthy();
    expect(screen.getByLabelText('Email').getAttribute('aria-invalid')).toBe('true');
  });
});

describe('AuthPage (login)', () => {
  beforeEach(() => {
    localStorage.clear();
    localStorage.setItem('qanda.locale', 'en');
  });

  it('a wrong password is reported without naming the field', async () => {
    const backend = new MockBackend({ 'POST /api/auth/login': reply(401, { code: 'invalid_credentials' }) });
    await render(AuthPage, { inputs: { mode: 'login' }, providers: appProviders(backend) });
    await screen.findByRole('heading', { name: 'Welcome back' });

    await user.type(screen.getByLabelText('Email'), 'alice@example.com');
    await user.type(screen.getByLabelText('Password'), 'Wrong12345');
    await user.keyboard('{Enter}');

    expect((await screen.findByRole('alert')).textContent).toContain('Wrong email or password.');
  });
});
