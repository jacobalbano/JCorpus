import pReactive from "../Component.js";
import { html } from "/libs/htm.js";
import { forwardRef } from "/libs/preact/compat.js";

const AddinComponent = forwardRef((props, ref) => {
	return html`<${Impl} ...${props} forwardRef=${ref} />`;
})

export default AddinComponent;

function ErrorComponent({error}) {
	return html`<code style="color: red">${error.message}</code>`;
}

function Nothing() {
	return html``;
}

class Impl extends pReactive() {
	#displayClass = Nothing;
	#addinKey;

	constructor({addinKey}) {
		super();
		this.#tryLoading(addinKey);
	}

	#tryLoading(newAddinKey) {
		const oldAddinKey = this.#addinKey;
		this.#addinKey = newAddinKey;
		load(oldAddinKey)
			.catch(e => this.#displayClass = () => html`<${ErrorComponent} error=${e} />`)
			.then(x => {
				if (!x) return;
				this.#displayClass = x;
				this.forceUpdate();
			});

		async function load(oldAddinKey) {
			if (keysEqual(newAddinKey, oldAddinKey)) return;
			if (newAddinKey == null) return Nothing;
			try {
				const { pluginName, addinTypeName } = newAddinKey;
				const comp = await import(`/api/UI/${pluginName}/${addinTypeName}.js`);
				return comp.default;
			} catch (e) {
				if (e instanceof TypeError) return Nothing;
				console.error(e);
				throw e;
			}
		}

		function keysEqual(key1, key2) {
			if (key1 == null && key2 == null) return true;
			if ((key1 == null) !== (key2 == null)) return false;
			if (key1.pluginName !== key2.pluginName) return false;
			if (key1.addinTypeName !== key2.addinTypeName) return false;
			return true;
		}
	}

	render(attrs) {
		const { addinKey, forwardRef, ...props } = attrs;
		this.#tryLoading(addinKey);

		return html`<${this.#displayClass} ref=${forwardRef} ...${props} />`;
	}
}