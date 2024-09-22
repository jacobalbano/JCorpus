import { html } from '/libs/htm.js';
import { attrs } from "/utility/htmlUtility.js";

export function bindIndex(self, property) {
	return (e) => self[property] = e.target.selectedIndex;
}

export default function Select({ children, selectedIndex, title, onChange }) {
	if (!Array.isArray(children))
		children = [children];

	return html`
	<div class="field label suffix border small">
		<select onChange="${onChange}">
			${children.map((x, i) => html`
			<option ${attrs({selected: i == selectedIndex })}>
				${x}
			</option>
			`)}
		</select>
		<label>${title}</label>
	</div>`;
}