import { html } from '/libs/htm.js';
import pReactive from '/components/Component.js';
import AddinSelect from '/components/addin/AddinSelect.js';
import { blur, classes, preventDefault } from "/utility/htmlUtility.js";
import PagedTable from '/components/PagedTable.js';
import { cancel, get, poll, postJson } from '/utility/api.js';
import objectConfig from '/utility/objectConfig.js';
import { reactiveDictionary, reactorSym } from '/utility/Reactive.js';
import { ResourceEmpty, ResourceHeader, ResourceRow } from './ResourceTableComponents.js';
import ResourceTableSource from './ResourceTableSource.js';
import AddinComponent from '/components/addin/AddinComponent.js';
import LoadingButton from '/components/LoadingButton.js';
import cancellable from '/utility/cancellable.js';

export default class Extract extends pReactive({
	sourceAddinKey: null,
	extractorAddinKey: null,
	loading: false,
	checked: null,
	allChecked: false
}) {
	#sourceConfigControl = null;
	#extractorConfigControl = null;
	#controller = null;
	#source = new ResourceTableSource(this, () => this.#sourceConfigControl?.getConfig?.());

	constructor() {
		super();
		this.checked = reactiveDictionary();
	}

	render() {
		return html`
		<div class="s4">
			<h3>Extract</h3>
			<form onSubmit="${preventDefault(() => this.#onSubmit())}" >
				<article class="no-elevate border">
					<${AddinSelect}
						title="Source"
						category="ICorpusWorkSource"
						onChange="${key => this.#onSourceChange(key)}"
					/>

					<${AddinComponent}
						readonly=${true}
						addinKey=${this.sourceAddinKey}
						ref=${c => this.#updateSourceConfig(c)}
					/>

					<${AddinSelect}
						title="Extractor"
						category="ICorpusWorkExtractor"
						onChange="${key => this.extractorAddinKey = key}"
					/>

					<${AddinComponent}
						addinKey=${this.extractorAddinKey}
						ref=${c => this.#extractorConfigControl = c}
					/>

					<div class="row">
						<div class="max"></div>
						<${LoadingButton} loading=${this.loading} onClick=${blur()} />
					</div>
				</article>
			</form>
		</div>
		<div class="s8">
			<${PagedTable}
				title="Resources"
				outController=${c => this.#controller = c}
				source=${this.#source}
				headerComponent=${ResourceHeader}
				rowComponent=${ResourceRow}
				emptyComponent=${ResourceEmpty}
			/>
		</div>
		`;
	}

	async #onSubmit() {
		if (this.loading) {
			await this.#cancelExtract();
		} else {
			this.loading = true;
			await this.#runExtract()
		}

		this.loading = false;
	}

	#cancel;
	#job;
	async #cancelExtract() {
		this.#cancel?.();
		this.#cancel = null;

		await cancel(`/api/Extract/${this.#job.id}`)
	}

	async #runExtract() {
		const sk = this.sourceAddinKey;
		const ek = this.extractorAddinKey;

		const response = await postJson('api/Extract', {
			source: objectConfig(
				sk.pluginName,
				sk.addinTypeName,
				this.#sourceConfigControl?.getConfig?.(),
			),
			extractor: objectConfig(
				ek.pluginName,
				ek.addinTypeName,
				this.#extractorConfigControl?.getConfig?.()
			),
			filterMode: this.allChecked ? 'Exclude' : 'Include',
			filterIds: Object.keys(this.checked)
		});

		const { token, cancel } = cancellable();
		this.#cancel = cancel;
		this.#job = await response.json();

		await poll(() => get(`/api/Extract/${this.#job.id}`), token);
	}

	async #refreshResources() {
		await this.#controller.cancel();
		await this.#controller.startNew();
	}

	#unsubWatch;
	#updateSourceConfig(ref) {
		if (this.#unsubWatch) {
			this.#unsubWatch();
			this.#unsubWatch = null;
		}

		this.#sourceConfigControl = ref;
		if (!ref) return;
		
		const later = debounce(() => this.#refreshResources(), 550);
		this.#unsubWatch = ref[reactorSym]?.subscribe(later);
	}

	async #onSourceChange(key) {
		this.sourceAddinKey = key;
		await this.#refreshResources();
		this.checked = reactiveDictionary();
	}
}

function debounce(callback, wait) {
	let timeoutId = null;
	return (...args) => {
		window.clearTimeout(timeoutId);
		timeoutId = window.setTimeout(() => {
			callback(...args);
		}, wait);
	};
}