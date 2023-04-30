import React from "react";
import Helmet from "react-helmet";

import { get } from "../api/auth";
import Fade from "../components/effects/fade";
import Divider from "../components/layout/divider";
import Load from "../components/load";

import "../styles/document.css";

class MessageLogs extends React.Component {
	constructor(props) {
		console.log("hello world");
		super(props);
		this.id = this.props.match.params.id;
		this.state = {
			entry: null
		};
	}

	async componentDidMount() {
		var response = await get(`message-logs/${this.id}`);
		var entry = await response?.json();
		this.setState({ entry: entry });
	}

	render() {
		return (
			<>
				<Helmet>
					<title>Message Logs - Utili</title>
				</Helmet>
				<div className="container" style={{ paddingBottom: "40px" }}>
					<Fade>
						<h1>Message Logs</h1>
						<Load loaded={this.state.entry}>
							<Divider top="40" bottom="30">Information</Divider>
							<div style={{ fontSize: "16px" }}>
								<p>Logged {this.state.entry?.messagesRecorded} of {this.state.entry?.messagesDeleted} deleted messages.</p>
							</div>
							<Divider top="30" bottom="30">Messages</Divider>
							<div style={{ fontSize: "16px" }}>
								<p>{this.state.entry?.messages}</p>
							</div>
						</Load>
					</Fade>
				</div>
			</>
		);
	}
}

export default MessageLogs;
