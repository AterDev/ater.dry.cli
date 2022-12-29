import { Injectable } from '@angular/core';
import { Project } from './models/project/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectStateService {
  project?: Project;
  constructor() {
  }
  setProject(project: Project): void {
    this.project = project;
  }
}
