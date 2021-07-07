import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PackingStationComponent } from './packing-station.component';

describe('PackingStationComponent', () => {
  let component: PackingStationComponent;
  let fixture: ComponentFixture<PackingStationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PackingStationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PackingStationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
