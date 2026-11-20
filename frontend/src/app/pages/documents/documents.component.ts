import { Component, HostListener, OnInit, ViewChild, ViewEncapsulation, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Project } from '../../core/models';
import { createEmptyInvoice, createEmptyProposal, hydrateInvoice, hydrateProposal } from './document.defaults';
import { INVOICE_STEPS, InvoiceData, ProposalData, QUOTE_STEPS, SavedDocumentListItem } from './document.models';
import { INVOICE_WATERMARK, exportProposalPdf } from './export-pdf';
import { InvoiceFormComponent } from './invoice-form.component';
import { InvoicePreviewComponent } from './invoice-preview.component';
import { ProposalFormComponent } from './proposal-form.component';
import { ProposalPreviewComponent } from './proposal-preview.component';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [FormsModule, ProposalFormComponent, ProposalPreviewComponent, InvoiceFormComponent, InvoicePreviewComponent],
  templateUrl: './documents.component.html',
  styleUrl: './documents.component.scss',
  encapsulation: ViewEncapsulation.None
})
export class DocumentsComponent implements OnInit {
  private api = inject(ApiService);

  @ViewChild('quotePreviewCmp') quotePreviewCmp?: ProposalPreviewComponent;
  @ViewChild('invoicePreviewCmp') invoicePreviewCmp?: InvoicePreviewComponent;

  mode: 'quotation' | 'invoice' = 'quotation';
  quoteSteps = QUOTE_STEPS;
  invoiceSteps = INVOICE_STEPS;
  quoteStep = 0;
  invoiceStep = 0;
  proposal = createEmptyProposal();
  invoice = createEmptyInvoice();
  projects: Project[] = [];
  saved: SavedDocumentListItem[] = [];
  documentId: number | null = null;
  projectId: number | null = null;
  saving = false;
  deletingId: number | null = null;
  downloading = false;
  saveMsg = '';
  dirty = false;
  private snapshot = '';

  get quotePreview() {
    return this.quoteSteps[this.quoteStep] === 'Preview';
  }

  get invoicePreview() {
    return this.invoiceSteps[this.invoiceStep] === 'Preview';
  }

  ngOnInit() {
    this.api.getProjects().subscribe((items) => (this.projects = items));
    this.refreshSaved();
    this.markClean();
  }

  canLeave() {
    if (!this.dirty) return true;
    return confirm('You have unsaved quotation/invoice changes. Leave anyway?');
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent) {
    if (!this.dirty) return;
    event.preventDefault();
    event.returnValue = true;
  }

  setMode(mode: 'quotation' | 'invoice') {
    if (mode === this.mode) return;
    if (!this.confirmDiscard()) return;
    this.mode = mode;
    this.documentId = null;
    this.projectId = null;
    this.saveMsg = '';
    this.saved = [];
    if (mode === 'quotation') {
      this.proposal = createEmptyProposal();
      this.quoteStep = 0;
    } else {
      this.invoice = createEmptyInvoice();
      this.invoiceStep = 0;
    }
    this.markClean();
    this.refreshSaved();
  }

  newDoc() {
    if (!this.confirmDiscard()) return;
    this.documentId = null;
    this.projectId = null;
    this.saveMsg = '';
    if (this.mode === 'quotation') {
      this.proposal = createEmptyProposal();
      this.quoteStep = 0;
    } else {
      this.invoice = createEmptyInvoice();
      this.invoiceStep = 0;
    }
    this.markClean();
  }

  resetQuote() {
    if (!this.confirmDiscard()) return;
    this.proposal = createEmptyProposal();
    this.markDirty();
  }

  resetInvoice() {
    if (!this.confirmDiscard()) return;
    this.invoice = createEmptyInvoice();
    this.markDirty();
  }

  onProposalChange(data: ProposalData) {
    this.proposal = data;
    this.markDirty();
  }

  onInvoiceChange(data: InvoiceData) {
    this.invoice = data;
    this.markDirty();
  }

  onProjectChange(projectId: number | null) {
    this.projectId = projectId;
    this.markDirty();
  }

  refreshSaved() {
    const kind = this.mode === 'quotation' ? 'Quotation' : 'Invoice';
    this.api.getDocuments(kind).subscribe({
      next: (items) => (this.saved = items),
      error: () => (this.saved = [])
    });
  }

