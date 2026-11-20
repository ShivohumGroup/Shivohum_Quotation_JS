import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { calcInvoice, formatAmount } from './document.calc';
import { InvoiceData, InvoiceStep } from './document.models';

@Component({
  selector: 'app-invoice-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './invoice-form.component.html'
})
export class InvoiceFormComponent {
  @Input({ required: true }) data!: InvoiceData;
  @Input({ required: true }) step!: InvoiceStep;
  @Output() dataChange = new EventEmitter<InvoiceData>();

  formatAmount = formatAmount;

  get totals() {
    return calcInvoice(this.data);
  }

  get termsText() {
    return this.data.paymentTerms.join('\n');
  }

  patch(key: keyof InvoiceData, value: InvoiceData[keyof InvoiceData]) {
    this.dataChange.emit({ ...this.data, [key]: value });
  }

  num(value: string | number) {
    return Number(value);
  }

  setTerms(text: string) {
    this.patch(
      'paymentTerms',
      text.split('\n').filter((line) => line.trim().length > 0)
    );
  }
}
