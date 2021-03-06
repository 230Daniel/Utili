import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class ChannelMirroring extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			channelMirroring: null,
			textChannels: null
		};
		this.settings = {
			channelAdder: React.createRef(),
			channels: []
		}
	}

	render(){
		var channels = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		var mirroringChannels = channels?.filter(x => this.state.channelMirroring?.rows.some(y => y.fromChannelId == x.id));
		return(
			<>
				<Helmet>
					<title>Channel Mirroring - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Channel Mirroring</div>
					<div className="dashboard-subtitle">Clones messages from one channel to another</div>
					<div className="dashboard-description">
						<p>New messages will be copied to the other channel using the original sender's username and avatar.<br/>
						Unfortunately it's not possible to copy the username as well.</p>
					</div>
					<Load loaded={this.state.channelMirroring !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent 
								prompt="Add a channel..." 
								values={channels} 
								selected={mirroringChannels}
								onSelected={(id) => this.onChannelAdded(id)} 
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder}/>
						</Card>
						<div className="inline">
							{this.state.channelMirroring?.rows.map((row, i) =>{
								return(
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.fromChannelId} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.fromChannelId)}>
										<CardComponent type="select-value" title="Mirror to" values={channels} value={row.toChannelId} ref={this.settings.channels[i].toChannelId}/>
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
		var response = await get(`dashboard/${this.guildId}/channelmirroring`);
		this.state.channelMirroring = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		this.state.textChannels = await response?.json();

		for(var i = 0; i < this.state.channelMirroring.rows.length; i++){
			this.settings.channels.push({ toChannelId: React.createRef() });
			this.state.channelMirroring.rows[i]["channelName"] = this.getChannelName(this.state.channelMirroring.rows[i].fromChannelId);
		}
		this.sortChannels();
		this.setState({});
	}

	onChannelAdded(channel){
		this.settings.channels.push({ toChannelId: React.createRef() });
		this.state.channelMirroring.rows.push({
			fromChannelId: channel.id,
			toChannelId: 0,
			channelName: this.getChannelName(channel.id)
		});
		this.sortChannels();
		this.setState({});
	}

	onChannelRemoved(id){
		var index = this.state.channelMirroring.rows.map(x => x.fromChannelId).indexOf(id);
		this.settings.channels.splice(index, 1);
		this.state.channelMirroring.rows.splice(index, 1);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.channelMirroring.rows;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].toChannelId = card.toChannelId.current.getValue();
		}
		this.state.channelMirroring.rows = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/channelmirroring`, this.state.channelMirroring);
		return response.ok;
	}

	sortChannels(){
		this.state.channelMirroring.rows.sort((a, b) => (a.channelName > b.channelName) ? 1 : -1)
	}

	getChannelName(id){
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default ChannelMirroring;
