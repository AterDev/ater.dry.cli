import { Component } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CreateSolutionDto } from 'src/app/share/models/feature/create-solution-dto.model';
import { FeatureService } from 'src/app/share/services/feature.service';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.css']
})
export class CreateComponent {
  addForm!: FormGroup;
  data = {} as CreateSolutionDto;
  isProcess = false;

  constructor(
    service: FeatureService
  ) {

  }


  ngOnInit(): void {
    this.addForm = new FormGroup({

    });
  }

  initForm(): void {

  }

  addSolution(): void {

  }
}
