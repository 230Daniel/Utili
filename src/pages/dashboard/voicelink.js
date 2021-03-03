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
			prefix: React.createRef(),
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
					<div className="dashboard-subtitle">Automatically deletes messages</div>
					<div className="dashboard-description">
						<p>dd</p>
					</div>
					<Load loaded={this.state.voiceLink !== null}>
						<Card title="Voice Link Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="checkbox" title="Enabled" value={this.state.voiceLink?.enabled} ref={this.settings.enabled}></CardComponent>
							<CardComponent type="checkbox" title="Delete Channels" value={this.state.voiceLink?.deleteChannels} ref={this.settings.deleteChannels}></CardComponent>
							<CardComponent type="text" title="Channel Prefix" value={this.state.voiceLink?.prefix} ref={this.settings.prefix}></CardComponent>
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
		var response = await get(`dashboard/${this.guildId}/voicelink`);
		this.state.voiceLink = await response?.json();
		response = await get(`discord/${this.guildId}/channels/voice`);
		this.state.voiceChannels = await response?.json();

		this.setState({});
	}

	getInput(){
		this.state.voiceLink = {
			enabled: this.settings.enabled.current.getValue(),
			deleteChannels: this.settings.deleteChannels.current.getValue(),
			prefix: this.settings.prefix.current.getValue(),
			excludedChannels: this.settings.excludedChannels.current.getSelected()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/voicelink`, this.state.voiceLink);
		return response.ok;
	}
}

export default VoiceLink;
