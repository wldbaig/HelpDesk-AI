import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from './environment';
import { ApiResponse, AuthResponse, Comment, DashboardStats, PagedResult, Ticket, TicketFilters, User } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private unwrap<T>() { return map((response: ApiResponse<T>) => response.data); }
  login(body: { email: string; password: string }): Observable<AuthResponse> { return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/login`, body).pipe(this.unwrap()); }
  register(body: { name: string; email: string; password: string }): Observable<AuthResponse> { return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/register`, body).pipe(this.unwrap()); }
  stats(): Observable<DashboardStats> { return this.http.get<ApiResponse<DashboardStats>>(`${this.baseUrl}/dashboard/stats`).pipe(this.unwrap()); }
  agents(): Observable<User[]> { return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users/agents`).pipe(this.unwrap()); }
  tickets(filters: TicketFilters): Observable<PagedResult<Ticket>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => { if (value !== undefined && value !== '') params = params.set(key, String(value)); });
    return this.http.get<ApiResponse<PagedResult<Ticket>>>(`${this.baseUrl}/tickets`, { params }).pipe(this.unwrap());
  }
  ticket(id: string): Observable<Ticket> { return this.http.get<ApiResponse<Ticket>>(`${this.baseUrl}/tickets/${id}`).pipe(this.unwrap()); }
  createTicket(body: { title: string; description: string; priority: string }): Observable<Ticket> { return this.http.post<ApiResponse<Ticket>>(`${this.baseUrl}/tickets`, body).pipe(this.unwrap()); }
  updateTicket(id: string, body: Partial<Ticket>): Observable<Ticket> { return this.http.put<ApiResponse<Ticket>>(`${this.baseUrl}/tickets/${id}`, body).pipe(this.unwrap()); }
  assignTicket(id: string, agentId: string): Observable<Ticket> { return this.http.post<ApiResponse<Ticket>>(`${this.baseUrl}/tickets/${id}/assign`, { agentId }).pipe(this.unwrap()); }
  addComment(id: string, body: string): Observable<Comment> { return this.http.post<ApiResponse<Comment>>(`${this.baseUrl}/tickets/${id}/comments`, { body }).pipe(this.unwrap()); }
  analyze(id: string): Observable<Ticket> { return this.http.post<ApiResponse<Ticket>>(`${this.baseUrl}/tickets/${id}/analyze`, {}).pipe(this.unwrap()); }
}
