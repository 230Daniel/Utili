import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";
import Info from "../../components/dashboard/info";

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
					<div className="dashboard-subtitle">Subtitle</div>
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
								var channel = this.state.textChannels.find(x => x.id == row.channelId).name;
								return(
									<Card title={channel} size={350} titleSize={150} inputSize={200} key={i} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										{console.log(row)}
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
		var autopurge = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		var textChannels = await response?.json();
		for(var i = 0; i < autopurge.rows.length; i++){
			this.settings.channels.push({ timespan: React.createRef(), mode: React.createRef() });
		}
		this.setState({autopurge: autopurge, textChannels: textChannels});
		
	}

	onChannelAdded(channel){
		var autopurge = this.state.autopurge;
		autopurge.rows.push({
			channelId: channel.id.toString(),
			timespan: "PT5M",
			mode: 0
		});
		this.settings.channels.push({ timespan: React.createRef(), mode: React.createRef() });
		this.setState({autopurge: autopurge});
	}

	onChannelRemoved(id){
		var autopurge = this.state.autopurge;
		autopurge.rows = autopurge.rows.filter(x => x.channelId != id);
		this.settings.channels.pop();
		this.setState({autopurge: autopurge});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.autopurge.rows;
		console.log(rows);
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].timespan = card.timespan.current.getValue();
			rows[i].mode = card.mode.current.getValue();
		}
		this.state.autopurge.rows = rows;
		this.setState({autopurge: this.state.autopurge});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/autopurge`, this.state.autopurge);
		return response.ok;
	}
}

export default Autopurge;
