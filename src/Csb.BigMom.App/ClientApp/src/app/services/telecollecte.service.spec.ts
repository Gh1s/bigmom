import { TestBed } from '@angular/core/testing';

import { TelecollecteService } from './telecollecte.service';

describe('TelecollecteService', () => {
  let service: TelecollecteService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TelecollecteService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
