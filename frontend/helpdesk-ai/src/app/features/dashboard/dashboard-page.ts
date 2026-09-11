import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ApiService } from '../../core/api.service';
import { DashboardStats } from '../../core/models';

@Component({
  selector:'app-dashboard-page',
  imports:[RouterLink,MatButtonModule,MatProgressSpinnerModule,BaseChartDirective],
  template:`
    <header class="page-header"><div><p class="eyebrow">Overview</p><h1>Good {{ greeting }}, here’s the queue.</h1><p>Current workload and resolution signals across your support team.</p></div><a mat-flat-button routerLink="/tickets">Open ticket queue</a></header>
    @if (loading()) { <div class="loading"><mat-spinner diameter="38"/><span>Reading the support queue…</span></div> }
    @if (stats(); as s) {
      <section class="stat-grid">
        <article><span>All tickets</span><strong>{{s.total}}</strong><small>total conversations</small></article>
        <article><span>Needs attention</span><strong class="blue">{{s.open}}</strong><small>open or in progress</small></article>
        <article><span>Resolved</span><strong class="green">{{s.closed}}</strong><small>resolved or closed</small></article>
        <article><span>Unassigned</span><strong class="orange">{{s.unassigned}}</strong><small>waiting for an owner</small></article>
      </section>
      <section class="insight-grid">
        <article class="chart-card"><div><p class="eyebrow">Ticket mix</p><h2>Requests by category</h2></div><div class="chart-wrap"><canvas baseChart [data]="chartData" [options]="chartOptions" type="doughnut"></canvas></div></article>
        <article class="status-card"><p class="eyebrow">Workflow</p><h2>Status distribution</h2><div class="status-list">@for(item of statusItems;track item.name){<div><span><i [style.background]="item.color"></i>{{item.name}}</span><strong>{{item.value}}</strong><em><b [style.width.%]="item.percent" [style.background]="item.color"></b></em></div>}</div></article>
      </section>
    }
  `,
  styles:[`
    .page-header{display:flex;justify-content:space-between;align-items:end;margin-bottom:1.8rem}.page-header h1{font-size:2rem;margin:.25rem 0;letter-spacing:-.035em}.page-header p:last-child{color:var(--muted);margin:0}.eyebrow{color:#3975dc;text-transform:uppercase;letter-spacing:.12em;font-size:.72rem;font-weight:800}.stat-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:1rem}.stat-grid article,.chart-card,.status-card{background:#fff;border:1px solid var(--line);border-radius:1rem;padding:1.4rem;box-shadow:0 8px 28px rgba(28,45,75,.045)}.stat-grid article>span{color:var(--muted);font-size:.82rem;font-weight:650}.stat-grid strong{display:block;font-size:2.6rem;letter-spacing:-.05em;margin:.8rem 0 .15rem}.stat-grid small{color:#8993a3}.blue{color:#2867d9}.green{color:#16825e}.orange{color:#d66a1e}.insight-grid{display:grid;grid-template-columns:minmax(0,1.35fr) minmax(300px,.65fr);gap:1rem;margin-top:1rem}.chart-card,.status-card{min-height:370px}.chart-card{display:grid;grid-template-columns:1fr 300px;align-items:center}.chart-wrap{height:270px}h2{font-size:1.2rem;margin:.25rem 0}.status-list{display:grid;gap:1.25rem;margin-top:2rem}.status-list div{display:grid;grid-template-columns:1fr auto;gap:.45rem}.status-list span{display:flex;align-items:center;gap:.5rem}.status-list i{width:.55rem;height:.55rem;border-radius:50%}.status-list em{grid-column:1/-1;height:6px;border-radius:6px;background:#edf0f5;overflow:hidden}.status-list b{display:block;height:100%;border-radius:6px}.loading{height:50vh;display:grid;place-items:center;align-content:center;gap:1rem;color:var(--muted)}@media(max-width:1050px){.stat-grid{grid-template-columns:repeat(2,1fr)}.insight-grid{grid-template-columns:1fr}}@media(max-width:620px){.page-header{align-items:start;gap:1rem}.page-header>a{display:none}.stat-grid{grid-template-columns:1fr 1fr}.chart-card{grid-template-columns:1fr}.chart-wrap{height:230px}}
  `]
})
export class DashboardPage {
  private readonly api=inject(ApiService); readonly loading=signal(true); readonly stats=signal<DashboardStats|null>(null); chartData:ChartConfiguration<'doughnut'>['data']={labels:[],datasets:[]}; readonly chartOptions:ChartConfiguration<'doughnut'>['options']={responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom',labels:{usePointStyle:true,padding:20}}},cutout:'68%'}; statusItems:{name:string;value:number;percent:number;color:string}[]=[];
  readonly greeting=new Date().getHours()<12?'morning':new Date().getHours()<17?'afternoon':'evening';
  constructor(){this.api.stats().subscribe({next:s=>{this.stats.set(s);const entries=Object.entries(s.byCategory);this.chartData={labels:entries.map(x=>x[0]),datasets:[{data:entries.map(x=>x[1]),backgroundColor:['#3478e5','#7b61d1','#15a073','#f2a23b'],borderWidth:0,hoverOffset:5}]};const colors=['#3478e5','#7b61d1','#15a073','#8993a3'];this.statusItems=Object.entries(s.byStatus).map(([name,value],i)=>({name,value,percent:s.total?value/s.total*100:0,color:colors[i%colors.length]}));this.loading.set(false)},error:()=>this.loading.set(false)});}
}
