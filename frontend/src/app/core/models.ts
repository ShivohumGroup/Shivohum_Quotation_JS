export interface LoginResponse {
  token: string;
  userId: number;
  fullName: string;
  username: string;
  role: string;
}

export interface Lookup {
  id: number;
  name: string;
  extra?: string | null;
}

export interface Project {
  projectId: number;
  projectName: string;
  clientName?: string | null;
  status: string;
  startDate?: string | null;
  contractAmount?: number | null;
  receivedAmount: number;
  dueAmount?: number | null;
  expenseAmount: number;
  profitAmount: number;
  notes?: string | null;
  isActive: boolean;
}

export interface ProjectWrite {
  projectName: string;
  clientName?: string | null;
  status: string;
  startDate?: string | null;
  contractAmount?: number | null;
  notes?: string | null;
}

export interface ClientPayment {
  clientPaymentId: number;
  projectId: number;
  paymentDate: string;
  amount: number;
  paymentModeId: number;
  paymentModeName: string;
  accountId?: number | null;
  accountName?: string | null;
  referenceNo?: string | null;
  notes?: string | null;
  createdByName: string;
  createdOn: string;
}

export interface ClientPaymentWrite {
  paymentDate: string;
  amount: number;
  paymentModeId: number;
  accountId?: number | null;
  referenceNo?: string | null;
  notes?: string | null;
}

export interface Transaction {
  transactionId: number;
  transactionDate: string;
  type: 'Income' | 'Expense' | string;
  categoryId: number;
  categoryName: string;
  description: string;
  projectId?: number | null;
  projectName?: string | null;
  paymentModeId: number;
  paymentModeName: string;
  amount: number;
  accountId?: number | null;
  accountName?: string | null;
  notes?: string | null;
  createdByName: string;
  createdOn: string;
}

export interface TransactionWrite {
  transactionDate: string;
  type: string;
  categoryId: number;
  description: string;
  projectId?: number | null;
  paymentModeId: number;
  amount: number;
  accountId?: number | null;
  notes?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ImportResult {
  importedCount: number;
  skippedCount: number;
  errors: string[];
}

export interface DashboardSummary {
  totalIncome: number;
  totalExpense: number;
  netBalance: number;
  thisMonthBalance: number;
}

export interface ProjectProfit {
  projectId: number | null;
  projectName: string;
  contractAmount?: number | null;
  receivedAmount: number;
  dueAmount?: number | null;
  income: number;
  expense: number;
  profit: number;
}

export interface MonthlySummary {
  year: number;
  month: number;
  label: string;
  income: number;
  expense: number;
  balance: number;
}

export interface CategoryTotal {
  categoryId: number;
  categoryName: string;
  expense: number;
}

export interface DashboardData {
  preset: string;
  from: string;
  to: string;
  summary: DashboardSummary;
  projects: ProjectProfit[];
  months: MonthlySummary[];
  categories: CategoryTotal[];
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

export interface SavedDocument extends SavedDocumentListItem {
  payloadJson: string;
}
