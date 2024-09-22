import pReactive from "/components/Component.js";
import { html } from '/libs/htm.js';
import AddinDescription from '/components/addin/AddinDescription.js'
import Input, { bindInput } from "/components/material/Input.js";

export default class Downloader extends pReactive({
	author: null,
	title: null,
}) {
	getConfig() {
		const { author, title } = this;
		return { author, title };
	}

	render({ readonly }) {
		return html`
		<${AddinDescription} title="Usage">
			<p>Download works from <a href="https://www.aozora.gr.jp/">Aozora Bunko</a></p>
		<//>
		<${Input}
			readonly=${readonly}
			label="Author"
			onInput="${bindInput(this, 'author')}"
		>${this.author}<//>
		
		<${Input}
			label="Title"
			onInput="${bindInput(this, 'title')}"
		>${this.title}<//>
		`;
	}
}
