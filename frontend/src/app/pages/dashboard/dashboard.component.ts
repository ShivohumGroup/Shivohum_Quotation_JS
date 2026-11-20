import { CurrencyPipe } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  inject
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { ApiService } from '../../core/api.service';
import { CategoryTotal, DashboardData } from '../../core/models';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  private api = inject(ApiService);

  @ViewChild('pieCanvas') pieCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('lineCanvas') lineCanvas?: ElementRef<HTMLCanvasElement>;

  preset: 'thisMonth' | 'thisYear' | 'custom' = 'thisYear';
  customFrom = yearStart();
  customTo = todayIso();
  loading = true;
  error = '';
  viewReady = false;
  data: DashboardData = emptyDashboard();

  private pieChart?: Chart;
  private lineChart?: Chart;

  ngOnInit() {
    this.load();
  }

  ngAfterViewInit() {
    this.viewReady = true;
    this.renderCharts();
  }

  ngOnDestroy() {
    this.pieChart?.destroy();
    this.lineChart?.destroy();
  }

  setPreset(preset: 'thisMonth' | 'thisYear' | 'custom') {
    this.preset = preset;
    if (preset !== 'custom') {
      this.load();
    }
  }

  load() {
    this.loading = true;
    this.error = '';
    const from = this.preset === 'custom' ? this.customFrom : undefined;
    const to = this.preset === 'custom' ? this.customTo : undefined;

    this.api.getDashboard(this.preset, from, to).subscribe({
      next: (data) => {
        this.data = data;
        this.loading = false;
        this.renderCharts();
      },
      error: () => {
        this.error = 'Could not load the dashboard. Sign in again if the session expired.';
        this.loading = false;
      }
    });
  }

  get pieCategories(): CategoryTotal[] {
    return this.data.categories.filter((c) => c.expense > 0);
  }

  private renderCharts() {
    if (!this.viewReady || this.loading) {
      return;
    }

    this.pieChart?.destroy();
    this.lineChart?.destroy();

    if (this.pieCanvas) {
      const slices = this.pieCategories;
      this.pieChart = new Chart(this.pieCanvas.nativeElement, {
        type: 'pie',
        data: {
          labels: slices.length ? slices.map((c) => c.categoryName) : ['No expense'],
          datasets: [
            {
              data: slices.length ? slices.map((c) => c.expense) : [1],
              backgroundColor: slices.length
                ? PALETTE.slice(0, slices.length)
                : ['#e7dfd0']
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { position: window.innerWidth < 760 ? 'bottom' : 'right' }
          }
        }
      });
    }

    if (this.lineCanvas) {
      this.lineChart = new Chart(this.lineCanvas.nativeElement, {
        type: 'line',
        data: {
          labels: this.data.months.map((m) => m.label),
          datasets: [
            {
              label: 'Income',
              data: this.data.months.map((m) => m.income),
              borderColor: '#1b7a4a',
              backgroundColor: 'rgba(27, 122, 74, 0.12)',
              tension: 0.25,
              fill: false
            },
            {
              label: 'Expense',
              data: this.data.months.map((m) => m.expense),
              borderColor: '#b42318',
              backgroundColor: 'rgba(180, 35, 24, 0.12)',
              tension: 0.25,
              fill: false
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'top' } },
          scales: {
            y: { beginAtZero: true }
          }
        }
      });
    }
  }
}

const PALETTE = [
  '#0b3d4a',
  '#d4a017',
  '#b42318',
  '#1b7a4a',
  '#3d7ea6',
  '#c46b1a',
  '#6b4c9a',
  '#4a7c59',
  '#8c4a3a',
  '#2f5d50',
  '#a67c52',
  '#5c6b73',
  '#9a3b4a',
  '#3b6d8c',
  '#7a8f3d'
];

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

function yearStart(): string {
  return `${new Date().getFullYear()}-01-01`;
}

function emptyDashboard(): DashboardData {
  return {
    preset: 'thisYear',
    from: yearStart(),
    to: todayIso(),
    summary: { totalIncome: 0, totalExpense: 0, netBalance: 0, thisMonthBalance: 0 },
    projects: [],
    months: [],
    categories: []
  };
}
