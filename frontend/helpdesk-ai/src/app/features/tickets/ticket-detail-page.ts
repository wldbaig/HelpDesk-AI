import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Ticket, User } from '../../core/models';
import { TICKET_CATEGORIES, TICKET_PRIORITIES, TICKET_STATUSES } from '../../core/ticket-options';

@Component({
  selector: 'app-ticket-detail-page',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './ticket-detail-page.html',
  styleUrl: './ticket-detail-page.scss',
})
export class TicketDetailPage {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly snack = inject(MatSnackBar);
  readonly auth = inject(AuthService);

  readonly ticket = signal<Ticket | null>(null);
  readonly agents = signal<User[]>([]);
  readonly loading = signal(true);
  readonly analyzing = signal(false);
  readonly saving = signal(false);
  readonly commenting = signal(false);
  readonly draft = signal('');

  readonly statuses = TICKET_STATUSES;
  readonly priorities = TICKET_PRIORITIES;
  readonly categories = TICKET_CATEGORIES;

  readonly commentForm = this.fb.nonNullable.group({
    body: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  private readonly id = this.route.snapshot.paramMap.get('id')!;

  constructor() {
    this.api
      .ticket(this.id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((ticket) => this.setTicket(ticket));
    this.api.agents().subscribe((agents) => this.agents.set(agents));
  }

  analyze(): void {
    this.analyzing.set(true);
    this.api
      .analyze(this.id)
      .pipe(finalize(() => this.analyzing.set(false)))
      .subscribe((ticket) => {
        this.setTicket(ticket);
        this.snack.open('AI analysis is ready.', 'Dismiss', { duration: 3000 });
      });
  }

  saveDraft(): void {
    const current = this.ticket();
    if (!current) return;

    this.saving.set(true);
    this.persist(current, { suggestedReply: this.draft() })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe((updated) => {
        this.applyUpdate(current, updated);
        this.snack.open('Reply draft saved.', 'Dismiss', { duration: 2500 });
      });
  }

  updateProperty(key: 'status' | 'priority' | 'category', value: string): void {
    const current = this.ticket();
    if (!current) return;

    this.persist(current, { [key]: value }).subscribe((updated) => this.applyUpdate(current, updated));
  }

  assign(agentId: string): void {
    if (!agentId) return;

    const current = this.ticket()!;
    this.api.assignTicket(this.id, agentId).subscribe((updated) => this.applyUpdate(current, updated));
  }

  addComment(): void {
    if (this.commentForm.invalid) return;

    this.commenting.set(true);
    this.api
      .addComment(this.id, this.commentForm.getRawValue().body)
      .pipe(finalize(() => this.commenting.set(false)))
      .subscribe((comment) => {
        const current = this.ticket()!;
        this.ticket.set({ ...current, comments: [...current.comments, comment] });
        this.commentForm.reset();
      });
  }

  /** Sends the full ticket payload with a set of overridden fields applied. */
  private persist(base: Ticket, overrides: Partial<Ticket>): Observable<Ticket> {
    return this.api.updateTicket(base.id, {
      title: base.title,
      description: base.description,
      category: base.category,
      priority: base.priority,
      status: base.status,
      suggestedReply: base.suggestedReply,
      ...overrides,
    });
  }

  /** Merges a server response back onto the current ticket, keeping already-loaded comments. */
  private applyUpdate(base: Ticket, updated: Ticket): void {
    this.setTicket({ ...base, ...updated, comments: base.comments });
  }

  private setTicket(ticket: Ticket): void {
    this.ticket.set({ ...ticket, comments: ticket.comments ?? [] });
    this.draft.set(ticket.suggestedReply ?? '');
  }
}
