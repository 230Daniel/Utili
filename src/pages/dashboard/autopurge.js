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
			textChannels: null
		};
		this.settings = {
			channelAdder: React.createRef(),
			channels: []
		}
	}

	render(){
		var channels = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		var autopurgeChannels = channels?.filter(x => this.state.autopurge?.rows.some(y => y.channelId == x.id));
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
						<p>On your server, one autopurge channel will be checked every 30 seconds. If you have more than 1 channel, they cycle around in a queue.</p>
						<p><b>Premium:</b> Process 5 channels every 10 seconds</p>
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
							{this.state.autopurge?.rows.map((row, i) =>{
								return(
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={i} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent title="Threshold" type="timespan" value={Duration.fromISO(row.timespan)} ref={this.settings.channels[i].timespan}/>
										<CardComponent title="Mode" type="select" value={row.mode} options={["All Messages", "Bot Messages", "Disabled"]} ref={this.settings.channels[i].mode}/>
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
		var response = await get(`dashboard/${this.guildId}/autopurge`);
		this.state.autopurge = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		this.state.textChannels = await response?.json();
		for(var i = 0; i < this.state.autopurge.rows.length; i++){
			this.settings.channels.push({ timespan: React.createRef(), mode: React.createRef() });
			this.state.autopurge.rows[i]["channelName"] = this.getChannelName(this.state.autopurge.rows[i].channelId);
		}
		this.sortChannels();
		this.setState({});
	}

	onChannelAdded(channel){
		this.settings.channels.push({ timespan: React.createRef(), mode: React.createRef() });
		this.state.autopurge.rows.push({
			channelId: channel.id.toString(),
			timespan: "PT5M",
			mode: 0,
			channelName: this.getChannelName(channel.id)
		});
		this.sortChannels();
		this.setState({});
	}

	onChannelRemoved(id){
		this.settings.channels.pop();
		this.state.autopurge.rows = this.state.autopurge.rows.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.autopurge.rows;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].timespan = card.timespan.current.getValue();
			rows[i].mode = card.mode.current.getValue();
		}
		this.state.autopurge.rows = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/autopurge`, this.state.autopurge);
		return response.ok;
	}

	sortChannels(){
		this.state.autopurge.rows.sort((a, b) => (a.channelName > b.channelName) ? 1 : -1)
	}

	getChannelName(id){
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default Autopurge;
