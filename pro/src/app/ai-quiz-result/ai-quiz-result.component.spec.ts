import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AiQuizResultComponent } from './ai-quiz-result.component';

describe('AiQuizResult', () => {
  let component: AiQuizResultComponent;
  let fixture: ComponentFixture<AiQuizResultComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiQuizResultComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AiQuizResultComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
