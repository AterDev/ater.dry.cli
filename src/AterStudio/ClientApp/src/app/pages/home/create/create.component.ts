import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { CacheType } from 'src/app/share/models/enum/cache-type.model';
import { DBType } from 'src/app/share/models/enum/dbtype.model';
import { ProjectType } from 'src/app/share/models/enum/project-type.model';
import { CreateSolutionDto } from 'src/app/share/models/feature/create-solution-dto.model';
import { ModuleInfo } from 'src/app/share/models/feature/module-info.model';
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
  isLoading = true;
  DBType = DBType;
  CacheType = CacheType;
  ProjectType = ProjectType;
  defaultModules: ModuleInfo[] = [];

  constructor(
    private service: FeatureService,
    private snb: MatSnackBar,
    private router: Router
  ) {

  }

  get name() { return this.addForm.get('name'); }
  get path() { return this.addForm.get('path'); }
  get isLight() { return this.addForm.get('isLight'); }
  get dbType() { return this.addForm.get('dbType'); }
  get cacheType() { return this.addForm.get('cacheType'); }
  get defaultPassword() { return this.addForm.get('defaultPassword'); }
  get hasTenant() { return this.addForm.get('hasTenant'); }
  get hasIdentityServer() { return this.addForm.get('hasIdentityServer'); }
  get hasTaskManager() { return this.addForm.get('hasTaskManager'); }
  get commandDbConnStrings() { return this.addForm.get('commandDbConnStrings'); }
  get queryDbConnStrings() { return this.addForm.get('queryDbConnStrings'); }
  get cacheConnStrings() { return this.addForm.get('cacheConnStrings'); }
  get cacheInstanceName() { return this.addForm.get('cacheInstanceName'); }
  get projectType() { return this.addForm.get('projectType'); }

  ngOnInit(): void {
    this.getDefaultModules();
  }

  getDefaultModules(): void {
    this.service.getDefaultModules()
      .subscribe({
        next: (res) => {
          if (res) {
            this.defaultModules = res;
            this.initForm();
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  initForm(): void {
    this.addForm = new FormGroup({
      name: new FormControl('', [Validators.required, Validators.maxLength(50)]),
      path: new FormControl('', [Validators.required, Validators.maxLength(50)]),
      isLight: new FormControl(false, [Validators.required]),
      dbType: new FormControl(DBType.PostgreSQL, []),
      cacheType: new FormControl(CacheType.Redis, []),
      defaultPassword: new FormControl('Hello.Net', []),
      hasTenant: new FormControl(false, []),
      hasIdentityServer: new FormControl(false, []),
      hasTaskManager: new FormControl(false, []),
      commandDbConnStrings: new FormControl('Server=localhost;Port=5432;Database=MyProjectName;User Id=postgres;Password=root;', [Validators.maxLength(300)]),
      queryDbConnStrings: new FormControl('Server=localhost;Port=5432;Database=MyProjectName;User Id=postgres;Password=root;', [Validators.maxLength(300)]),
      cacheConnStrings: new FormControl('localhost:6379', [Validators.maxLength(200)]),
      cacheInstanceName: new FormControl('Dev', [Validators.maxLength(60)]),
      projectType: new FormControl(ProjectType.WebAPI, [Validators.required]),
      modules: new FormControl(['SystemMod'], [])
    });

    this.name?.valueChanges.subscribe(val => {
      this.commandDbConnStrings?.setValue(`Server=localhost;Port=5432;Database=${val};User Id=postgres;Password=root;`);
      this.queryDbConnStrings?.setValue(`Server=localhost;Port=5432;Database=${val};User Id=postgres;Password=root;`);
    });
  }

  addSolution(): void {
    if (this.addForm.valid) {
      const data = this.addForm.value as CreateSolutionDto;
      this.isProcess = true;
      this.service.createNewSolution(data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('创建成功');
              this.router.navigateByUrl('/');
            } else {
              this.snb.open('');
            }
          },
          error: (error) => {
            // this.snb.open(error.detail);
            this.isProcess = false;
          },
          complete: () => {
            this.isProcess = false;
          }
        });
    }
  }
}
