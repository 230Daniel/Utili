import React from "react";
import { Link } from "react-router-dom";
import Helmet from "react-helmet";
import Fade from "../components/effects/fade";
import Divider from "../components/layout/divider";

class Contact extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			feature: ""
		};
		this.features = [];
		this.text = "dd";
		this.i = 0;
	}

	render() {
		return (
			<>
				<Helmet>
					<title>Contact Us - Utili</title>
				</Helmet>
				<div className="container" style={{ paddingBottom: "40px" }}>
					<Fade>
						<h1>Contact Us</h1>
						<Divider top="40" bottom="30">Discord Server</Divider>
						<div style={{ fontSize: "16px" }}>
							<p>The fastest way to contact us is by joining our <a href="https://discord.gg/WsxqABZ" className="link">Discord server</a>.</p>
						</div>
						{window.__config.officialInstance &&
							<>
								<Divider top="30" bottom="30">Email</Divider>
								<div style={{ fontSize: "16px" }}>
									<p>Alternatively, contact us by email:</p>
									<ul>
										<li>All inquiries - <a className="link" href="mailto:daniel.baynton@hotmail.com">daniel.baynton@hotmail.com</a></li>
									</ul>
								</div>
							</>
						}

					</Fade>
				</div>

			</>
		);
	}
}

export default Contact;
