import { Component, Input } from '@angular/core';
import { AppState } from '@/components/navbar/navbar';

@Component({
  selector: 'app-content',
  imports: [],
  templateUrl: './content.html',
  styleUrl: './content.css',
})
export class Content {
  @Input() activeState?: AppState;
}
