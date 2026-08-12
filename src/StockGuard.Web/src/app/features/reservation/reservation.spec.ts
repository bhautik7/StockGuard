import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReservationPage } from './reservation';

describe('Reservation', () => {
  let component: ReservationPage;
  let fixture: ComponentFixture<ReservationPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReservationPage],
    }).compileComponents();

    fixture = TestBed.createComponent(ReservationPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
