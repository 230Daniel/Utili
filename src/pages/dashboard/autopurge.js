import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class Autopurge extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			autopurge: null,
			textChannels: null,
			premium: null
		};
		this.settings = {
			channelAdder: React.createRef(),
			channels: []
		}
	}

	render(){
		var channels = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		var autopurgeChannels = channels?.filter(x => this.state.autopurge?.some(y => y.channelId == x.id));
		return(
			<>
				<Helmet>
					<title>Autopurge - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Autopurge</div>
					<div className="dashboard-subtitle">Automatically deletes messages</div>
					<div className="dashboard-description">
						<p>Messages are deleted when they're older than the threshold you set.</p>
						{this.renderDescription()}
					</div>
					<Load loaded={this.state.autopurge !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent 
								prompt="Add a channel..." 
								values={channels} 
								selected={autopurgeChannels}
								onSelected={(id) => this.onChannelAdded(id)} 
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder}/>
						</Card>
						<div className="inline">
							{this.state.autopurge?.map((row, i) =>{
								return(
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.channelId} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent title="Threshold" type="timespan" value={Duration.fromISO(row.timespan)} ref={this.settings.channels[i].timespan}/>
										<CardComponent title="Mode" type="select" value={row.mode} options={["All Messages", "Bot Messages", "User Messages"]} ref={this.settings.channels[i].mode}/>
									</Card>
								);
							})}
						</div>
					</Load>
				</Fade>
			</>
		);
	}

	renderDescription(){
		if(this.state.premium && this.state.premium.premium){
			return(
				<p>On your server, five autopurge channels will be checked every 10 seconds.<br/>If you have more than 5 channels, they cycle around in a queue.</p>
			);
		} else {
			return(
				<div>
					<p>On your server, one autopurge channel will be checked every 30 seconds.<br/>If you have more than 1 channel, they cycle around in a queue.</p>
					<p><b>Premium:</b> Checks 5 channels every 10 seconds</p>
				</div>
			);
		}
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/autopurge`);
		this.state.autopurge = await response?.json();
		response = await get(`discord/${this.guildId}/text-channels`);
		this.state.textChannels = await response?.json();
		response = await get(`premium/guild/${this.guildId}`);
		this.state.premium = await response?.json();

		this.state.autopurge = this.state.autopurge.filter(x => this.state.textChannels.some(y => y.id == x.channelId))
		for(var i = 0; i < this.state.autopurge.length; i++){
			this.settings.channels.push({ timespan: React.createRef(), mode: React.createRef() });
			this.state.autopurge[i]["channelName"] = this.getChannelName(this.state.autopurge[i].channelId);
		}
		this.state.autopurge.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelAdded(channel){
		this.settings.channels.push({ timespan: React.createRef(), mode: React.createRef() });
		this.state.autopurge.push({
			channelId: channel.id,
			timespan: "PT5M",
			mode: 0,
			channelName: this.getChannelName(channel.id)
		});
		this.state.autopurge.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id){
		this.settings.channels.pop();
		this.state.autopurge = this.state.autopurge.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.autopurge;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].timespan = card.timespan.current.getValue();
			rows[i].mode = card.mode.current.getValue();
		}
		this.state.autopurge = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/autopurge`, this.state.autopurge);
		return response.ok;
	}

	getChannelName(id){
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default Autopurge;
