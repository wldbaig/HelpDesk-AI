import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, filter, finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { PagedResult, Ticket, TicketFilters } from '../../core/models';
import { TICKET_CATEGORIES, TICKET_PRIORITIES, TICKET_STATUSES } from '../../core/ticket-options';
import { TicketCreateDialog } from './ticket-create-dialog';

const DEFAULT_FILTERS: TicketFilters = { page: 1, pageSize: 10 };

@Component({
  selector: 'app-tickets-page',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './tickets-page.html',
  styleUrl: './tickets-page.scss',
})
export class TicketsPage {
  private readonly api = inject(ApiService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly page = signal<PagedResult<Ticket> | null>(null);
  readonly loading = signal(true);
  readonly search = new FormControl('', { nonNullable: true });

  readonly columns = ['ticket', 'category', 'priority', 'status', 'agent', 'created'];
  readonly statuses = TICKET_STATUSES;
  readonly categories = TICKET_CATEGORIES;
  readonly priorities = TICKET_PRIORITIES;

  filters: TicketFilters = { ...DEFAULT_FILTERS };

  constructor() {
    this.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((search) => {
        this.filters = { ...this.filters, search, page: 1 };
        this.load();
      });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api
      .tickets(this.filters)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((data) => this.page.set(data));
  }

  setFilter(key: keyof TicketFilters, value: string): void {
    this.filters = { ...this.filters, [key]: value, page: 1 };
    this.load();
  }

  paginate(event: PageEvent): void {
    this.filters = { ...this.filters, page: event.pageIndex + 1, pageSize: event.pageSize };
    this.load();
  }

  reset(): void {
    this.filters = { ...DEFAULT_FILTERS };
    this.search.setValue('', { emitEvent: false });
    this.load();
  }

  openCreate(): void {
    this.dialog
      .open(TicketCreateDialog, { width: '600px' })
      .afterClosed()
      .pipe(filter(Boolean))
      .subscribe((value) => this.api.createTicket(value).subscribe(() => this.reset()));
  }
}
