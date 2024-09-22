import { classes } from "../utility/htmlUtility.js";
import { html } from '/libs/htm.js';
import pReactive from './Component.js';
import Router from '/libs/navigo.js'

export function NavElement({children, icon}) {
	return html`
		<i>${icon}</i>
		<span class="capitalize">${children}</span>
	`;
}

export class AppNav extends pReactive({
	route: '',
	onNavigate: null,
}) {
	router = new Router();

	constructor({onNavigate, children, route}) {
		super();
		this.route = route ?? '';
		this.onNavigate = onNavigate;

		if (!Array.isArray(children))
			children = [children];

		for (const child of children) {
			const { route } = child.props;
			this.router.add(route, () => {
				this.route = route;
				this.router.navigate(route);
				this.onNavigate?.apply(this, [route]);
			});
		}

		this.router.check(this.router.getFragment())
			.listen();
	}

	render({children}) {
		return html`
			<nav class="m l left">
				<header></header>
				${children.map(child => {
					const { route } = child.props;
					return html`
					<a
						onClick="${() => this.router.check(route)}"
						class="${classes({ active: this.route === route})}"
					>
						${child}
					</a>
				`})}
				
			</nav>`;
	}
}
