import { TestBed } from '@angular/core/testing';

import { AlertHub } from './alert-hub';

describe('AlertHub', () => {
  let service: AlertHub;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AlertHub);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
