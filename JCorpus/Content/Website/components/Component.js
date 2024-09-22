import { Component } from '/libs/preact.js';
import Reactive, { reactorSym } from '/utility/Reactive.js';

/**
 * @template T
 * @param {T} properties
 * @returns {T}
 */
export default function pReactive(properties) {
	return class Impl extends Reactive(properties, Component) {
		constructor() {
			super();
			const reactor = this[reactorSym];
			this.#unsub = reactor.subscribe((key, newVal) => this.setState({ [key]: newVal }));
		}

		componentWillUnmount() {
			if (this.#unsub) this.#unsub();
			this.#unsub = null;
		}

		#unsub;
	}
}
