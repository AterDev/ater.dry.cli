import { Component, OnInit, ViewChild } from '@angular/core';
import { MatAccordion } from '@angular/material/expansion';
import { ActivatedRoute, NavigationStart, Router } from '@angular/router';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {
  events: string[] = [];
  opened = true;
  expanded = true;
  isNodeJs = false;
  @ViewChild(MatAccordion, { static: true }) accordion!: MatAccordion;
  constructor(
    public route: ActivatedRoute,
  ) {
    const project = localStorage.getItem('project');
    if (project) {
      let solutionType = JSON.parse(project).solutionType;
      if (solutionType === 1) {
        this.isNodeJs = true;
      }
    }
  }

  ngOnInit(): void {
  }

  toggle(): void {
    this.opened = !this.opened;
  }

  expandToggle(): void {
    this.expanded = !this.expanded;
    if (this.expanded) {
      this.accordion?.openAll();
    } else {
      this.accordion?.closeAll();
    }
  }
}
