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
import { TicketCreateDialog } from './ticket-create-dialog';

@Component({
  selector:'app-tickets-page',
  imports:[DatePipe,RouterLink,ReactiveFormsModule,MatButtonModule,MatDialogModule,MatFormFieldModule,MatInputModule,MatPaginatorModule,MatProgressSpinnerModule,MatSelectModule,MatTableModule],
  template:`
    <header class="page-header"><div><p class="eyebrow">Ticket queue</p><h1>Customer conversations</h1><p>Search, prioritize, and move every issue toward resolution.</p></div><button mat-flat-button (click)="openCreate()">+ New ticket</button></header>
    <section class="toolbar">
      <mat-form-field appearance="outline" class="search"><mat-label>Search tickets</mat-label><input matInput [formControl]="search" placeholder="Title or description"></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Status</mat-label><mat-select [value]="filters.status" (selectionChange)="setFilter('status',$event.value)"><mat-option value="">All</mat-option>@for(x of statuses;track x){<mat-option [value]="x">{{x}}</mat-option>}</mat-select></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Category</mat-label><mat-select [value]="filters.category" (selectionChange)="setFilter('category',$event.value)"><mat-option value="">All</mat-option>@for(x of categories;track x){<mat-option [value]="x">{{x}}</mat-option>}</mat-select></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Priority</mat-label><mat-select [value]="filters.priority" (selectionChange)="setFilter('priority',$event.value)"><mat-option value="">All</mat-option>@for(x of priorities;track x){<mat-option [value]="x">{{x}}</mat-option>}</mat-select></mat-form-field>
    </section>
    <section class="table-card">
      @if(loading()){<div class="loading"><mat-spinner diameter="34"/><span>Loading tickets…</span></div>}@else if(page()?.items?.length){
      <div class="table-scroll"><table mat-table [dataSource]="page()!.items">
        <ng-container matColumnDef="ticket"><th mat-header-cell *matHeaderCellDef>Ticket</th><td mat-cell *matCellDef="let ticket"><a [routerLink]="['/tickets',ticket.id]">{{ticket.title}}</a><small>{{ticket.description}}</small></td></ng-container>
        <ng-container matColumnDef="category"><th mat-header-cell *matHeaderCellDef>Category</th><td mat-cell *matCellDef="let ticket"><span class="category">{{ticket.category}}</span></td></ng-container>
        <ng-container matColumnDef="priority"><th mat-header-cell *matHeaderCellDef>Priority</th><td mat-cell *matCellDef="let ticket"><span class="priority" [class]="'priority '+ticket.priority.toLowerCase()">{{ticket.priority}}</span></td></ng-container>
        <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let ticket"><span class="status" [class]="'status '+ticket.status.toLowerCase()">{{ticket.status}}</span></td></ng-container>
        <ng-container matColumnDef="agent"><th mat-header-cell *matHeaderCellDef>Owner</th><td mat-cell *matCellDef="let ticket">{{ticket.assignedAgent?.name || 'Unassigned'}}</td></ng-container>
        <ng-container matColumnDef="created"><th mat-header-cell *matHeaderCellDef>Created</th><td mat-cell *matCellDef="let ticket">{{ticket.createdAt|date:'MMM d, HH:mm'}}</td></ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr><tr mat-row *matRowDef="let row; columns: columns" [routerLink]="['/tickets',row.id]"></tr>
      </table></div><mat-paginator [length]="page()!.totalCount" [pageIndex]="page()!.page-1" [pageSize]="page()!.pageSize" [pageSizeOptions]="[5,10,25]" (page)="paginate($event)"/>
      }@else{<div class="empty"><strong>No tickets found</strong><p>Try changing the filters or create a new conversation.</p><button mat-stroked-button (click)="reset()">Clear filters</button></div>}
    </section>
  `,
  styles:[`
    .page-header{display:flex;justify-content:space-between;align-items:end;margin-bottom:1.5rem}.page-header h1{font-size:2rem;margin:.25rem 0;letter-spacing:-.035em}.page-header p:last-child{color:var(--muted);margin:0}.eyebrow{color:#3975dc;text-transform:uppercase;letter-spacing:.12em;font-size:.72rem;font-weight:800}.toolbar{display:grid;grid-template-columns:minmax(260px,1fr) repeat(3,170px);gap:.75rem}.toolbar mat-form-field{margin-bottom:-1.1rem}.table-card{margin-top:1.4rem;background:#fff;border:1px solid var(--line);border-radius:1rem;overflow:hidden;min-height:400px}table{width:100%}th{color:#717b8b;font-size:.73rem;text-transform:uppercase;letter-spacing:.06em}td{color:#4b5566}td:first-child{max-width:360px}td a{display:block;color:var(--ink);font-weight:700;text-decoration:none;margin-bottom:.25rem}td small{display:block;white-space:nowrap;text-overflow:ellipsis;overflow:hidden;color:#7b8595;max-width:340px}tr.mat-mdc-row{cursor:pointer}tr.mat-mdc-row:hover{background:#f7f9fc}.category,.priority,.status{display:inline-flex;padding:.25rem .5rem;border-radius:99px;background:#eef2f7;font-size:.76rem;font-weight:700}.priority.high,.priority.urgent{background:#fff0e9;color:#bd4a15}.priority.low{background:#eef7f3;color:#26735a}.status.open{background:#eaf2ff;color:#2867c9}.status.inprogress{background:#f1edff;color:#6648bd}.status.resolved,.status.closed{background:#e9f7f1;color:#147354}.loading,.empty{min-height:360px;display:grid;place-items:center;align-content:center;gap:.75rem;color:var(--muted);text-align:center}.empty strong{font-size:1.1rem;color:var(--ink)}.empty p{margin:0}.table-scroll{overflow:auto}@media(max-width:1000px){.toolbar{grid-template-columns:1fr 1fr}.search{grid-column:1/-1}}@media(max-width:620px){.page-header p:last-child{display:none}.toolbar{grid-template-columns:1fr}.search{grid-column:auto}.page-header{align-items:start}.page-header h1{font-size:1.6rem}}
  `]
})
export class TicketsPage {
  private readonly api=inject(ApiService);private readonly dialog=inject(MatDialog);private readonly destroyRef=inject(DestroyRef);readonly page=signal<PagedResult<Ticket>|null>(null);readonly loading=signal(true);readonly search=new FormControl('',{nonNullable:true});readonly columns=['ticket','category','priority','status','agent','created'];readonly statuses=['Open','InProgress','Resolved','Closed'];readonly categories=['Billing','Technical','Account','Other'];readonly priorities=['Low','Medium','High','Urgent'];filters:TicketFilters={page:1,pageSize:10};
  constructor(){this.search.valueChanges.pipe(debounceTime(300),distinctUntilChanged(),takeUntilDestroyed()).subscribe(search=>{this.filters={...this.filters,search,page:1};this.load();});this.load();}
  load():void{this.loading.set(true);this.api.tickets(this.filters).pipe(finalize(()=>this.loading.set(false))).subscribe(data=>this.page.set(data));}
  setFilter(key:keyof TicketFilters,value:string):void{this.filters={...this.filters,[key]:value,page:1};this.load();}
  paginate(event:PageEvent):void{this.filters={...this.filters,page:event.pageIndex+1,pageSize:event.pageSize};this.load();}
  reset():void{this.filters={page:1,pageSize:10};this.search.setValue('',{emitEvent:false});this.load();}
  openCreate():void{this.dialog.open(TicketCreateDialog,{width:'600px'}).afterClosed().pipe(filter(Boolean)).subscribe(value=>this.api.createTicket(value).subscribe(()=>this.reset()));}
}
