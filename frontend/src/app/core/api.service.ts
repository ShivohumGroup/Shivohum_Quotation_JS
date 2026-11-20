import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import {
  ClientPayment,
  ClientPaymentWrite,
  DashboardData,
  DashboardSummary,
  ImportResult,
  Lookup,
  PagedResult,
  Project,
  ProjectWrite,
  SavedDocument,
  SavedDocumentListItem,
  Transaction,
  TransactionWrite
} from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  getTransactions(filters: {
    from?: string;
    to?: string;
    projectId?: number | null;
    type?: string;
    search?: string;
    page?: number;
    pageSize?: number;
  }) {
    let params = new HttpParams();
    if (filters.from) params = params.set('from', filters.from);
    if (filters.to) params = params.set('to', filters.to);
    if (filters.projectId) params = params.set('projectId', filters.projectId);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.search) params = params.set('search', filters.search);
    params = params.set('page', String(filters.page ?? 1));
    params = params.set('pageSize', String(filters.pageSize ?? 50));
    return this.http.get<PagedResult<Transaction>>(`${environment.apiUrl}/transactions`, { params });
  }

  getTransaction(id: number) {
    return this.http.get<Transaction>(`${environment.apiUrl}/transactions/${id}`);
  }

  createTransaction(body: TransactionWrite) {
    return this.http.post<Transaction>(`${environment.apiUrl}/transactions`, body);
  }

  updateTransaction(id: number, body: TransactionWrite) {
    return this.http.put<Transaction>(`${environment.apiUrl}/transactions/${id}`, body);
  }

  deleteTransaction(id: number) {
    return this.http.delete(`${environment.apiUrl}/transactions/${id}`);
  }

  getProjects() {
    return this.http.get<Project[]>(`${environment.apiUrl}/projects`);
  }

  createProject(body: ProjectWrite) {
    return this.http.post<Project>(`${environment.apiUrl}/projects`, body);
  }

  updateProject(id: number, body: ProjectWrite) {
    return this.http.put<Project>(`${environment.apiUrl}/projects/${id}`, body);
  }

  getProject(id: number) {
    return this.http.get<Project>(`${environment.apiUrl}/projects/${id}`);
  }

  getProjectPayments(projectId: number) {
    return this.http.get<ClientPayment[]>(`${environment.apiUrl}/projects/${projectId}/payments`);
  }

  createProjectPayment(projectId: number, body: ClientPaymentWrite) {
    return this.http.post<ClientPayment>(`${environment.apiUrl}/projects/${projectId}/payments`, body);
  }

  updateProjectPayment(projectId: number, paymentId: number, body: ClientPaymentWrite) {
    return this.http.put<ClientPayment>(
      `${environment.apiUrl}/projects/${projectId}/payments/${paymentId}`,
      body
    );
  }

  deleteProjectPayment(projectId: number, paymentId: number) {
    return this.http.delete(`${environment.apiUrl}/projects/${projectId}/payments/${paymentId}`);
  }

  getCategories(type?: string) {
    let params = new HttpParams();
    if (type) params = params.set('type', type);
    return this.http.get<Lookup[]>(`${environment.apiUrl}/lookups/categories`, { params });
  }

  getPaymentModes() {
    return this.http.get<Lookup[]>(`${environment.apiUrl}/lookups/payment-modes`);
  }

  getAccounts() {
    return this.http.get<Lookup[]>(`${environment.apiUrl}/lookups/accounts`);
  }

  getDashboard(preset: string, from?: string, to?: string) {
    let params = new HttpParams().set('preset', preset);
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<DashboardData>(`${environment.apiUrl}/dashboard`, { params });
  }

  getDashboardSummary(preset = 'thisYear') {
    return this.http.get<DashboardSummary>(`${environment.apiUrl}/dashboard/summary`, {
      params: new HttpParams().set('preset', preset)
    });
  }

  importExcel(file: File) {
    const data = new FormData();
    data.append('file', file, file.name);
    return this.http.post<ImportResult>(`${environment.apiUrl}/import/excel`, data);
  }

  getDocuments(kind?: string) {
    let params = new HttpParams();
    if (kind) params = params.set('kind', kind);
    return this.http.get<SavedDocumentListItem[]>(`${environment.apiUrl}/documents`, { params });
  }

  getDocument(id: number) {
    return this.http.get<SavedDocument>(`${environment.apiUrl}/documents/${id}`);
  }

  createDocument(body: { kind: string; title: string; projectId?: number | null; payloadJson: string }) {
    return this.http.post<SavedDocument>(`${environment.apiUrl}/documents`, body);
  }

  updateDocument(
    id: number,
    body: { kind: string; title: string; projectId?: number | null; payloadJson: string }
  ) {
    return this.http.put<SavedDocument>(`${environment.apiUrl}/documents/${id}`, body);
  }

  deleteDocument(id: number) {
    return this.http.delete(`${environment.apiUrl}/documents/${id}`);
  }
}
