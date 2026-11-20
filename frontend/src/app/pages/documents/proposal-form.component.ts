import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { calcTotals, formatCurrency } from './document.calc';
import { createId } from './document.defaults';
import { ProposalData, QuoteStep } from './document.models';

@Component({
  selector: 'app-proposal-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './proposal-form.component.html'
})
export class ProposalFormComponent {
  @Input({ required: true }) data!: ProposalData;
  @Input({ required: true }) step!: QuoteStep;
  @Output() dataChange = new EventEmitter<ProposalData>();

  formatCurrency = formatCurrency;

  get totals() {
    return calcTotals(this.data);
  }

  patch(key: keyof ProposalData, value: ProposalData[keyof ProposalData]) {
    this.dataChange.emit({ ...this.data, [key]: value });
  }

  num(value: string | number) {
    return Number(value);
  }

  patchList(key: 'scopeItems' | 'milestones' | 'costItems' | 'paymentMilestones', id: string, partial: object) {
    const list = (this.data[key] as { id: string }[]).map((item) =>
      item.id === id ? { ...item, ...partial } : item
    );
    this.dataChange.emit({ ...this.data, [key]: list } as ProposalData);
  }

  removeList(key: 'scopeItems' | 'milestones' | 'costItems' | 'paymentMilestones', id: string) {
    this.dataChange.emit({
      ...this.data,
      [key]: (this.data[key] as { id: string }[]).filter((item) => item.id !== id)
    } as ProposalData);
  }

  addScope() {
    this.patch('scopeItems', [...this.data.scopeItems, { id: createId(), title: '', description: '' }]);
  }

  addCost() {
    this.patch('costItems', [...this.data.costItems, { id: createId(), name: '', unit: '', area: '', price: 0 }]);
  }

  addMilestone() {
    this.patch('milestones', [...this.data.milestones, { id: createId(), name: '', date: '', notes: '' }]);
  }

  addPay() {
    this.patch('paymentMilestones', [
      ...this.data.paymentMilestones,
      { id: createId(), label: '', percent: 0, dueTrigger: '' }
    ]);
  }
}
