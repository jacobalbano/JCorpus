import { html } from "/libs/htm.js";

export function addHighlights(matchRanges, text) {
	let cursor = 0;
	const parts = [];
	for (const {start, length} of matchRanges) {
		if (start < cursor) continue;
		parts.push(text.substring(cursor, start));
		parts.push(html`<span class="primary-container">${text.substr(start, length)}</span>`);
		cursor = start + length;
	}

	parts.push(text.substr(cursor));
	return html`<span highlights="${matchRanges}">${parts}</span>`;
}