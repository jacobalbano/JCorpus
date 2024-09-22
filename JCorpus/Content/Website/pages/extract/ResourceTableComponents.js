import { html } from '/libs/htm.js';
import Checkbox, { bindChecked } from '/components/material/Checkbox.js';

export const ResourceHeader = ({source}) => html`
<thead>
	<th class="min">
		<${Checkbox}
			selected=${source.allChecked}
			onChange=${bindChecked(source, 'allChecked') }
		/>
	</th>
	<th>Title</th>
	<th>Author</th>
</thead>`;

export const ResourceRow = ({uniqueId, source, fields}) => html`
<tr>
	<td>
		<${Checkbox}
			selected=${source.getChecked(uniqueId)}
			onChange=${({target}) => source.setChecked(uniqueId, target.checked)}
		/>
	</td>
	<td>
		${[...fields].filter(x => x.key === 'Title').pop()?.value}
	</td>
	<td>
		${[...fields].filter(x => x.key === 'Author').pop()?.value}
	</td>
</tr>`;

export const ResourceEmpty = () => html`
<div class="large-height middle-align center-align">
	<div class="center-align">
		<h5>No resources</h5>
	</div>
</div>
`;