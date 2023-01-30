import { Component, OnInit, ViewChild } from '@angular/core';
import { MatAccordion } from '@angular/material/expansion';

@Component({
  selector: 'admin-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {
  events: string[] = [];
  opened = true;
  expanded = true;
  @ViewChild(MatAccordion, { static: true }) accordion!: MatAccordion;
  constructor() { }

  ngOnInit(): void {
    if (this.expanded) {
      this.accordion?.openAll();
    } else {
      this.accordion?.closeAll();
    }
  }

  toggle(): void {
    this.opened = !this.opened;
  }

}
