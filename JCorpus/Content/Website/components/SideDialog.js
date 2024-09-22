import { html } from '/libs/htm.js';
import pReactive from './Component.js';
import { classes, when} from "../utility/htmlUtility.js";

export default class SideDialog extends pReactive({
	active: false,
}) {
	render({title, children}) {
		const active = this.active;
		return html`
		<div class="overlay ${classes({ active })}" onClick="${() => this.active = false }"></div>
		<dialog class="right large-width ${classes({ active })}">
			<header>
				<nav>
					<h5>${title}</h5>
					<div class="max"></div>
					<button class="circle transparent"
						onClick="${() => this.active = false}">
						<i>close</i>
					</button>
				</nav>
			</header>
			${when(this.active, () => html`
			<div>
				${children}
			</div>
			`)}
		</dialog>
		`;
	}
}