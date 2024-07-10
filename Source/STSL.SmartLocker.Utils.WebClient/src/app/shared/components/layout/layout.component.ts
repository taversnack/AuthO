import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  standalone: true,
  imports: [CommonModule],
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  host: { 'class': 'block h-full bg-gray-100' }
})
export class LayoutComponent {
  @Input() fullWidth = false;
  @Input() fullHeight = false;
  @Input() verticallyCentered = false;
  @Input() pageTitle?: string;
  @Input() subtitle?: string;
}
