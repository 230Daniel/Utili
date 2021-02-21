import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";
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
			channels: []
		}
	}

	render(){
		var values = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		console.log(this.state.autopurge);
		return(
			<>
				<Helmet>
					<title>Autopurge - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Autopurge</div>
					<div className="dashboard-subtitle">Subtitle</div>
					<Load loaded={this.state.autopurge !== null}>
							{this.state.autopurge?.rows.map((row, i) =>{
								var channel = this.state.textChannels.find(x => x.id == row.channelId).name;
								return(
									<Card title={channel} size={400} titleSize={200} inputSize={200} key={i} onChanged={this.props.onChanged}>
										<CardComponent title="Threshold" type="timespan" value={Duration.fromISO(row.timespan)} ref={this.settings.channels[i].timespan}/>
									</Card>
								);
							})}
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
			this.settings.channels.push({ timespan: React.createRef() });
		}
		this.setState({autopurge: autopurge, textChannels: textChannels});
		
	}

	getInput(){
		var rows = this.state.autopurge.rows;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].timespan = card.timespan.current.getValue();
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
