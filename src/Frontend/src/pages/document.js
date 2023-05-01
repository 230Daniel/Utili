import React from "react";
import Helmet from "react-helmet";
import toMarkdown from "marked";
import cheerio from "cheerio";
import { useParams } from "react-router-dom";

import Fade from "../components/effects/fade";
import Error from "../pages/error";
import "../styles/document.css";

export default function DocumentLoader() {
	let { document } = useParams();
	return (
		<Document source={document} key={document} />
	);
}

class Document extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			markdown: '',
			title: '',
			notFound: false
		};
	}

	async componentDidMount() {
		try {
			var source = await import(`../documents/${this.props.source}.md`);
			var response = await fetch(source.default);
			var text = await response.text();
			var markdown = toMarkdown(text);
			var title = cheerio.load(markdown)('h1').first().html();
			this.setState({ markdown: markdown, title: title });
		}
		catch {
			this.setState({ notFound: true });
		}
	}

	componentDidUpdate() {
		setTimeout(() => {
			window.scrollTo({ top: 0, left: 0, behavior: "smooth" });
		}, 100);
	}

	render() {
		if (this.state.notFound || !window.__config.officialInstance) {
			return notFound();
		}
		if (this.state.markdown === '') {
			return (
				<></>
			);
		}
		return (
			<>
				<Helmet>
					<title>{this.state.title} - Utili</title>
				</Helmet>
				<Fade>
					<div className="document container" style={{ paddingBottom: "40px" }} dangerouslySetInnerHTML={{ __html: this.state.markdown }} />
				</Fade>
			</>
		);
	}
}

function notFound() {
	return (
		<Error
			code="404"
			shortDescription="Not found"
			longDescription="Sorry, we couldn't find that page on our servers."
		/>
	);
}
