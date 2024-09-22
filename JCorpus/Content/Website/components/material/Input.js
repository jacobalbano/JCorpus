import { html } from '/libs/htm.js';

export function bindInput(self, property) {
	return (e) => self[property] = e.target.value;
}

export default function Input({children, label, onInput, type="text", error, helper, ...rest}) {
	return html`
	<div class="field border small label ${error ? 'invalid' : null}">
		<input
			type="${type}"
			placeholder=" "
			onInput="${onInput}"
			value="${children}"
			...${rest}
		/>
		<label>${label}</label>
		${helper && html`<span class="helper">${helper}</span>`}
		${error && html`<span class="error">${error}</span>`}
	</div>`;
}

