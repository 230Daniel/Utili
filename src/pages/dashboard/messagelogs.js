import React from "react";
import Helmet from "react-helmet";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";

class MessageLogs extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			messageLogs: null,
			textChannels: null
		};
		this.settings = {
			deletedChannel: React.createRef(),
			editedChannel: React.createRef(),
			excludedChannels: React.createRef()
		}
	}

	render(){
		var values = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Message Logging - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Message Logging</div>
					<div className="dashboard-subtitle">Keeps a record of deleted and edited messages</div>
					<div className="dashboard-description">
						<p>description</p>
					</div>
					<Load loaded={this.state.messageLogs !== null}>
						<Card title="Message Logging Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="select-value" title="Deleted messages" values={values} value={this.state.messageLogs?.deletedChannelId} ref={this.settings.deletedChannel}></CardComponent>
							<CardComponent type="select-value" title="Edited messages" values={values} value={this.state.messageLogs?.editedChannelId} ref={this.settings.editedChannel}></CardComponent>
						</Card>
						<Card title="Excluded Channels" size={400} onChanged={this.props.onChanged}>
							<CardListComponent prompt="Exclude a channel..." values={values} selected={this.state.messageLogs?.excludedChannels} ref={this.settings.excludedChannels}></CardListComponent>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/messagelogs`);
		this.state.messageLogs = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		this.state.textChannels = await response?.json();

		this.setState({});
	}

	getInput(){
		this.state.messageLogs = {
			deletedChannelId: this.settings.deletedChannel.current.getValue(),
			editedChannelId: this.settings.editedChannel.current.getValue(),
			excludedChannels: this.settings.excludedChannels.current.getSelected()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/messagelogs`, this.state.messageLogs);
		return response.ok;
	}
}

export default MessageLogs;
