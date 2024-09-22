import pReactive from "/components/Component.js";
import { html } from '/libs/htm.js';
import AddinDescription from '/components/addin/AddinDescription.js'
import Input, { bindInput } from "/components/material/Input.js";
import { get } from "/utility/api.js";
import { makeReactive } from "/utility/Reactive.js";
import Select, { bindIndex } from "/components/material/Select.js";
import Checkbox, { bindChecked} from "/components/material/Checkbox.js";
import { when, classes } from "/utility/htmlUtility.js";

export default class HostMachineSource extends pReactive({
	libraryRoot: null,
	extensionFilters: [],
	idFilters: [],
	identifierScheme: null,
	orderByType: {
		options: [],
		selected: 0
	},
	orderByDirection: {
		options: [],
		selected: 0
	}
}) {
	constructor() {
		super();
		get('/api/Addin/ICorpusWorkSource/CalibreLibrary/HostMachineSource')
			.then(x => x.json())
			.then(({ExtensionFilters, OrderBy}) => {
				this.populateEnums(ExtensionFilters, OrderBy);
			});
	}

	getConfig() {
		const {
			libraryRoot,
			extensionFilters,
			idFilters,
			identifierScheme,
			orderByType,
			orderByDirection
		} = this;

		return {
			libraryRoot,
			extensionFilters: extensionFilters
				.filter(x => x.selected)
				.map(x => x.name),
			idFilters,
			identifierScheme,
			orderBy: {
				type: orderByType.options[orderByType.selected],
				direction: orderByDirection.options[orderByDirection.selected],
			},
		}
	}

	populateEnums(extensions, orderBy) {
		const ext = extensions.$subtype.$enum;
		const dir = orderBy.$schema.Direction.$enum;
		const type = orderBy.$schema.Type.$enum;

		for (const e of ext) {
			this.extensionFilters.push(makeReactive({
				name: e, 
				selected: false
			}));
		}
		
		this.orderByDirection.options.push(...dir);
		this.orderByType.options.push(...type);
	}

	render() {
		const getLibraryErrorMessage = () => {
			if (!this.libraryRoot)
				return 'required';
			if (String(this.libraryRoot).indexOf('\\') >= 0)
				return 'path must use forward slashes';
		}

		return html`
		<${AddinDescription} title="Usage">
			<p>Books from a <a href="https://calibre-ebook.com/">Calibre</a> library on the machine this service is running on.</p>
		<//>
		<${Input}
			class="...${classes({ 'error-text': !this.libraryRoot })}"
			label="Library Root"
			onInput="${bindInput(this, 'libraryRoot')}"
			error=${getLibraryErrorMessage()}
		>${this.libraryRoot}<//>
		
		<${Input}
			class="...${classes({ 'error-text': !this.identifierScheme })}"
			label="Identifier Scheme"
			onInput="${bindInput(this, 'identifierScheme')}"
			error=${this.identifierScheme ? null : 'required (e.g. isbn, uri)'}
			helper="e.g. isbn"
		>${this.identifierScheme}<//>

		${this.extensionFilters.map(x => html`
			<${Checkbox}
				key=${x.name}
				checked=${x.selected}
				onChange=${bindChecked(x, 'selected')}
			>${x.name}<//>`
		)}

		<${Select}
			title="Order By"
			onChange=${bindIndex(this.orderByType, 'selected')}
		>
			${this.orderByType.options}
		<//>

		${when(this.orderByType.selected > 0, () => html`
		<${Select}
			title="Direction"
			onChange=${bindIndex(this.orderByDirection, 'selected')}
		>
			${this.orderByDirection.options}
		<//>
		`)}
		`;
	}
}
