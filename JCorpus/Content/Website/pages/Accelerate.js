import { html } from '/libs/htm.js';
import pReactive from '/components/Component.js';
import AddinSelect from '/components/addin/AddinSelect.js';
import AddinComponent from '/components/addin/AddinComponent.js';

export default class Accelerate extends pReactive({
	accelerateAddinKey: null
}) {
	render() {
		return html`
		<div class="s4">
			<h3>Accelerate</h3>
			
			<article class="no-elevate border">
				<${AddinSelect}
					title="Accelerator"
					category="IAccelerator"
					onChange="${key => this.accelerateAddinKey = key}"
				/>

				<${AddinComponent} addinKey=${this.accelerateAddinKey} />
					
			</article>
		</div>
		<div class="s8">
		</div>
		`;
	}
}