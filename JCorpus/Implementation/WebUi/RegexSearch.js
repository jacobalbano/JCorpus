import pReactive from "/components/Component.js";
import { html } from '/libs/htm.js';
import AddinDescription from '/components/addin/AddinDescription.js'

export default class RegexSearch extends pReactive({
	search: '思いや(?![ら-ろ])'
}) {
	render() {
		return html`
		<${AddinDescription} title="Usage">
			<p>Use plain text or regular expressions. You can test patterns at <a class="primary-text" href="https://regexr.com/" target="blank">regexr.com</a>.</p>
			<p>Capture groups will be <span class="primary-container">highlighted</span></p>
			<p>Can be slow unless the filesystem has been warmed up.</p>
		<//>
		<div class="field border label">
			<input
				type="search"
				placeholder=" "
				onInput="${({target}) => this.search = target.value }"
				value="${this.search}"
			/>
			<label>Pattern</label>
		</div>
		`;
	}

	getConfig() {
		return { Pattern: this.search };
	}
}