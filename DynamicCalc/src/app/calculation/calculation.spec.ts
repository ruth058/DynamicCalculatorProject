import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Calculation } from './calculation';

describe('Calculation', () => {
  let component: Calculation;
  let fixture: ComponentFixture<Calculation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Calculation],
    }).compileComponents();

    fixture = TestBed.createComponent(Calculation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
