import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";
import CardRemoverComponent from "../../components/dashboard/cardRemoverComponent";

class VoteChannels extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			voteChannels: null,
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
		var filterChannels = channels?.filter(x => this.state.voteChannels?.rows.some(y => y.channelId == x.id));
		return(
			<>
				<Helmet>
					<title>Vote Channels - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Vote Channels</div>
					<div className="dashboard-subtitle">Automatically add reactions to messages</div>
					<div className="dashboard-description">
						<p>Add an emoji to a channel using this command:</p>
						<ul>
							<li>votes addEmoji [channel] [emoji]</li>
						</ul>
						<p>{this.renderDescription()}</p>
					</div>
					<Load loaded={this.state.voteChannels !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent 
								prompt="Add a channel..." 
								values={channels} 
								selected={filterChannels}
								onSelected={(id) => this.onChannelAdded(id)} 
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder}/>
						</Card>
						<div className="inline">
							{this.state.voteChannels?.rows.map((row, i) =>{
								return(
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.channelId} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent title="React to" type="select" value={row.mode} options={["All Messages", "Images", "Videos", "Media", "Music", "Attachments", "URLs", "URLs and Media", "Embeds"]} ref={this.settings.channels[i].mode}/>
										<CardRemoverComponent values={row.emotes} render={(emote) => this.renderEmote(emote)} ref={this.settings.channels[i].emotes}/>
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
				<p>On your server, you can have 5 emojis for each vote channel.</p>
			);
		} else {
			return(
				<div>
					<p>On your server, you can have 2 emojis for each vote channel.</p>
					<p><b>Premium:</b> Up to 5 emojis for each vote channel.</p>
				</div>
			);
		}
	}

	renderEmote(emote){
		let regex = new RegExp("^<:.+:([0-9]+)>$");
		if(regex.test(emote)){
			var id = regex.exec(emote)[1];
			return (
				<img src={`https://cdn.discordapp.com/emojis/${id}`} width="24px"/>
			);
		} else {
			return (
				<span style={{fontSize: "24px"}}>{emote}</span>
			);
		}
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/votechannels`);
		this.state.voteChannels = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		this.state.textChannels = await response?.json();
		response = await get(`premium/guild/${this.guildId}`);
		this.state.premium = await response?.json();

		this.state.voteChannels.rows = this.state.voteChannels.rows.filter(x => this.state.textChannels.some(y => y.id == x.channelId))
		for(var i = 0; i < this.state.voteChannels.rows.length; i++){
			this.settings.channels.push({ mode: React.createRef(), emotes: React.createRef() });
			this.state.voteChannels.rows[i]["channelName"] = this.getChannelName(this.state.voteChannels.rows[i].channelId);

			if(this.state.premium) this.state.voteChannels.rows[i].emotes = this.state.voteChannels.rows[i].emotes.slice(0, 5);
			else this.state.voteChannels.rows[i].emotes = this.state.voteChannels.rows[i].emotes.slice(0, 2);
		}
		
		this.state.voteChannels.rows.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelAdded(channel){
		this.settings.channels.push({ mode: React.createRef(), emotes: React.createRef() });
		this.state.voteChannels.rows.push({
			channelId: channel.id,
			mode: -1,
			emotes: [],
			channelName: this.getChannelName(channel.id)
		});
		this.state.voteChannels.rows.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id){
		this.settings.channels.pop();
		this.state.voteChannels.rows = this.state.voteChannels.rows.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.voteChannels.rows;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].mode = card.mode.current.getValue();
			rows[i].emotes = card.emotes.current.getValues();
		}
		this.state.voteChannels.rows = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/votechannels`, this.state.voteChannels);
		return response.ok;
	}

	getChannelName(id){
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default VoteChannels;
