import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TICKET_PRIORITIES } from '../../core/ticket-options';

@Component({
  selector: 'app-ticket-create-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './ticket-create-dialog.html',
  styleUrl: './ticket-create-dialog.scss',
})
export class TicketCreateDialog {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(MatDialogRef<TicketCreateDialog>);

  readonly priorities = TICKET_PRIORITIES;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(160)]],
    description: ['', [Validators.required, Validators.maxLength(5000)]],
    priority: ['Medium', Validators.required],
  });

  save(): void {
    if (this.form.valid) this.ref.close(this.form.getRawValue());
  }
}
