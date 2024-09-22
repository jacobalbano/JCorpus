import objectConfig from "/utility/objectConfig.js";
import pReactive from "/components/Component.js";
import { html } from '/libs/htm.js';
import AddinDescription from '/components/addin/AddinDescription.js'

export default class JsonSearch extends pReactive({
}) {
	render() {
		return html`
		<${AddinDescription} title="Usage">
			<p>Get a static set of data for testing with low iteration time</p>
		<//>
		`;
	}
}