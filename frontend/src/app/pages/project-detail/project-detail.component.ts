import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { ClientPayment, ClientPaymentWrite, Lookup, Project } from '../../core/models';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss'
})
export class ProjectDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  projectId = 0;
  project: Project | null = null;
  payments: ClientPayment[] = [];
  paymentModes: Lookup[] = [];
  accounts: Lookup[] = [];
  editingPaymentId: number | null = null;
  error = '';
  saving = false;

  form = this.fb.nonNullable.group({
    paymentDate: [todayIso(), Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    paymentModeId: [0, Validators.required],
    accountId: [''],
    referenceNo: [''],
    notes: ['']
  });

  get isEditPayment(): boolean {
    return this.editingPaymentId !== null;
  }

  ngOnInit() {
    this.projectId = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getPaymentModes().subscribe((items) => {
      this.paymentModes = items;
      if (!this.isEditPayment && items.length) {
        this.form.patchValue({ paymentModeId: items[0].id });
      }
    });
    this.api.getAccounts().subscribe((items) => (this.accounts = items));
    this.load();
  }

  load() {
    this.api.getProject(this.projectId).subscribe({
      next: (project) => (this.project = project),
      error: () => (this.error = 'Project not found.')
    });
    this.api.getProjectPayments(this.projectId).subscribe({
      next: (payments) => (this.payments = payments),
      error: () => (this.error = 'Could not load client payments.')
    });
  }

  editPayment(payment: ClientPayment) {
    this.editingPaymentId = payment.clientPaymentId;
    this.error = '';
    this.form.reset({
      paymentDate: payment.paymentDate.slice(0, 10),
      amount: payment.amount,
      paymentModeId: payment.paymentModeId,
      accountId: payment.accountId ? String(payment.accountId) : '',
      referenceNo: payment.referenceNo ?? '',
      notes: payment.notes ?? ''
    });
  }

  cancelEdit() {
    this.editingPaymentId = null;
    this.error = '';
    this.form.reset({
      paymentDate: todayIso(),
      amount: 0,
      paymentModeId: this.paymentModes[0]?.id ?? 0,
      accountId: '',
      referenceNo: '',
      notes: ''
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body: ClientPaymentWrite = {
      paymentDate: value.paymentDate,
      amount: Number(value.amount),
      paymentModeId: Number(value.paymentModeId),
      accountId: value.accountId ? Number(value.accountId) : null,
      referenceNo: value.referenceNo || null,
      notes: value.notes || null
    };

    this.saving = true;
    this.error = '';
    const request = this.editingPaymentId
      ? this.api.updateProjectPayment(this.projectId, this.editingPaymentId, body)
      : this.api.createProjectPayment(this.projectId, body);

    request.subscribe({
      next: () => {
        this.saving = false;
        this.cancelEdit();
        this.load();
      },
      error: (err) => {
        this.saving = false;
        this.error = err.error?.message || 'Could not save this payment.';
      }
    });
  }

  removePayment(payment: ClientPayment) {
    if (!confirm(`Delete payment of ₹${payment.amount.toFixed(2)} on ${payment.paymentDate.slice(0, 10)}?`)) {
      return;
    }

    this.api.deleteProjectPayment(this.projectId, payment.clientPaymentId).subscribe({
      next: () => this.load(),
      error: () => (this.error = 'Could not delete this payment.')
    });
  }
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}
