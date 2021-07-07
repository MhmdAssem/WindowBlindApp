import { TestBed } from '@angular/core/testing';

import { PackingStationService } from './packing-station.service';

describe('PackingStationService', () => {
  let service: PackingStationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PackingStationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
