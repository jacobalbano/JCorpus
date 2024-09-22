import pReactive from '/components/Component.js';

export class Switch extends pReactive() {
	render({on, children}) {
		if (!Array.isArray(children))
			children = [children];
		
		let _default = null;
		for (const child of children) {
			if (child.type === Default)
				_default = child;
			else if (child.type === Case && child.props?.when == on) {
				child.props.when = true;
				return child;
			}
		}

		return _default;
	}
}

export class Case extends pReactive() {
	render({children, when}) {
		if (!when) return null;
		if (typeof children === 'function') return children();
		return children; 
	}
}

export function Default({children}) {
	return children;
}