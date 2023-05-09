import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { EntityService } from 'src/app/share/services/entity.service';

@Component({
  selector: 'app-entity',
  templateUrl: './entity.component.html',
  styleUrls: ['./entity.component.css']
})
export class EntityComponent {
  isProcessing = false;


  constructor(
    public snb: MatSnackBar,
    public router: Router,
    public service: EntityService,
    private location: Location
  ) {


  }

  ngOnInit(): void {

  }
  back(): void {
    this.location.back();

  }
}
