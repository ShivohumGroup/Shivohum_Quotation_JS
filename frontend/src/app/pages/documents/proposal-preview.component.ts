import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { calcTotals, formatCurrency, formatDate } from './document.calc';
import { ProposalData } from './document.models';

@Component({
  selector: 'app-proposal-preview',
  standalone: true,
  templateUrl: './proposal-preview.component.html'
})
export class ProposalPreviewComponent {
  @Input({ required: true }) data!: ProposalData;
  @ViewChild('sheet', { static: true }) sheet?: ElementRef<HTMLElement>;

  formatDate = formatDate;
  formatCurrency = formatCurrency;

  get totals() {
    return calcTotals(this.data);
  }

  get filledScope() {
    return this.data.scopeItems.filter((s) => s.title || s.description);
  }

  get filledMilestones() {
    return this.data.milestones.filter((m) => m.name || m.date || m.notes);
  }

  get filledCosts() {
    return this.data.costItems.filter((i) => i.name || i.price);
  }

  get filledPayments() {
    return this.data.paymentMilestones.filter((p) => p.label || p.percent);
  }

  nativeSheet() {
    return this.sheet?.nativeElement ?? null;
  }
}
