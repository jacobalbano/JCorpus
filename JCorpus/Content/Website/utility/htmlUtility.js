import { html } from "/libs/htm.js";

export function blur(callback) {
	return e => {
		if (callback) callback(e);
		e.target.blur();
	}
}

export function preventDefault(callback) {
	return e => {
		if (callback) callback(e);
		e.preventDefault();
	}
}

export function when(condition, ifExpr = () => null, elseExpr = () => null) {
	return condition ? ifExpr() : elseExpr();
}

export function classes(values) {
	return Object.keys(values).filter(x => values[x]);
}

export function attrs(values) {
	const result = {};
	for (const key of Object.keys(values)) {
		const value = values[key];
		if (!(value == null || value === false))
			result[key] = value;
	}

	return html`...${result}`;
}
