// Type definitions for environment variables exposed by @ngx-env/builder.
// Only variables prefixed with NG_APP_ are injected into the browser bundle.
declare interface ImportMetaEnv {
  readonly [key: string]: string | undefined;
  /** Base URL of the HelpDesk AI backend API, e.g. http://localhost:5006/api */
  readonly NG_APP_API_BASE_URL?: string;
}

declare interface ImportMeta {
  readonly env: ImportMetaEnv;
}
