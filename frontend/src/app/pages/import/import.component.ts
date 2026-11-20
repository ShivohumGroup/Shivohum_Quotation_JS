import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { ImportResult } from '../../core/models';

@Component({
  selector: 'app-import',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './import.component.html',
  styleUrl: './import.component.scss'
})
export class ImportComponent {
  file: File | null = null;
  result: ImportResult | null = null;
  error = '';
  loading = false;

  constructor(private api: ApiService) {}

  onFile(event: Event) {
    const input = event.target as HTMLInputElement;
    this.file = input.files?.[0] ?? null;
    this.result = null;
    this.error = '';
  }

  upload() {
    if (!this.file) {
      this.error = 'Choose an .xlsx file first.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.api.importExcel(this.file).subscribe({
      next: (result) => {
        this.result = result;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Import failed. Use an .xlsx sheet with the tracker columns.';
      }
    });
  }
}
