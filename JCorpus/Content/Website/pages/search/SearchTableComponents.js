import { addHighlights } from './searchResultUtility.js';
import { html } from '/libs/htm.js';

export const SearchHeader = () => html`
<thead>
	<th class="min">CorpusWorkId</th>
	<th class="min">Reference</th>
	<th>Text</th>
	<th class="min">Info</th>
</thead>`;

export const SearchRow = ({source, corpusWorkId, line, lineReference, matchRanges}) => html`
<tr>
	<td>${corpusWorkId}</td>
	<td>${lineReference}</td>
	<td >${addHighlights(matchRanges, line)}</td>
	<td>
		<a class="transparent circle"
			onClick=${() => source.showDialog({ corpusWorkId, line, lineReference, matchRanges })}
		>
			<i>info</i>
		</a>
	</td>
</tr>`;

export const SearchEmpty = () => html`
<div class="large-height middle-align center-align">
	<div class="center-align">
		<h5>No search results</h5>
	</div>
</div>
`;