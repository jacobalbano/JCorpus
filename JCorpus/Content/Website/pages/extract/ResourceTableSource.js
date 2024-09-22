import { PagedTableSource } from "/components/PagedTable.js";
import { cancel, get, poll, postJson } from "/utility/api.js";
import objectConfig from "/utility/objectConfig.js";
import { reactiveDictionary } from "/utility/Reactive.js";

export default class ResourceTableSource extends PagedTableSource {
	constructor(owner, getConfig) {
		super();
		this.#owner = owner;
		this.#jobId = null;
		this.#getConfig = getConfig;
	}

	set allChecked(checked) {
		this.#owner.allChecked = checked;
		this.#owner.checked = reactiveDictionary();
	}

	get allChecked() {
		return this.#owner.allChecked;
	}

	setChecked(id, checked) {
		this.#owner.checked[id] = (checked !== this.allChecked);
	}

	getChecked(id) {
		if (!this.allChecked) {
			return Boolean(this.#owner.checked[id]);
		} else {
			return !this.#owner.checked[id];
		}
	}

	async startNew() {
		const {pluginName, addinTypeName} = this.#owner.sourceAddinKey;
		const response = await postJson('api/Extract/Enumerate', {
			source: objectConfig(
				pluginName,
				addinTypeName,
				this.#getConfig()
			)
		});

		const job = await response.json();
		this.#jobId = job.id;
	}

	async cancel() {
		await cancel(`api/Extract/Enumerate/${this.#jobId}`);
		this.#jobId = null;
	}

	async getPage(page, token) {
		const { result } = await poll(() => get(`api/Extract/Enumerate/${this.#jobId}`, { page }), token);
		return result;
	}

	#owner;
	#jobId = null;
	#getConfig;
}