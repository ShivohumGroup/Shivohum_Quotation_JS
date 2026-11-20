import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Lookup, Project } from '../../core/models';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './transaction-form.component.html',
  styleUrl: './transaction-form.component.scss'
})
export class TransactionFormComponent implements OnInit {
  id: number | null = null;
  categories: Lookup[] = [];
  projects: Project[] = [];
  paymentModes: Lookup[] = [];
  accounts: Lookup[] = [];
  error = '';
  saving = false;

  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  form = this.fb.nonNullable.group({
    transactionDate: [todayIso(), Validators.required],
    type: ['Expense', Validators.required],
    categoryId: [0, Validators.required],
    description: ['', Validators.required],
    projectId: [''],
    paymentModeId: [0, Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    accountId: [''],
    notes: ['']
  });

  get isEdit(): boolean {
    return this.id !== null;
  }

  ngOnInit() {
    const rawId = this.route.snapshot.paramMap.get('id');
    this.id = rawId ? Number(rawId) : null;

    this.api.getProjects().subscribe((items) => (this.projects = items));
    this.api.getPaymentModes().subscribe((items) => {
      this.paymentModes = items;
      if (!this.isEdit && items.length) {
        this.form.patchValue({ paymentModeId: items[0].id });
      }
    });
    this.api.getAccounts().subscribe((items) => (this.accounts = items));

    this.form.controls.type.valueChanges.subscribe((type) => this.loadCategories(type, true));
    this.loadCategories(this.form.controls.type.value, !this.isEdit);

    if (this.id) {
      this.api.getTransaction(this.id).subscribe({
        next: (tx) => {
          this.form.patchValue({
            transactionDate: toDateInput(tx.transactionDate),
            type: tx.type,
            categoryId: tx.categoryId,
            description: tx.description,
            projectId: tx.projectId ? String(tx.projectId) : '',
            paymentModeId: tx.paymentModeId,
            amount: tx.amount,
            accountId: tx.accountId ? String(tx.accountId) : '',
            notes: tx.notes ?? ''
          });
          this.loadCategories(tx.type, false);
        },
        error: () => (this.error = 'Transaction not found.')
      });
    }
  }

  loadCategories(type: string, reset: boolean) {
    this.api.getCategories(type).subscribe((items) => {
      this.categories = items;
      if (reset) {
        this.form.patchValue({ categoryId: items[0]?.id ?? 0 });
      }
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body = {
      transactionDate: value.transactionDate,
      type: value.type,
      categoryId: Number(value.categoryId),
      description: value.description,
      projectId: value.projectId ? Number(value.projectId) : null,
      paymentModeId: Number(value.paymentModeId),
      amount: Number(value.amount),
      accountId: value.accountId ? Number(value.accountId) : null,
      notes: value.notes || null
    };

    this.saving = true;
    this.error = '';
    const request = this.id
      ? this.api.updateTransaction(this.id, body)
      : this.api.createTransaction(body);

    request.subscribe({
      next: () => this.router.navigateByUrl('/transactions'),
      error: (err) => {
        this.saving = false;
        this.error = err.error?.message || 'Could not save this transaction.';
      }
    });
  }
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

function toDateInput(value: string): string {
  return value.slice(0, 10);
}
