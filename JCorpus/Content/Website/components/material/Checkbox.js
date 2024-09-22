import { html } from '/libs/htm.js';
import { blur } from '/utility/htmlUtility.js';

export function bindChecked(self, property) {
	return (e) => self[property] = e.target.checked;
}

export default function Checkbox({ children, selected, onChange }) {
	return html`
	<label class="checkbox icon">
		<input
			type="checkbox"
			checked="${selected}"
			onClick="${blur(onChange)}"
		/>
		<span>
			<i>close</i>
			<i>done</i>
		</span>
		<span>${children}</span>
	</label>`;
}