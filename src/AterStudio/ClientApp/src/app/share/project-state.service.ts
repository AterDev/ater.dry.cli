import { Injectable } from '@angular/core';
import { Project } from './models/project/project.model';
import { EntityInfo } from './models/entity-info.model';
import { EntityFile } from './models/entity/entity-file.model';

@Injectable({
  providedIn: 'root'
})

export class ProjectStateService {
  project: Project | null = null;
  version: string | null = null;
  currentEntity: EntityFile | null = null;
  constructor() {
    this.getProject();
    this.getVersion();
  }

  setVersion(version: string): void {
    localStorage.setItem('version', version);
  }
  getVersion(): string | null {
    if (this.version) {
      return this.version;
    }
    const version = localStorage.getItem('version');
    if (version)
      this.version = version;
    return this.version;
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
