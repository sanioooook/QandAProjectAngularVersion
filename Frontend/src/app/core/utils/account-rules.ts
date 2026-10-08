import { EMAIL_PATTERN, LIMITS } from '../limits';

// Mirror of Backend/src/QandA.Api/Auth/AccountRules.cs: each returns the API error code or null.

export function emailError(email: string): string | null {
  const value = email.trim();
  return !value || value.length > LIMITS.emailMax || !EMAIL_PATTERN.test(value) ? 'email_invalid' : null;
}

export function displayNameError(name: string): string | null {
  const value = name.trim().replace(/\s+/g, ' ');
  return value.length < LIMITS.displayNameMin || value.length > LIMITS.displayNameMax ? 'name_length' : null;
}

export function passwordError(password: string, email: string): string | null {
  if (password.length < LIMITS.passwordMin || password.length > LIMITS.passwordMax) return 'password_length';
  if (!/\p{L}/u.test(password) || !/\d/.test(password)) return 'password_weak';
  const lower = password.toLowerCase();
  const mail = email.trim().toLowerCase();
  if (mail && (lower === mail || lower === mail.split('@')[0])) return 'password_equals_email';
  return null;
}
