import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";

class VoiceLink extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			voiceLink: null,
			voiceChannels: null
		};
		this.settings = {
			enabled: React.createRef(),
			deleteChannels: React.createRef(),
			channelPrefix: React.createRef(),
			excludedChannels: React.createRef()
		}
	}

	render(){
		var values = this.state.voiceChannels?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Voice Link - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Voice Link</div>
					<div className="dashboard-subtitle">Creates a text channel for each voice channel</div>
					<div className="dashboard-description">
						<p>A private text channel is created in the same category as the voice channel.<br/>
						When people are in the voice channel, they can use the text channel.</p>
						<p><b>Delete Channels:</b> Determines if the text channel should be deleted when the voice channel is empty</p>
					</div>
					<Load loaded={this.state.voiceLink !== null}>
						<Card title="Voice Link Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="checkbox" title="Enabled" value={this.state.voiceLink?.enabled} ref={this.settings.enabled}></CardComponent>
							<CardComponent type="checkbox" title="Delete Channels" value={this.state.voiceLink?.deleteChannels} ref={this.settings.deleteChannels}></CardComponent>
							<CardComponent type="text" title="Channel Prefix" value={this.state.voiceLink?.channelPrefix} ref={this.settings.channelPrefix}></CardComponent>
						</Card>
						<Card title="Excluded Channels" size={400} onChanged={this.props.onChanged}>
							<CardListComponent prompt="Exclude a channel..." values={values} selected={this.state.voiceLink?.excludedChannels} ref={this.settings.excludedChannels}></CardListComponent>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/voice-link`);
		this.state.voiceLink = await response?.json();
		response = await get(`discord/${this.guildId}/voice-channels`);
		this.state.voiceChannels = await response?.json();
		this.state.voiceLink.excludedChannels = this.state.voiceLink.excludedChannels.filter(x => this.state.voiceChannels.some(y => x == y.id));
		this.setState({});
	}

	getInput(){
		this.state.voiceLink = {
			enabled: this.settings.enabled.current.getValue(),
			deleteChannels: this.settings.deleteChannels.current.getValue(),
			channelPrefix: this.settings.channelPrefix.current.getValue(),
			excludedChannels: this.settings.excludedChannels.current.getSelected()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/voice-link`, this.state.voiceLink);
		return response.ok;
	}
}

export default VoiceLink;
