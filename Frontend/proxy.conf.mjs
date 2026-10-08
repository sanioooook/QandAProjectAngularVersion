// `ng serve` proxies the API so the auth cookie stays same-origin.
// In docker compose (profile dev) the API is reachable as http://api:8080.
export default {
  '/api': {
    target: process.env.API_URL ?? 'http://localhost:8080',
    changeOrigin: true,
  },
};
