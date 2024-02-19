import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Json2TypeComponent } from './json2-type.component';

describe('Json2TypeComponent', () => {
  let component: Json2TypeComponent;
  let fixture: ComponentFixture<Json2TypeComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [Json2TypeComponent]
    });
    fixture = TestBed.createComponent(Json2TypeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
