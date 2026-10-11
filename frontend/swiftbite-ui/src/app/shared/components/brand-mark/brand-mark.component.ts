import { Component } from '@angular/core';

@Component({
  selector: 'app-brand-mark',
  standalone: true,
  template: `
    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"
         style="width: 58%; height: 58%;" role="img" aria-label="SwiftBite">
      <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="1.7"/>
      <path d="M13.2 5.4L8.3 13.3H11.6L10.3 18.6L16.5 10.2H12.9L13.2 5.4Z" fill="currentColor"/>
    </svg>
  `,
})
export class BrandMarkComponent {}
