import { html } from '/libs/htm.js';
import pReactive from '/components/Component.js';
import SideDialog from '/components/SideDialog.js';
import { blur, preventDefault, when } from '/utility/htmlUtility.js';
import { addHighlights } from './searchResultUtility.js';
import PagedTable from '/components/PagedTable.js';
import { SearchEmpty, SearchHeader, SearchRow } from './SearchTableComponents.js';
import SearchTableSource from './SearchTableSource.js';
import AddinSelect from '/components/addin/AddinSelect.js';
import AddinComponent from '/components/addin/AddinComponent.js';
import LoadingButton from '/components/LoadingButton.js';


export default class Search extends pReactive({
	searchAddinKey: null,
	dialog: null,
	loading: false,
}) {
	#controller;
	#dialog;
	#searchConfigControl = null;
	#source = new SearchTableSource(
		this,
		() => this.#searchConfigControl?.getConfig?.()
	);

	constructor() {
		super();
	}

	async doSearch() {
		if (this.loading) {
			await this.#controller.cancel();
		} else {
			this.loading = true;
			await this.#controller.startNew();
		}

		this.loading = false;
	}
	
	showDialog({corpusWorkId, line, lineReference, matchRanges}) {
		this.dialog = { corpusWorkId, line, lineReference, matchRanges };
		if (this.#dialog) this.#dialog.active = true;
	}

	render() {
		return html`
		<div class="s4">
			<h3>Search</h3>
			<form onSubmit="${preventDefault(() => this.doSearch())}" >
				<article class="no-elevate border">
					<${AddinSelect}
						category="ISearch"
						title="Method"
						onChange="${key => this.#onAddinChange(key)}"
					/>

					<${AddinComponent}
						addinKey=${this.searchAddinKey}
						ref=${c => this.#searchConfigControl = c}
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
				title="Results"
				outController=${c => this.#controller = c}
				source=${this.#source}
				headerComponent=${SearchHeader}
				rowComponent=${SearchRow}
				emptyComponent=${SearchEmpty}
			/>
		</div>
		<${SideDialog} ref=${c => this.#dialog = c}>
			${when(this.#dialog?.active, () => html`
			<article class="no-elevate border">
				<h6>Context</h6>
				<div>
					<img src="api/Context/${this.dialog.corpusWorkId}/${this.dialog.lineReference}" />
				</div>
			</article>
			<article class="no-elevate border">
				<h6>Text</h6>
				<p>${addHighlights(this.dialog.matchRanges, this.dialog.line)}</p>
			</article>`)}
		<//>
		`;
	}
	
	async #onAddinChange(key) {
		this.searchAddinKey = key;
	}
	
}
