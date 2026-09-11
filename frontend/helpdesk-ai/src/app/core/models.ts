export interface ApiResponse<T> { success: boolean; data: T; message?: string; errors?: Record<string, string[]>; }
export interface User { id: string; name: string; email: string; role: 'Admin' | 'Agent'; }
export interface AuthResponse { token: string; user: User; }
export interface Comment { id: string; body: string; createdAt: string; author: User; }
export interface Ticket {
  id: string; title: string; description: string; category: string; priority: string; status: string;
  sentiment?: string; suggestedReply?: string; createdAt: string; updatedAt: string; assignedAgent?: User; comments: Comment[];
}
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; }
export interface DashboardStats { total: number; open: number; closed: number; unassigned: number; byCategory: Record<string, number>; byStatus: Record<string, number>; }
export interface TicketFilters { search?: string; status?: string; category?: string; priority?: string; page?: number; pageSize?: number; }
