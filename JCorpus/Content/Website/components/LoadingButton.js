import { html } from "/libs/htm.js";
import { classes } from "/utility/htmlUtility.js";

export default function LoadingButton({ onClick, loading, cancel = 'Cancel', go = 'Go', ...props }) {
	return html`
	<button
		...${props}
		class="${ classes({'tertiary-border tertiary-text': loading }) } border"
		onClick="${onClick}"
	>
		${loading
			? html`<progress class="circle small tertiary-text"></progress> <span>${cancel}</span>`
			: html`<span>${go}</span> <i>arrow_forward</i>`
		}
	</button>
	`;
}