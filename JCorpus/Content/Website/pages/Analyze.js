import { html } from '/libs/htm.js';
import pReactive from '/components/Component.js';
import AddinSelect from '/components/addin/AddinSelect.js';
import { createRef } from '/libs/preact.js';
import AddinComponent from '/components/addin/AddinComponent.js';

export default class Accelerate extends pReactive({
	analysisAddinKey: null
}) {
	outputRef = createRef();

	render() {
		return html`
		<div class="s4">
			<h3>Analyze</h3>
			
			<article class="no-elevate border">
				<${AddinSelect}
					title="Analysis"
					category="IAnalysis"
					onChange="${key => this.analysisAddinKey = key}"
				/>

				<${AddinComponent} addinKey=${this.analysisAddinKey} />
			</article>
		</div>
		<div class="s8">
		</div>
		`;
	}
}