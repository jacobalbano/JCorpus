import { html } from '/libs/htm.js';

export default function AddinDescription({children}) {
	return html`
	<article class="no-elevate primary fill">
		<div>
			${children}
		</div>
	</article>`;
}