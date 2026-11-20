import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Project, Transaction } from '../../core/models';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss'
})
export class TransactionsComponent implements OnInit {
  rows: Transaction[] = [];
  projects: Project[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  loading = false;
  error = '';

  private api = inject(ApiService);
  private fb = inject(FormBuilder);

  filters = this.fb.nonNullable.group({
    from: [''],
    to: [''],
    projectId: [''],
    type: [''],
    search: ['']
  });

  ngOnInit() {
    this.api.getProjects().subscribe((projects) => (this.projects = projects));
    this.load();
  }

  load(page = 1) {
    this.page = page;
    this.loading = true;
    this.error = '';
    const f = this.filters.getRawValue();
    this.api.getTransactions({
      from: f.from || undefined,
      to: f.to || undefined,
      projectId: f.projectId ? Number(f.projectId) : null,
      type: f.type || undefined,
      search: f.search || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        this.rows = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: () => {
        this.error = 'Could not load transactions.';
        this.loading = false;
      }
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  remove(row: Transaction) {
    if (!confirm(`Delete ${row.type.toLowerCase()} of ₹${row.amount} — ${row.description}?`)) {
      return;
    }

    this.api.deleteTransaction(row.transactionId).subscribe({
      next: () => this.load(this.page),
      error: () => (this.error = 'Could not delete this transaction.')
    });
  }
}
