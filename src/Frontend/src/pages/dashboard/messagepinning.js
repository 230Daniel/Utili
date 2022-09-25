import React from "react";
import Helmet from "react-helmet";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";

class MessagePinning extends React.Component {
	constructor(props) {
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			messagePinning: null,
			textChannels: null
		};
		this.settings = {
			pinMessages: React.createRef(),
			pinChannel: React.createRef()
		};
	}

	render() {
		var values = this.state.textChannels?.map(x => { return { id: x.id, value: x.name }; });
		return (
			<>
				<Helmet>
					<title>Message Pinning - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Message Pinning</div>
					<div className="dashboard-subtitle">Send messages to a channel using the pin command</div>
					<div className="dashboard-description">
						<ul>
							<li>pin [message id]</li>
							<li>pin [from channel] [message id]</li>
							<li>pin [message id] [pin channel]</li>
							<li>pin [from channel] [message id] [pin channel]</li>
						</ul>
					</div>
					<Load loaded={this.state.messagePinning !== null}>
						<Card title="Message Pinning Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="checkbox" title="Pin messages" values={values} value={this.state.messagePinning?.pinMessages} ref={this.settings.pinMessages}></CardComponent>
							<CardComponent type="select-value" title="Send pins to" values={values} value={this.state.messagePinning?.pinChannelId} ref={this.settings.pinChannel}></CardComponent>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}

	async componentDidMount() {
		var response = await get(`dashboard/${this.guildId}/message-pinning`);
		this.state.messagePinning = await response?.json();
		response = await get(`discord/${this.guildId}/message-channels`);
		this.state.textChannels = await response?.json();

		this.setState({});
	}

	getInput() {
		this.state.messagePinning = {
			pinMessages: this.settings.pinMessages.current.getValue(),
			pinChannelId: this.settings.pinChannel.current.getValue()
		};
		this.setState({});
	}

	async save() {
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/message-pinning`, this.state.messagePinning);
		return response.ok;
	}
}

export default MessagePinning;
