import pReactive from "/components/Component.js";
import { html } from '/libs/htm.js';
import { attrs } from "/utility/htmlUtility.js";
import { get } from "/utility/api.js";

export default class AddinSelect extends pReactive({
	index: 0,
	keys: [],
}) {
	category = null;
	title = null;
	onChange = null;

	get selectedKey() {
		return this.keys[this.index];
	}

	constructor({category, title, onChange}) {
		super();
		this.category = category;
		this.title = title;
		this.onChange = onChange;

		get(`/api/Addin/${category}`)
			.then(x => x.json())
			.then(x => this.keys.push(...x))
			.then(() => this.onChange?.(this.selectedKey));
	}

	render() {
		return html`
		<div class="field label suffix border small">
			<select onChange="${e => this.#onChangeHandler(e)}">
			${this.keys.map((x, i) => html`
				<option ${attrs({selected: i == this.index })}>
					<div>
						${x.pluginName}/${x.addinTypeName}
					</div>
				</option>
			`)}
			</select>
			<label>${this.title}</label>
		</div>`;
	}

	#onChangeHandler(e) {
		this.index = e.target.selectedIndex;
		this.onChange?.(this.selectedKey);
	}
}