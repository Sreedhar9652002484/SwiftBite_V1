import { Component } from '@angular/core';

/**
 * SwiftBite mark: a plate with a bite taken out of it ("bite") and a lightning
 * bolt cut through the middle ("swift"). Uses currentColor, so it takes the
 * colour of whatever tile or text it sits in. Keep in sync with public/favicon.svg.
 */
@Component({
  selector: 'app-brand-mark',
  standalone: true,
  template: `
    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"
         style="width: 62%; height: 62%;" role="img" aria-label="SwiftBite">
      <defs>
        <mask id="sb-mark-mask" maskUnits="userSpaceOnUse" x="0" y="0" width="24" height="24">
          <rect width="24" height="24" fill="#fff"/>
          <circle cx="19.4" cy="5.2" r="2.8" fill="#000"/>
          <circle cx="21.5" cy="9.8" r="2" fill="#000"/>
          <circle cx="14.8" cy="2.9" r="2" fill="#000"/>
          <path d="M13.4 6.4 7.9 13.5h3.7l-1.2 4.9 6-7.7h-3.8l.8-4.3Z" fill="#000"/>
        </mask>
      </defs>
      <circle cx="12" cy="12" r="9.5" fill="currentColor" mask="url(#sb-mark-mask)"/>
    </svg>
  `,
})
export class BrandMarkComponent {}
