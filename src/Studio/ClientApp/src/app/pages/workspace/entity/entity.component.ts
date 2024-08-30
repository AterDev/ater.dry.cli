import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AdvanceService } from 'src/app/services/advance/advance.service';
import { ProjectStateService } from 'src/app/share/project-state.service';

@Component({
  selector: 'app-entity',
  templateUrl: './entity.component.html',
  styleUrls: ['./entity.component.css']
})
export class EntityComponent {
  isProcessing = false;
  name: string | null = null;
  description: string | null = null;
  entities: string[] | null = null;
  selectedIndex: number | null = null;
  selectedContent: string | null = null;
  namespace: string | null = null;
  projectId: string | null = null;
  editorOptions = {
    theme: 'vs-dark', language: 'csharp', minimap: {
      enabled: false
    }
  };
  constructor(
    public snb: MatSnackBar,
    public router: Router,
    public service: AdvanceService,
    public projectState: ProjectStateService,
    private location: Location
  ) {
    if (projectState.project)
      this.projectId = projectState.project?.id;
  }
  ngOnInit(): void {

  }

  onInit(editor: any) {

  }


  back(): void {
    this.location.back();

  }
}
