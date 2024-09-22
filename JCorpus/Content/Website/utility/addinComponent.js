export async function addinComponent({pluginName, addinTypeName}) {
	try {
		const comp = await import(`/api/UI/${pluginName}/${addinTypeName}.js`);
		return comp.default;
	} catch (e) {
		if (!(e instanceof TypeError)) throw e;
		return null;
	}
}