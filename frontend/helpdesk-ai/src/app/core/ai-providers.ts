// Supported LLM providers for ticket analysis. `value` must match the backend
// AiProvider enum names sent as the `provider` query parameter; `label` is shown in the UI.
export interface AiProviderOption {
  label: string;
  value: string;
}

export const AI_PROVIDERS: AiProviderOption[] = [
  { label: 'OpenAI', value: 'OpenAi' },
  { label: 'Grok AI', value: 'Grok' },
  { label: 'Gemini', value: 'Gemini' },
  { label: 'Claude AI', value: 'Claude' },
];
