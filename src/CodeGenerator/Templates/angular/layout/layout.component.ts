import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit {

  isLogin = true;
  username = '';
  constructor(
  ) {
  }
  ngOnInit(): void {
   
  }
  login(): void {
  }

  logout(): void {

  }
}
