import { TestBed } from '@angular/core/testing';

import { HiostStationService } from './hiost-station.service';

describe('HiostStationService', () => {
  let service: HiostStationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HiostStationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
