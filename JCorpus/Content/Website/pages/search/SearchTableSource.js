import { PagedTableSource } from "/components/PagedTable.js";
import { cancel, get, poll, postJson } from "/utility/api.js";
import objectConfig from "/utility/objectConfig.js";

export default class SearchTableSource extends PagedTableSource {
	#owner;

	constructor(owner, getConfig) {
		super();
		this.#owner = owner;
		this.#jobId = null;
		this.#getConfig = getConfig;
	}

	showDialog(dialog) {
		this.#owner.showDialog(dialog);
	}

	async startNew() {
		const {pluginName, addinTypeName} = this.#owner.searchAddinKey;
		const response = await postJson('api/Search', {
			searchBy: objectConfig(
				pluginName,
				addinTypeName,
				this.#getConfig()
			)
		});

		const job = await response.json();
		this.#jobId = job.id;
	}

	async cancel() {
		await cancel(`api/Search/${this.#jobId}`);
		this.#jobId = null;
	}

	async getPage(page, token) {
		const { result } = await poll(() => get(`api/Search/${this.#jobId}`, { page }), token);
		return result;
	}

	#jobId = null;
	#getConfig;
}