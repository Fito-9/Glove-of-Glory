import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatchRoomComponent } from './match-room.component';

describe('MatchRoomComponent', () => {
  let component: MatchRoomComponent;
  let fixture: ComponentFixture<MatchRoomComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MatchRoomComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MatchRoomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
