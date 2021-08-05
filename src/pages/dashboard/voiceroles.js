import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class VoiceRoles extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			voiceRoles: null,
			voiceChannels: null,
			roles: null
		};
		this.settings = {
			channelAdder: React.createRef(),
			channels: []
		}
	}

	render(){
		var channels = this.state.voiceChannels?.map(x => {return {id: x.id, value: x.name}});
		var voiceRolesChannels = channels?.filter(x => this.state.voiceRoles?.some(y => y.channelId == x.id));
		var roles = this.state.roles?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Voice Roles - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Voice Roles</div>
					<div className="dashboard-subtitle">Give users roles while they're in a voice channel</div>
					<Load loaded={this.state.voiceRoles !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent 
								prompt="Add a channel..." 
								values={channels} 
								selected={voiceRolesChannels}
								onSelected={(id) => this.onChannelAdded(id)} 
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder}/>
						</Card>
						<div className="inline">
							{this.state.voiceRoles?.map((row, i) =>{
								return(
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.channelId} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent type="select-value" title="Role" values={roles} value={row.roleId} ref={this.settings.channels[i].roleId}></CardComponent>
									</Card>
								);
							})}
						</div>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/voice-roles`);
		this.state.voiceRoles = await response?.json();
		response = await get(`discord/${this.guildId}/voice-channels`);
		this.state.voiceChannels = await response?.json();
		this.state.voiceChannels.push({name:"Any other channel", id:"0"});
		response = await get(`discord/${this.guildId}/roles`);
		this.state.roles = await response?.json();

		this.state.voiceRoles = this.state.voiceRoles.filter(x => this.state.voiceChannels.some(y => y.id == x.channelId))
		for(var i = 0; i < this.state.voiceRoles.length; i++){
			this.settings.channels.push({ roleId: React.createRef() });
			this.state.voiceRoles[i]["channelName"] = this.getChannelName(this.state.voiceRoles[i].channelId);
		}

		this.state.voiceRoles.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelAdded(channel){
		this.settings.channels.push({ roleId: React.createRef() });
		this.state.voiceRoles.push({
			channelId: channel.id,
			roleId: 0,
			channelName: this.getChannelName(channel.id)
		});
		this.state.voiceRoles.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id){
		this.settings.channels.pop();
		this.state.voiceRoles = this.state.voiceRoles.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.voiceRoles;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].roleId = card.roleId.current.getValue();
		}
		this.state.voiceRoles = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/voice-roles`, this.state.voiceRoles);
		return response.ok;
	}

	getChannelName(id){
		return this.state.voiceChannels.find(x => x.id == id).name;
	}
}

export default VoiceRoles;
