import { InvoiceData, ProposalData } from './document.models';

export function round2(n: number) {
  return Math.round((n + Number.EPSILON) * 100) / 100;
}

export function calcTotals(data: ProposalData) {
  const itemsTotal = round2(data.costItems.reduce((sum, item) => sum + (Number(item.price) || 0), 0));
  const discount = round2(Math.min(Math.max(data.discount || 0, 0), itemsTotal));
  const subtotal = round2(itemsTotal - discount);
  const tax = round2(subtotal * ((data.taxRate || 0) / 100));
  const grandTotal = round2(subtotal + tax);
  return { itemsTotal, discount, subtotal, tax, grandTotal };
}

export function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 2
  }).format(amount || 0);
}

export function formatDate(value: string) {
  if (!value) return '—';
  const d = new Date(value + 'T00:00:00');
  if (Number.isNaN(d.getTime())) return value;
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function calcInvoice(data: InvoiceData) {
  const total = round2(data.totalProjectAmount || 0);
  const previous = round2(data.previousPayment || 0);
  const request = round2(data.paymentRequest || 0);
  const remaining = round2(total - previous - request);
  return {
    total,
    previous,
    request,
    remaining,
    amountInWords: amountToWordsINR(request)
  };
}

export function formatInvoiceDate(value: string) {
  if (!value) return '—';
  const d = new Date(value + 'T00:00:00');
  if (Number.isNaN(d.getTime())) return value;
  const day = d.getDate();
  const suffix = ordinalSuffix(day);
  const month = d.toLocaleDateString('en-IN', { month: 'short' });
  const year = d.getFullYear();
  return `${day}${suffix} ${month} ${year}`;
}

function ordinalSuffix(day: number) {
  if (day > 3 && day < 21) return 'th';
  switch (day % 10) {
    case 1:
      return 'st';
    case 2:
      return 'nd';
    case 3:
      return 'rd';
    default:
      return 'th';
  }
}

const ones = [
  '',
  'One',
  'Two',
  'Three',
  'Four',
  'Five',
  'Six',
  'Seven',
  'Eight',
  'Nine',
  'Ten',
  'Eleven',
  'Twelve',
  'Thirteen',
  'Fourteen',
  'Fifteen',
  'Sixteen',
  'Seventeen',
  'Eighteen',
  'Nineteen'
];

const tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

function twoDigits(n: number): string {
  if (n < 20) return ones[n];
  const t = Math.floor(n / 10);
  const o = n % 10;
  return `${tens[t]}${o ? ` ${ones[o]}` : ''}`.trim();
}

function threeDigits(n: number): string {
  const h = Math.floor(n / 100);
  const rest = n % 100;
  if (h && rest) return `${ones[h]} Hundred ${twoDigits(rest)}`;
  if (h) return `${ones[h]} Hundred`;
  return twoDigits(rest);
}

export function amountToWordsINR(amount: number): string {
  const value = Math.round(Math.abs(amount || 0));
  if (value === 0) return 'Rupees Zero Only';
  const crore = Math.floor(value / 10000000);
  const lakh = Math.floor((value % 10000000) / 100000);
  const thousand = Math.floor((value % 100000) / 1000);
  const hundred = value % 1000;
  const parts: string[] = [];
  if (crore) parts.push(`${threeDigits(crore)} Crore`);
  if (lakh) parts.push(`${threeDigits(lakh)} Lakh`);
  if (thousand) parts.push(`${threeDigits(thousand)} Thousand`);
  if (hundred) parts.push(threeDigits(hundred));
  return `Rupees ${parts.join(' ')} Only`;
}

export function formatAmount(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(amount || 0);
}
