import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ApiService } from '../../core/api.service';
import { DashboardStats } from '../../core/models';

interface StatusItem {
  name: string;
  value: number;
  percent: number;
  color: string;
}

const CATEGORY_COLORS = ['#3478e5', '#7b61d1', '#15a073', '#f2a23b'];
const STATUS_COLORS = ['#3478e5', '#7b61d1', '#15a073', '#8993a3'];

@Component({
  selector: 'app-dashboard-page',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, BaseChartDirective],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  private readonly api = inject(ApiService);

  readonly loading = signal(true);
  readonly stats = signal<DashboardStats | null>(null);
  readonly greeting = this.resolveGreeting();

  chartData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [] };
  readonly chartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom', labels: { usePointStyle: true, padding: 20 } } },
    cutout: '68%',
  };
  statusItems: StatusItem[] = [];

  constructor() {
    this.api.stats().subscribe({
      next: (stats) => this.applyStats(stats),
      error: () => this.loading.set(false),
    });
  }

  private applyStats(stats: DashboardStats): void {
    this.stats.set(stats);
    this.chartData = this.buildCategoryChart(stats);
    this.statusItems = this.buildStatusItems(stats);
    this.loading.set(false);
  }

  private buildCategoryChart(stats: DashboardStats): ChartConfiguration<'doughnut'>['data'] {
    const entries = Object.entries(stats.byCategory);
    return {
      labels: entries.map(([name]) => name),
      datasets: [
        {
          data: entries.map(([, value]) => value),
          backgroundColor: CATEGORY_COLORS,
          borderWidth: 0,
          hoverOffset: 5,
        },
      ],
    };
  }

  private buildStatusItems(stats: DashboardStats): StatusItem[] {
    return Object.entries(stats.byStatus).map(([name, value], index) => ({
      name,
      value,
      percent: stats.total ? (value / stats.total) * 100 : 0,
      color: STATUS_COLORS[index % STATUS_COLORS.length],
    }));
  }

  private resolveGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'morning';
    if (hour < 17) return 'afternoon';
    return 'evening';
  }
}
