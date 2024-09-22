import { html } from '/libs/htm.js';
import pReactive from './Component.js';
import { blur, when } from '/utility/htmlUtility.js';
import cancellable, { cancelled } from '/utility/cancellable.js';

function maybe(component, props = {}) {
	if (component) return html`<${component} ...${props} />`
}

export class PagedTableSource {
	/**
	 * @typedef PagedTableController
	 * @prop {async function():TransitPage} startNew Start a new search at page 1.
	 * @prop {async function():void} cancel Stop the currently running operation.
	 */

	/**
	 * @typedef TransitPage
	 * @prop {object[]} items The items contained in the page
	 * @prop {number} totalCount The total number of items in the result set
	 * @prop {number} pageCount	The total number of pages in the result set
	 * @prop {number} pageNumber This page number (will be 1 for the first page)
	 * @prop {number} pageSize The number of items per page (may be more than {@link totalCount}
	 */

	/**
	 * Used internally to create an empty initial dataset.
	 * @returns {TransitPage}
	 * */
	static emptyPage() {
		return {
			items: [],
			totalCount: 0,
			pageCount: 0,
			pageNumber: 0,
			pageSize: 0, 
		};
	}
	
	/**
	 * Start a new search or other operation which will return paginated results.  
	 * The implementor should keep track of a long-running job ID or other tracking mechanism.  
	 * The calling {@link PagedTable} will await {@link getPage} until a page is returned.
	 */
	async startNew() {
		console.warn(`${this.constructor.name}.startNew() has not been overridden`);
	}

	/**
	 * Cancel the currently-running search or pagination operation.
	 */
	async cancel() {
		console.warn(`${this.constructor.name}.cancel() has not been overridden`);
	}

	/**
	 * Get a page from a result set.  
	 * The implementor is responsible for making API calls and polling for results if necessary.
	 * @import { CancellationToken } from '/utility/cancellable.js'
	 * @param {number} pageNum The number of the page to return. The first page is represented by 1.
	 * @param {CancellationToken} token Cancellation token. The implementor should check for cancellation and throw if necessary to interrupt the operation.
	 * @returns {TransitPage}
	 */
	async getPage(pageNum, token) {
		this.pageNum = pageNum;
		return PagedTableSource.emptyPage();
	}
}

/**
 * Component for rendering paged data from an (optionally) async source
 * @prop {PagedTableSource} source An instance of the data source. Should not change.
 * @prop {function(object):void?} outController Called by the table to pass out a {@link PagedTableController}.
 * @prop {string?} title The table title.
 * @prop {string?} errorText Text shown if loading fails.
 * @prop {typeof Component?} headerComponent Optional component class 
 * @prop {typeof Component?} rowComponent Optional component class 
 * @prop {typeof Component?} emptyComponent Optional component class 
 * @prop {typeof Component?} footerComponent Optional component class 
 */
export default class PagedTable extends pReactive({
	title: null,
	pageNum: 1,
	loading: false,
	error: false,
	
	 /**@type {TransitPage} */
	page: null,
}) {
	#cancel = null;
	#source = null;
	#controller = null;

	get #pageStart() {
		return this.page.pageSize * (this.pageNum - 1);
	}

	constructor({source, outController}) {
		super();

		this.#source = source;
		this.page = PagedTableSource.emptyPage();
		
		this.#controller = Object.freeze({
			startNew: async () => {
				await this.#tryLoading(async token => {
					await this.#source.startNew();
					return this.#doSearch(token, 1);
				});
			},
			cancel: async () => {
				try {
					this.#cancel && this.#cancel();
				} finally {
					while (self.loading)
						await new Promise(r => setTimeout(r, 100));
				}
			}
		});

		if (outController)
			outController(this.#controller);
	}

	render({title, headerComponent, rowComponent, emptyComponent, footerComponent, errorText}) {
		const source = this.#source;
		return html`
			<div class="row" >
				<h3>${title}</h3>
				<div class="max"></div>

				${when(this.page.items.length > 0, () => html`
					<label>
						${this.#pageStart + 1} - ${this.#pageStart + this.page.items.length} of ${this.page.totalCount}
					</label>
					<button class="circle transparent"
						onClick="${blur(() => this.#go(-1))}"
						...${{ disabled: this.pageNum <= 1} }>
						<i>arrow_back</i>
					</button>
					<button class="circle transparent"
						onClick="${blur(() => this.#go(+1))}"
						...${{ disabled: this.pageNum >= this.page.pageCount} }>
						<i>arrow_forward</i>
					</button>
				`)}
			</div>

			<div>
				<!-- TODO: Ideally shouldn't have to do this but preact doesn't support removing the value attr -->
				<!-- https://github.com/preactjs/preact/issues/4487 -->
				<div style="height: 1em; ">
					${when(this.loading, () => html`<progress></progress>`)}
				</div>

				<div>
					<table class="stripes">
						${maybe(headerComponent, { source })}
						${when(this.page.items.length > 0, () => html`
							<tbody>
								${this.page.items.map((x, idx) => maybe(rowComponent, { idx, ...x, source }))}
							</tbody>
						`)}

						${maybe(footerComponent)}
					</table>
					
					${when(this.page.items.length == 0, () =>
						maybe(emptyComponent, { source }))
					}
					
					${when(this.error, () => html`
						<a style="position: absolute;top: 5px;" class="chip center error-container medium small-elevate no-round"
							onClick=${() => this.error = false }
						>
							${errorText ?? 'Error loading results'}
						</div>
					`)}
				</div>
			</div>
		`;
	}
	
	async #go(moveBy) {
		return this.#tryLoading(token => this.#doSearch(token, this.pageNum + moveBy, 2));
	}

	async #tryLoading(callback) {
		if (this.loading) return;
		try {
			this.loading = true;
			const { token, cancel } = cancellable();
			this.#cancel = cancel;
			const result = await callback(token);
			this.error = false;
			return result;
		} catch (error) {
			if (error === cancelled) return this.#source.cancel();
			this.error = true;
			setTimeout(() => this.error = false, 2000);
		} finally {
			this.#cancel = null;
			this.loading = false;
		}
	}

	async #doSearch(token, pageNum, maxAttempts = 1, attempts = 0) {
		try {
			if (attempts > 0) await this.#source.startNew();
			this.page = await this.#source.getPage(pageNum, token);
			this.pageNum = pageNum;
			return true;
		} catch (e) {
			if (attempts >= maxAttempts) throw e;
			return this.#doSearch(token, pageNum, maxAttempts, attempts + 1);
		}
	}
}