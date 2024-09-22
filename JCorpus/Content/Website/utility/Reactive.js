export const reactorSym = Symbol('reactor');

export default function Reactive(properties, BaseClass = null) {
	BaseClass ??= class { };
	const clone = () => reactiveClone(properties ?? {});
	properties = clone(); // guard against any modification later
	return class Impl extends BaseClass {
		constructor(...args) {
			super(...args);
			markReactive(this, new Reactor(this, clone()));
		}
	};
}

export function reactiveDictionary() {
	const target = {};
	const reactor = new Reactor(target, {});
	markReactive(target, reactor);

	return new Proxy(target, {
		get(target, prop) {
			return target[prop];
		},
		set(target, key, value) {
			const oldVal = target[key];
			if (isReactive(oldVal))
				reactor.detatch(oldVal);

			target[key] = value;
			if (isReactive(value))
				reactor.attach(key, value);

			reactor.notify();
			return true;
		}
	});
}

const isReactiveSym = Symbol('isReactive');
export function isReactive(obj) {
	return obj && obj[isReactiveSym] === true;
}

export function makeReactive(obj) {
	const self = {};
	markReactive(self, new Reactor(self, obj));
	return self;
}

function reactiveClone(inObject) {
	if (typeof inObject !== "object" || inObject == null)
	  return inObject;
  
	const isArray = Array.isArray(inObject);
	const outObject = isArray ? [] : {}
	for (const key in inObject)
	  outObject[key] = reactiveClone(inObject[key]);
  
	// TODO: do we need to .attach() here?
	return isArray ? ArrayProxy(outObject) : makeReactive(outObject);
}

function markReactive(self, reactor) {
	Object.defineProperty(self, reactorSym, {
		configurable: false,
		enumerable: false,
		writable: false,
		value: reactor
	});

	Object.defineProperty(self, isReactiveSym, {
		configurable: false,
		enumerable: false,
		writable: false,
		value: true
	});
}

class Reactor {
	constructor(target, props) {
		const self = this;
		this.#props = props;
		for (const key of Object.keys(props)) {
			const value = props[key];
			if (isReactive(value)) {
				this.attach(key, value);
			} else if (Array.isArray(value)) {
				const array = ArrayProxy(value);
				props[key] = array;
				this.attach(key, array);
			}

			Object.defineProperty(target, key, {
				get() { return props[key]; },
				set(value) { self.setAndNotify(key, value); },
				enumerable: true,
				configurable: false,
			});
		}
	}
	
	notify() {
		for (const sub of Object.values(this.#setSubscriptions))
			sub(null, null, null);
	}

	setAndNotify(key, value) {
		console.log({key, value});
		const [oldVal, newVal] = [this.#props[key], this.rawset(key, value)];
		for (const sub of Object.values(this.#setSubscriptions))
			sub(key, newVal, oldVal);

		if (isReactive(oldVal)) this.detatch(key, oldVal);
		if (isReactive(newVal)) this.attach(key, newVal);
	}

	subscribe(callback) {
		const key = this.#subKey++;
		this.#setSubscriptions[key] = callback;
		return () => delete this.#setSubscriptions[key];
	}

	rawset(key, value) {
		if (!key) return;
		return this.#props[key] = value;
	}

	attach(key, reactive) {
		if (this.#attached.has(reactive)) return;
		const unsub = reactive[reactorSym].subscribe(() => this.setAndNotify(key, reactive));
		this.#attached.set(reactive, unsub);
	}

	detatch(reactive) {
		if (!this.#attached.has(reactive)) return;
		const unsub = this.#attached.get(reactive);
		unsub();
	}

	#attached = new WeakMap();
	#setSubscriptions = {};
	#subKey = 0;
	#props;
}

const arrayImpls = {
	copyWithin(sub, target, start, end = null) {
		throw 'implement this if you ever use it';
	},
	
	fill(sub, target, start, end = null) {
		throw 'implement this if you ever use it';
	},

	pop(sub) {
		const result = this.pop();
		if (isReactive(result))
			sub.detatch(result);
		return result;
	},

	push(sub, ...items) {
		const result = this.push(...items);
		for (const x of items) {
			if (isReactive(x))
				sub.attach(null, x);
		}

		return result;
	},

	reverse(sub) {
		return this.reverse();
	},
	
	shift(sub) {
		const result = this.shift();
		if (isReactive(result))
			sub.detatch(result);
		return result;
	},

	sort(sub) {
		return this.sort();
	},

	splice(sub, start, deleteCount, ...items) {
		const result = this.splice(start, deleteCount, ...items);
		for (const x of result) {
			if (isReactive(x))
				sub.detatch(x);
		}

		for (const x of items) {
			if (isReactive(x))
				sub.attach(null, x);
		}

		return result;
	},

	unshift(sub, ...items) {
		const result = this.unshift(...items);
		for (const x in items) {
			if (isReactive(x))
				sub.detatch(x);
		}

		return result;
	}
}

function ArrayProxy(target) {
	const reactor = new Reactor(target, {});
	markReactive(target, reactor);

	return new Proxy(target, {
		get(target, prop) {
			const val = target[prop];
			if (typeof val !== 'function')
				return val;

			const delegate = arrayImpls[prop];
			if (delegate != null) {
				return (...args) => {
					const result = delegate.apply(target, [reactor, ...args]);
					reactor.notify();
					return result;
				};
			}

			return val.bind(target);
		},
		set(target, key, value) {
			const oldVal = target[key];
			if (isReactive(oldVal))
				reactor.detatch(oldVal);

			target[key] = value;
			if (isReactive(value))
				reactor.attach(key, value);

			reactor.notify();
			return true;
		}
	});
}