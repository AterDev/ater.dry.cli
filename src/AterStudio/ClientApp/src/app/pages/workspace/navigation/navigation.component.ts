import { Component, OnInit, ViewChild } from '@angular/core';
import { MatAccordion } from '@angular/material/expansion';
import { ActivatedRoute, NavigationStart, Router } from '@angular/router';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {
  events: string[] = [];
  opened = true;
  expanded = true;
  @ViewChild(MatAccordion, { static: true }) accordion!: MatAccordion;
  constructor(
    public route: ActivatedRoute,
  ) {
  }

  ngOnInit(): void {
    if (this.expanded) {
      this.accordion?.openAll();
    } else {
      this.accordion?.closeAll();
    }
  }

  ngAfterViewInit(): void {


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
