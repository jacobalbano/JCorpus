export function get(route, params) {
	return fetch(makeRoute(route, params));
}

export function postJson(route, body) {
	return fetch(route, {
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(body),
		method: 'POST',
	});
}

export function cancel(route, params) {
	return fetch(makeRoute(route, params), {
		method: 'DELETE',
	});
}

export async function poll(action, token) {
	while (true) {
		token.throwIfCancelled();
		
		const response = await action();
		if (!response.ok)
			throw new Error('response not ok', { cause: response });
		if (response.status == 200)
			return response.json();

		await new Promise(r => setTimeout(r, 1000));
	}
}

function makeRoute(route, params) {
	if (params != null) route = `${route}?${Object.keys(params)
		.map(x => `${x}=${encodeURIComponent(params[x])}`)
		.join('&')}`;

	return route;
}