import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector:'app-ticket-create-dialog',
  imports:[ReactiveFormsModule,MatButtonModule,MatDialogModule,MatFormFieldModule,MatInputModule,MatSelectModule],
  template:`<h2 mat-dialog-title>New support ticket</h2><mat-dialog-content><p>Capture the customer’s issue. AI analysis can classify it after creation.</p><form [formGroup]="form"><mat-form-field appearance="outline"><mat-label>Title</mat-label><input matInput formControlName="title" maxlength="160"></mat-form-field><mat-form-field appearance="outline"><mat-label>Description</mat-label><textarea matInput formControlName="description" rows="6" maxlength="5000"></textarea></mat-form-field><mat-form-field appearance="outline"><mat-label>Priority</mat-label><mat-select formControlName="priority">@for(p of priorities;track p){<mat-option [value]="p">{{p}}</mat-option>}</mat-select></mat-form-field></form></mat-dialog-content><mat-dialog-actions align="end"><button mat-button mat-dialog-close>Cancel</button><button mat-flat-button [disabled]="form.invalid" (click)="save()">Create ticket</button></mat-dialog-actions>`,
  styles:[`mat-dialog-content{min-width:min(540px,80vw)}mat-dialog-content>p{color:var(--muted);margin-top:0}form{display:grid;gap:.35rem;padding-top:.5rem}textarea{resize:vertical}`]
})
export class TicketCreateDialog {
  private readonly fb=inject(FormBuilder);private readonly ref=inject(MatDialogRef<TicketCreateDialog>);readonly priorities=['Low','Medium','High','Urgent'];
  readonly form=this.fb.nonNullable.group({title:['',[Validators.required,Validators.maxLength(160)]],description:['',[Validators.required,Validators.maxLength(5000)]],priority:['Medium',Validators.required]});
  save():void{if(this.form.valid)this.ref.close(this.form.getRawValue());}
}
