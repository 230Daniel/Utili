import React from "react";
import Helmet from "react-helmet";

import { get } from "../api/auth";
import Fade from "../components/effects/fade";
import Divider from "../components/layout/divider";
import Load from "../components/load";

import "../styles/document.css";
import Error from "./error";

class MessageLogs extends React.Component {
	constructor(props) {
		super(props);
		this.id = this.props.match.params.id;
		this.state = {
			entry: null,
			notFound: false
		};
	}

	async componentDidMount() {
		var response = await get(`message-logs/${this.id}`);
		if (response.status === 404) {
			this.setState({ notFound: true });
		} else {
			var entry = await response?.json();
			this.setState({ entry: entry });
		}
	}

	render() {
		if (this.state)
			return (
				<>
					<Helmet>
						<title>Message Logs - Utili</title>
					</Helmet>
					<div className="container" style={{ paddingBottom: "40px" }}>
						<Fade>
							{this.renderContent()}
						</Fade>
					</div>
				</>
			);
	}

	renderContent() {
		if (this.state.notFound) {
			return <>
				<Error
					code="404"
					shortDescription="Not found"
					longDescription="Sorry, this link has expired."
				/>
			</>;
		} else {
			return <>
				<h1>Message Logs</h1>
				<Load loaded={this.state.entry}>
					<Divider top="40" bottom="30">Information</Divider>
					<div style={{ fontSize: "16px" }}>
						<p>Logged {this.state.entry?.messagesLogged} of {this.state.entry?.messagesDeleted} deleted messages.</p>
						<p>Messages were deleted on {this.state.entry?.timestamp} and will be available here for 30 days.</p>
					</div>
					<Divider top="30" bottom="30">Messages</Divider>
					<div style={{ fontSize: "16px", whiteSpace: "pre-line" }}>
						<table className="message-logs-table">
							<colgroup>
								<col span="1" style={{ width: "25%" }} />
								<col span="1" style={{ width: "75%", borderLeft: "1px solid var(--colour-divider)" }} />
							</colgroup>
							<tbody>
								{this.state.entry?.messages?.map((message, i) => {
									return (
										<tr key={i}>
											<td>
												<b>{message.username}</b><br />
												{message.timestamp}
											</td>
											<td>{message.content}</td>
										</tr>
									);
								})}
							</tbody>
						</table>
					</div>
				</Load>
			</>;
		}
	}

	renderMessages(messages) {
		var renderedMessages = [];
		for (var message in messages) {
			renderedMessages.push(<p>{message}</p>);
		}
		return renderedMessages;
	}
}

export default MessageLogs;
