import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { ApiError } from './api-error';
import { SessionEvents, apiInterceptor } from './api.interceptor';

describe('apiInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let expired: number;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([apiInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    expired = 0;
    TestBed.inject(SessionEvents).expired.subscribe(() => expired++);
  });

  afterEach(() => backend.verify());

  async function fail(url: string, status: number, body: string | object | null, statusText = 'Error'): Promise<unknown> {
    const result = firstValueFrom(http.get(url)).catch((e: unknown) => e);
    backend.expectOne(url).flush(body, { status, statusText });
    return result;
  }

  it('turns problem details into an ApiError with code and camelCase fields', async () => {
    const error = await fail('/api/auth/register', 400, { code: 'validation', title: 'Invalid', errors: { Email: ['email_invalid'] } });

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ status: 400, code: 'validation', fields: { email: ['email_invalid'] } });
  });

  it('derives a code from the status when the body is not JSON', async () => {
    await expect(fail('/api/surveys', 502, '<html>Bad gateway</html>')).resolves.toMatchObject({ code: 'server_error', status: 502 });
    await expect(fail('/api/surveys/x', 404, null)).resolves.toMatchObject({ code: 'not_found' });
  });

  it('reports network failures with the network code', async () => {
    const result = firstValueFrom(http.get('/api/surveys')).catch((e: unknown) => e);
    backend.expectOne('/api/surveys').error(new ProgressEvent('error'));

    await expect(result).resolves.toMatchObject({ code: 'network', status: 0 });
  });

  it('announces an expired session for 401 outside the auth endpoints only', async () => {
    await fail('/api/auth/login', 401, { code: 'invalid_credentials' });
    expect(expired).toBe(0);

    await fail('/api/surveys', 401, { code: 'unauthorized' });
    expect(expired).toBe(1);
  });

  it('asks for JSON', () => {
    void firstValueFrom(http.get('/api/auth/me')).catch(() => {});
    const request = backend.expectOne('/api/auth/me');
    expect(request.request.headers.get('Accept')).toBe('application/json');
    request.flush({});
  });
});
