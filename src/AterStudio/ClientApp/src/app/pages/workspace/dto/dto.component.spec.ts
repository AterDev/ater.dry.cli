import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DtoComponent } from './dto.component';

describe('DtoComponent', () => {
  let component: DtoComponent;
  let fixture: ComponentFixture<DtoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DtoComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DtoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
