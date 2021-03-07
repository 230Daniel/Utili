import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";

class Reputation extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			reputation: null
		};
		this.settings = {
			emotes: []
		}
	}

	render(){
		return(
			<>
				<Helmet>
					<title>Reputation - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Reputation</div>
					<div className="dashboard-subtitle">Let users award each other with rep points using reactions</div>
					<div className="dashboard-description">
						<p>Each emoji is assigned a value.<br/>
						When someone's message gets that reaction, their reputation changes by the emoji's value.<br/>
						The emoji's value can be negative.</p>
						<p>Commands</p>
						<ul>
							<li>rep addEmoji [emoji] [value]</li>
							<li>rep [user]</li>
							<li>rep top</li>
							<li>rep give [user] [amount]</li>
							<li>rep take [user] [amount]</li>
							<li>rep set [user] [amount]</li>
						</ul>
					</div>
					<Load loaded={this.state.reputation !== null}>
						<div className="inline">
							{this.state.reputation?.emotes.map((emote, i) =>{
								return(
									<Card title={emote.emote} size={300} titleSize={150} inputSize={150} key={emote.emote} onChanged={this.props.onChanged} onRemoved={() => this.onEmoteRemoved(emote.emote)}>
										<CardComponent title="Value" type="number" value={emote.value} ref={this.settings.emotes[i].value}/>
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
		var response = await get(`dashboard/${this.guildId}/reputation`);
		this.state.reputation = await response?.json();

		for(var i = 0; i < this.state.reputation.emotes.length; i++){
			this.settings.emotes.push({ value: React.createRef() });
		}
		this.sortChannels();
		this.setState({});
	}


	onEmoteRemoved(emote){
		this.settings.emotes.pop();
		this.state.reputation.emotes = this.state.reputation.emotes.filter(x => x.emote != emote);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var emotes = this.state.reputation.emotes;
		for(var i = 0; i < emotes.length; i++){
			var card = this.settings.emotes[i];
			emotes[i].value = card.value.current.getValue();
		}
		this.state.reputation.emotes = emotes;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/reputation`, this.state.reputation);
		return response.ok;
	}

	sortChannels(){
		this.state.reputation.emotes.sort((a, b) => (a.channelName > b.channelName) ? 1 : -1)
	}

	getChannelName(id){
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default Reputation;
