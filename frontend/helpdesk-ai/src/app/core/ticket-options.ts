// Shared ticket option lists, kept in one place so the queue filters,
// the detail properties panel and the create dialog stay in sync (DRY).
export const TICKET_STATUSES = ['Open', 'InProgress', 'Resolved', 'Closed'] as const;
export const TICKET_PRIORITIES = ['Low', 'Medium', 'High', 'Urgent'] as const;
export const TICKET_CATEGORIES = ['Billing', 'Technical', 'Account', 'Other'] as const;
