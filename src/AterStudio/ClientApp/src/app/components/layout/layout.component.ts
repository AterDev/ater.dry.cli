import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationStart, Router } from '@angular/router';
import { LoginService } from 'src/app/auth/login.service';
import { Project } from 'src/app/share/models/project/project.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/share/services/project.service';
import { MatBottomSheet, MatBottomSheetRef } from '@angular/material/bottom-sheet';
@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit {
  isLogin = false;
  isAdmin = false;
  username?: string | null = null;
  type: string | null = null;
  projects = [] as Project[];
  projectName = '';
  version: string | null = null;
  @ViewChild("projectSheet", { static: true }) projectSheet!: TemplateRef<{}>;
  bottomSheetRef!: MatBottomSheetRef<{}>;

  constructor(
    private auth: LoginService,
    private service: ProjectService,
    private projectState: ProjectStateService,
    private bottomSheet: MatBottomSheet,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // this layout is out of router-outlet, so we need to listen router event and change isLogin status
    router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        console.log(event);
        this.isLogin = this.auth.isLogin;
        this.isAdmin = this.auth.isAdmin;
        this.username = this.auth.userName;
      }
    });
    this.route.queryParamMap.subscribe((query) => {
      var type = query.get('type');
      if (type === 'desktop') {
        localStorage.setItem('type', 'desktop');
        this.type = 'desktop'
      }
    });
    this.projectName = this.projectState.project?.displayName || '';
    this.version = this.projectState.version;
  }

  ngOnInit(): void {
    this.isLogin = this.auth.isLogin;
    this.isAdmin = this.auth.isAdmin;
    if (this.isLogin) {
      this.username = this.auth.userName!;
    }
    this.getVersion();
    this.getProjects();
  }

  getVersion(): void {
    this.service.getVersion()
      .subscribe({
        next: (res) => {
          if (res) {
            this.projectState.setVersion(res);
            this.version = res;
          }
        },
      });
  }
  getProjects(): void {
    this.service.list()
      .subscribe({
        next: (res) => {
          if (res) {
            this.projects = res;
          }
        },
      });
  }

  openSolutionSheet() {
    this.bottomSheetRef = this.bottomSheet.open(this.projectSheet, {});
  }

  changeSolution(id: string) {
    const project = this.projects.find(p => p.id == id);
    if (project) {
      this.projectState.setProject(project);
      // reload current page
      window.location.reload();

      this.bottomSheetRef.dismiss();
    }
  }

  login(): void {
    this.router.navigateByUrl('/login')
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/index');
    location.reload();
  }
}
