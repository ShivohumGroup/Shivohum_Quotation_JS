export const QUOTE_STEPS = [
  'Company',
  'Client',
  'Project',
  'Scope',
  'Costs',
  'Schedule',
  'Terms',
  'Preview'
] as const;

export type QuoteStep = (typeof QUOTE_STEPS)[number];

export type ProjectType =
  | 'New construction'
  | 'Addition'
  | 'Renovation / remodel'
  | 'Commercial'
  | 'Other';

export type PartyRole = 'Contractor' | 'Architecture';

export interface CostItem {
  id: string;
  name: string;
  unit: string;
  area: string;
  price: number;
}

export interface ScopeItem {
  id: string;
  title: string;
  description: string;
}

export interface Milestone {
  id: string;
  name: string;
  date: string;
  notes: string;
}

export interface PaymentMilestone {
  id: string;
  label: string;
  percent: number;
  dueTrigger: string;
}

export interface ProposalData {
  proposalNumber: string;
  proposalDate: string;
  validUntil: string;
  projectName: string;
  jobLocation: string;
  projectType: ProjectType;
  estimatedStart: string;
  estimatedFinish: string;
  partyRole: PartyRole;
  companyName: string;
  companyContact: string;
  companyAddress: string;
  companyPhone: string;
  companyEmail: string;
  companyWebsite: string;
  licenseNumber: string;
  insuranceInfo: string;
  clientCompany: string;
  clientContact: string;
  clientBillingAddress: string;
  clientSiteAddress: string;
  clientPhone: string;
  clientEmail: string;
  projectSummary: string;
  scopeItems: ScopeItem[];
  milestones: Milestone[];
  exclusions: string;
  materialsNotes: string;
  assumptions: string;
  costItems: CostItem[];
  discount: number;
  taxRate: number;
  paymentTerms: string;
  paymentMilestones: PaymentMilestone[];
  agreementTerms: string;
  warranty: string;
  changeOrderPolicy: string;
  clientSignName: string;
  clientSignDate: string;
  contractorSignName: string;
  contractorSignDate: string;
}

export const INVOICE_STEPS = ['Details', 'Amounts', 'Terms', 'Preview'] as const;
export type InvoiceStep = (typeof INVOICE_STEPS)[number];

export interface InvoiceData {
  invoiceNumber: string;
  invoiceDate: string;
  billTo: string;
  companyName: string;
  companyAddress: string;
  companyPhones: string;
  companyEmail: string;
  totalProjectAmount: number;
  previousPayment: number;
  paymentRequest: number;
  paymentTerms: string[];
  thankYouLine: string;
  trustLine: string;
}

export interface SavedDocument {
  documentId: number;
  kind: 'Quotation' | 'Invoice' | string;
  title: string;
  projectId?: number | null;
  projectName?: string | null;
  payloadJson: string;
  createdOn: string;
  updatedOn?: string | null;
}

export interface SavedDocumentListItem {
  documentId: number;
  kind: string;
  title: string;
  projectId?: number | null;
  projectName?: string | null;
  createdOn: string;
  updatedOn?: string | null;
}
