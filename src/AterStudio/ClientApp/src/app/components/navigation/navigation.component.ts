import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'admin-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {
  events: string[] = [];
  opened = true;

  constructor() { }

  ngOnInit(): void {
  }

  toggle(): void {
    this.opened = !this.opened;
  }

}
