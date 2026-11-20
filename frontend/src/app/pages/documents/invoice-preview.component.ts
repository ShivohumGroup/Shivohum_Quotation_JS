import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { calcInvoice, formatAmount, formatInvoiceDate } from './document.calc';
import { InvoiceData } from './document.models';

@Component({
  selector: 'app-invoice-preview',
  standalone: true,
  templateUrl: './invoice-preview.component.html'
})
export class InvoicePreviewComponent {
  @Input({ required: true }) data!: InvoiceData;
  @ViewChild('sheet', { static: true }) sheet?: ElementRef<HTMLElement>;

  formatInvoiceDate = formatInvoiceDate;
  formatAmount = formatAmount;

  get totals() {
    return calcInvoice(this.data);
  }

  get rows() {
    const t = this.totals;
    return [
      { label: '1. Total Project Amount', amount: t.total, bold: false },
      { label: '2. Previous Payment', amount: t.previous, bold: false },
      { label: '3. Payment Request', amount: t.request, bold: true },
      { label: '4. Remaining Balance', amount: t.remaining, bold: false }
    ];
  }

  nativeSheet() {
    return this.sheet?.nativeElement ?? null;
  }
}
