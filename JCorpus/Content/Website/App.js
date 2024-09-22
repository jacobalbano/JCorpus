import { html } from '/libs/htm.js';
import Search from '/pages/search/Search.js';
import Extract from '/pages/extract/Extract.js';
import Accelerate from '/pages/Accelerate.js';
import Analyze from '/pages/Analyze.js';
import NotFound from '/pages/NotFound.js';
import { AppNav, NavElement } from '/components/AppNav.js';
import pReactive from '/components/Component.js';
import { Switch, Case, Default } from '/components/SwitchCase.js';

export default class App extends pReactive({
	nav: "search",
}) {
	render() {
		return html`
		<${AppNav} route="${this.nav}" onNavigate="${value => this.nav = value}" >
			<${NavElement} icon="search" route="search">
				Search
			<//>
			<${NavElement} icon="library_add" route="extract">
				Extract
			<//>
			<${NavElement} icon="speed" route="accelerate">
				Accelerate
			<//>
			<${NavElement} icon="insert_chart" route="analyze">
				Analyze
			<//>
			<${NavElement} icon="cycle" route="logs">
				Jobs
			<//>
			<${NavElement} icon="terminal" route="logs">
				Logs
			<//>
		<//>
		
		<header>
		<nav>
		<h5 class="capitalize max center-align">JCorpus</h5>
		</nav>
	  </header>
		<main class="responsive top-margin">
			<div class="grid">
				<${Switch} on="${this.nav}">
					<${Case} when="search">
						${() => html`<${Search} />`}
					<//>
					<${Case} when="extract">
						${() => html`<${Extract} />`}
					<//>
					<${Case} when="accelerate">
						${() => html`<${Accelerate} />`}
					<//>
					<${Case} when="analyze">
						${() => html`<${Analyze} />`}
					<//>
					<${Default}>
						<${NotFound} />
					<//>
				<//>
			</div>
		</main>
		<footer class="container">footer</footer>`;

	}
}