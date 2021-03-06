import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class MessageFilter extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			messageFilter: null,
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
		var filterChannels = channels?.filter(x => this.state.messageFilter?.rows.some(y => y.channelId == x.id));
		return(
			<>
				<Helmet>
					<title>Message Filter - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Message Filter</div>
					<div className="dashboard-subtitle">Force a certain type of message in each channel</div>
					<div className="dashboard-description">
						<p>Utili will delete any message that doesn't fit the rule for its channel.</p>
						<p><b>RegEx:</b> A regular expression in C# style which every message must match. (advanced)</p>
					</div>
					<Load loaded={this.state.messageFilter !== null}>
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
							{this.state.messageFilter?.rows.map((row, i) =>{
								return(
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.channelId} onChanged={() => this.onChanged()} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent title="Mode" type="select" value={row.mode} options={["Unrestricted", "Images", "Videos", "Media", "Music", "Attachments", "URLs", "URLs and Media", "RegEx (advanced)"]} ref={this.settings.channels[i].mode}/>
										<CardComponent title="RegEx (C#)" type="text" value={row.complex} visible={row.mode == 8} ref={this.settings.channels[i].complex}/>
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
		var response = await get(`dashboard/${this.guildId}/messagefilter`);
		this.state.messageFilter = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		this.state.textChannels = await response?.json();

		for(var i = 0; i < this.state.messageFilter.rows.length; i++){
			this.settings.channels.push({ mode: React.createRef(), complex: React.createRef() });
			this.state.messageFilter.rows[i]["channelName"] = this.getChannelName(this.state.messageFilter.rows[i].channelId);
		}
		this.sortChannels();
		this.setState({});
	}

	onChanged(){
		this.getInput();
		this.setState({});
		this.props.onChanged();
	}

	onChannelAdded(channel){
		this.settings.channels.push({ mode: React.createRef(), complex: React.createRef() });
		this.state.messageFilter.rows.push({
			channelId: channel.id,
			mode: -1,
			complex: "",
			channelName: this.getChannelName(channel.id)
		});
		this.sortChannels();
		this.setState({});
	}

	onChannelRemoved(id){
		this.settings.channels.pop();
		this.state.messageFilter.rows = this.state.messageFilter.rows.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.messageFilter.rows;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.channels[i];
			rows[i].mode = card.mode.current.getValue();
			rows[i].complex = card.complex.current.getValue();
		}
		this.state.messageFilter.rows = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/messagefilter`, this.state.messageFilter);
		return response.ok;
	}

	sortChannels(){
		this.state.messageFilter.rows.sort((a, b) => (a.channelName > b.channelName) ? 1 : -1)
	}

	getChannelName(id){
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default MessageFilter;
