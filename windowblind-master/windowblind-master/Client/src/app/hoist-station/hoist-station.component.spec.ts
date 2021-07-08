import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HoistStationComponent } from './hoist-station.component';

describe('HoistStationComponent', () => {
  let component: HoistStationComponent;
  let fixture: ComponentFixture<HoistStationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HoistStationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HoistStationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
