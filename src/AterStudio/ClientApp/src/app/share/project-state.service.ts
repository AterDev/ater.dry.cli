import { Injectable } from '@angular/core';
import { Project } from './models/project/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectStateService {
  project: Project | null = null;
  constructor() {
    this.getProject();
  }
  setProject(project: Project): void {
    this.project = project;
    localStorage.setItem('project', JSON.stringify(project));
    localStorage.setItem('projectId', project.id);
  }
  getProject(): Project | null {
    if (this.project) {
      return this.project;
    }
    const project = localStorage.getItem('project');
    if (project)
      this.project = JSON.parse(project);

    return this.project;
  }

}