  loadSaved(id: number) {
    if (id === this.documentId) return;
    if (!this.confirmDiscard()) return;
    this.api.getDocument(id).subscribe({
      next: (doc) => {
        this.documentId = doc.documentId;
        this.projectId = doc.projectId ?? null;
        const payload = JSON.parse(doc.payloadJson || '{}');
        if (doc.kind === 'Invoice') {
          this.mode = 'invoice';
          this.invoice = hydrateInvoice(payload);
          this.invoiceStep = 0;
        } else {
          this.mode = 'quotation';
          this.proposal = hydrateProposal(payload);
          this.quoteStep = 0;
        }
        this.saveMsg = '';
        this.markClean();
        this.refreshSaved();
      },
      error: () => {
        this.saveMsg = 'Could not open this document.';
      }
    });
  }

  deleteSaved(item: SavedDocumentListItem, event: Event) {
    event.stopPropagation();
    const ok = confirm(`Delete “${item.title}”? This cannot be undone.`);
    if (!ok) return;
    this.deletingId = item.documentId;
    this.api.deleteDocument(item.documentId).subscribe({
      next: () => {
        this.deletingId = null;
        if (this.documentId === item.documentId) {
          this.documentId = null;
          this.saveMsg = '';
          if (this.mode === 'quotation') {
            this.proposal = createEmptyProposal();
            this.quoteStep = 0;
          } else {
            this.invoice = createEmptyInvoice();
            this.invoiceStep = 0;
          }
          this.markClean();
        }
        this.refreshSaved();
      },
      error: () => {
        this.deletingId = null;
        this.saveMsg = 'Could not delete.';
      }
    });
  }

  save() {
    const kind = this.mode === 'quotation' ? 'Quotation' : 'Invoice';
    const payload = this.mode === 'quotation' ? this.proposal : this.invoice;
    const title =
      this.mode === 'quotation'
        ? this.proposal.proposalNumber || this.proposal.projectName || 'Quotation draft'
        : this.invoice.invoiceNumber || this.invoice.billTo || 'Invoice draft';

    this.saving = true;
    this.saveMsg = '';
    const body = {
      kind,
      title,
      projectId: this.projectId,
      payloadJson: JSON.stringify(payload)
    };
    const request = this.documentId
      ? this.api.updateDocument(this.documentId, body)
      : this.api.createDocument(body);

    request.subscribe({
      next: (doc) => {
        this.saving = false;
        this.documentId = doc.documentId;
        this.saveMsg = `Saved ${this.formatTime(new Date().toISOString())}`;
        this.markClean();
        this.refreshSaved();
      },
      error: (err) => {
        this.saving = false;
        this.saveMsg = err.error?.message || 'Could not save.';
      }
    });
  }

  formatListDate(value: string) {
    const raw = value;
    const d = new Date(raw);
    if (Number.isNaN(d.getTime())) return raw;
    return d.toLocaleString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  async downloadQuote() {
    const el = this.quotePreviewCmp?.nativeSheet();
    if (!el) return;
    this.downloading = true;
    try {
      const safeName =
        this.proposal.projectName.trim().replace(/[^\w\s-]/g, '').replace(/\s+/g, '-') || 'proposal';
      await exportProposalPdf(el, `${this.proposal.proposalNumber || 'SHG'}-${safeName}.pdf`);
    } catch {
      alert('PDF download failed. Please try again, or use Print (Ctrl+P) and Save as PDF.');
    } finally {
      this.downloading = false;
    }
  }

  async downloadInvoice() {
    const el = this.invoicePreviewCmp?.nativeSheet();
    if (!el) return;
    this.downloading = true;
    try {
      const safeName =
        this.invoice.billTo.trim().replace(/[^\w\s-]/g, '').replace(/\s+/g, '-') || 'invoice';
      await exportProposalPdf(el, `INV-${this.invoice.invoiceNumber || 'draft'}-${safeName}.pdf`, INVOICE_WATERMARK);
    } catch {
      alert('PDF download failed. Please try again, or use Print (Ctrl+P) and Save as PDF.');
    } finally {
      this.downloading = false;
    }
  }

  private confirmDiscard() {
    if (!this.dirty) return true;
    return confirm('You have unsaved changes. Discard them?');
  }

  private markDirty() {
    this.dirty = this.snapshot !== this.currentSnapshot();
  }

  private markClean() {
    this.snapshot = this.currentSnapshot();
    this.dirty = false;
  }

  private currentSnapshot() {
    return JSON.stringify({
      mode: this.mode,
      projectId: this.projectId,
      proposal: this.proposal,
      invoice: this.invoice
    });
  }

  private formatTime(value: string) {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return '';
    return d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
  }
}
