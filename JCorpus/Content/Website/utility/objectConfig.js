export default function objectConfig(pluginName, addinName, configJson) {
	return {
		PluginName: pluginName,
		AddinName: addinName,
		ConfigurationJson: configJson ?? {},
	};
}