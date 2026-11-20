import { InvoiceData, ProposalData } from './document.models';

export function createId() {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
}

function todayISO() {
  return new Date().toISOString().slice(0, 10);
}

function plusDays(days: number) {
  const d = new Date();
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

export function createEmptyProposal(): ProposalData {
  return {
    proposalNumber: `SHG-${new Date().getFullYear()}-001`,
    proposalDate: todayISO(),
    validUntil: plusDays(30),
    projectName: '',
    jobLocation: '',
    projectType: 'New construction',
    estimatedStart: '',
    estimatedFinish: '',
    partyRole: 'Contractor',
    companyName: 'Shivohum Group',
    companyContact: '',
    companyAddress: '',
    companyPhone: '',
    companyEmail: '',
    companyWebsite: '',
    licenseNumber: '',
    insuranceInfo: '',
    clientCompany: '',
    clientContact: '',
    clientBillingAddress: '',
    clientSiteAddress: '',
    clientPhone: '',
    clientEmail: '',
    projectSummary: '',
    scopeItems: [{ id: createId(), title: 'Site preparation', description: '' }],
    milestones: [{ id: createId(), name: 'Project kickoff', date: '', notes: '' }],
    exclusions: '',
    materialsNotes: '',
    assumptions: '',
    costItems: [{ id: createId(), name: '', unit: '', area: '', price: 0 }],
    discount: 0,
    taxRate: 0,
    paymentTerms:
      'Payment is due as outlined in the payment schedule below. Late payments may incur standard interest charges.',
    paymentMilestones: [
      { id: createId(), label: 'Mobilization / deposit', percent: 30, dueTrigger: 'Upon signing' },
      { id: createId(), label: 'Progress payment', percent: 40, dueTrigger: 'Mid-project milestone' },
      { id: createId(), label: 'Final payment', percent: 30, dueTrigger: 'Upon substantial completion' }
    ],
    agreementTerms:
      'Proposed costs, specifications, and conditions detailed above are accepted, and specified work is authorized to begin on the agreed upon date. Payment for services rendered will be made as specified.',
    warranty:
      'Workmanship is warranted for a period of one (1) year from the date of substantial completion, unless otherwise stated.',
    changeOrderPolicy:
      'Any changes to the scope of work must be approved in writing via a change order before additional work begins. Pricing and schedule impacts will be documented on the change order.',
    clientSignName: '',
    clientSignDate: '',
    contractorSignName: 'Shivohum Group',
    contractorSignDate: ''
  };
}

function asList<T>(value: unknown, fallback: T[]): T[] {
  return Array.isArray(value) ? (value as T[]) : fallback;
}

export function hydrateProposal(payload: Partial<ProposalData> | null | undefined): ProposalData {
  const base = createEmptyProposal();
  if (!payload) return base;
  return {
    ...base,
    ...payload,
    scopeItems: asList(payload.scopeItems, base.scopeItems),
    milestones: asList(payload.milestones, base.milestones),
    costItems: asList(payload.costItems, base.costItems),
    paymentMilestones: asList(payload.paymentMilestones, base.paymentMilestones)
  };
}

export function hydrateInvoice(payload: Partial<InvoiceData> | null | undefined): InvoiceData {
  const base = createEmptyInvoice();
  if (!payload) return base;
  return {
    ...base,
    ...payload,
    paymentTerms: asList(payload.paymentTerms, base.paymentTerms)
  };
}

export function createEmptyInvoice(): InvoiceData {
  return {
    invoiceNumber: '33126',
    invoiceDate: todayISO(),
    billTo: '',
    companyName: 'SHIVOHUM GROUP',
    companyAddress: 'Ahmedabad, Gujarat',
    companyPhones: '9998712125, 6354403509, 7069450940',
    companyEmail: 'shivohumgrp@gmail.com',
    totalProjectAmount: 0,
    previousPayment: 0,
    paymentRequest: 0,
    paymentTerms: [
      'Payment to be made as per the payment request.',
      'Kindly make the payment at the earliest.',
      'This is a computer generated invoice.'
    ],
    thankYouLine: 'Thank you for your business!',
    trustLine: 'We appreciate your trust in SHIVOHUM GROUP.'
  };
}
