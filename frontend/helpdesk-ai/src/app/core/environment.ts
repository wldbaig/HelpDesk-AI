/**
 * Runtime configuration for the Angular app.
 *
 * Values are read from the `.env` file at build time by @ngx-env/builder
 * (any variable prefixed with `NG_APP_`). A sensible local default is kept
 * as a fallback so the app still runs when `.env` is absent.
 */
export const environment = {
  apiBaseUrl: import.meta.env.NG_APP_API_BASE_URL ?? 'http://localhost:5006/api',
} as const;
