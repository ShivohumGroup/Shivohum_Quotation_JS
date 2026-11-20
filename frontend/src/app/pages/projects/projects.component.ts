import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Project, ProjectWrite } from '../../core/models';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, RouterLink],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss'
})
export class ProjectsComponent implements OnInit {
  projects: Project[] = [];
  editingId: number | null = null;
  error = '';
  saving = false;

  private api = inject(ApiService);
  private fb = inject(FormBuilder);

  form = this.fb.nonNullable.group({
    projectName: ['', Validators.required],
    clientName: [''],
    status: ['Active'],
    startDate: [''],
    contractAmount: [''],
    notes: ['']
  });

  get isEdit(): boolean {
    return this.editingId !== null;
  }

  ngOnInit() {
    this.load();
  }

  load() {
    this.api.getProjects().subscribe((projects) => (this.projects = projects));
  }

  edit(project: Project) {
    this.editingId = project.projectId;
    this.error = '';
    this.form.reset({
      projectName: project.projectName,
      clientName: project.clientName ?? '',
      status: project.status || 'Active',
      startDate: project.startDate ? project.startDate.slice(0, 10) : '',
      contractAmount: project.contractAmount != null ? String(project.contractAmount) : '',
      notes: project.notes ?? ''
    });
  }

  cancelEdit() {
    this.editingId = null;
    this.error = '';
    this.form.reset({
      projectName: '',
      clientName: '',
      status: 'Active',
      startDate: '',
      contractAmount: '',
      notes: ''
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const contractAmount = value.contractAmount === '' ? null : Number(value.contractAmount);
    if (contractAmount != null && (Number.isNaN(contractAmount) || contractAmount < 0)) {
      this.error = 'Client budget must be zero or more.';
      return;
    }

    const body: ProjectWrite = {
      projectName: value.projectName,
      clientName: value.clientName || null,
      status: value.status,
      startDate: value.startDate || null,
      contractAmount,
      notes: value.notes || null
    };

    this.saving = true;
    this.error = '';
    const request = this.editingId
      ? this.api.updateProject(this.editingId, body)
      : this.api.createProject(body);

    request.subscribe({
      next: () => {
        this.saving = false;
        this.cancelEdit();
        this.load();
      },
      error: (err) => {
        this.saving = false;
        this.error = err.error?.message || 'Could not save the project.';
      }
    });
  }
}
