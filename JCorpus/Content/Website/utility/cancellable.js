export const cancelled = 'async operation cancelled';

/**
 * @typedef CancellationToken
 * @property {boolean} isCancelled
 * @function throwIfCancelled()
 */

export default function cancellable() {
	let isCancelled = false;
	return {
		cancel() { isCancelled = true; },
		token: {
			get isCancelled() { return isCancelled; },
			throwIfCancelled() {
				if (isCancelled) throw cancelled;
			}
		},
	};
}