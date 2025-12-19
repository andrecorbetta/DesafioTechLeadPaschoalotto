import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TitulosAtrasados } from './titulos-atrasados';

describe('TitulosAtrasados', () => {
  let component: TitulosAtrasados;
  let fixture: ComponentFixture<TitulosAtrasados>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TitulosAtrasados]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TitulosAtrasados);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
