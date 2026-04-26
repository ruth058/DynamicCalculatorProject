import { TestBed } from '@angular/core/testing';

import { Calculation } from './calculation';

describe('Calculation', () => {
  let service: Calculation;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Calculation);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
