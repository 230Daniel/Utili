import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";

class JoinMessage extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			joinMessage: null,
			textChannels: null
		};
		this.settings = {
			enabled: React.createRef(),
			direct: React.createRef(),
			channelId: React.createRef(),
			title: React.createRef(),
			content: React.createRef(),
			footer: React.createRef(),
			text: React.createRef(),
			image: React.createRef(),
			thumbnail: React.createRef(),
			icon: React.createRef(),
			colour: React.createRef()
		}
	}

	render(){
		var channels = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Voice Link - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Join Message</div>
					<div className="dashboard-subtitle">Send a custom message when a user joins the server</div>
					<div className="dashboard-description">
						<ul>
							<li>Use <b>%user%</b> to @mention the user</li>
							<li>Use <b>user</b> as an image url to use the user's avatar</li>
						</ul>
						<p>Commands</p>
						<ul>
							<li>joinmessage preview - Preview the join message</li>
						</ul>
					</div>
					<Load loaded={this.state.joinMessage !== null}>
						<Card title="Join Message Settings" size={600} titleSize={200} inputSize={400} onChanged={() => this.onChanged()}>
							<CardComponent type="checkbox" title="Enabled" value={this.state.joinMessage?.enabled} ref={this.settings.enabled}/>
							<CardComponent type="checkbox" title="Direct Message" value={this.state.joinMessage?.direct} ref={this.settings.direct}/>
							<CardComponent type="select-value" title="Channel" visible={!this.state.joinMessage?.direct} values={channels} value={this.state.joinMessage?.channelId} ref={this.settings.channelId}/>
							<CardComponent type="text" title="Title" value={this.state.joinMessage?.title} ref={this.settings.title}/>
							<CardComponent type="text-multiline" title="Content" height={80} padding={16} value={this.state.joinMessage?.content} ref={this.settings.content}/>
							<CardComponent type="text" title="Footer" value={this.state.joinMessage?.footer} ref={this.settings.footer}/>
							<CardComponent type="text-multiline" title="Plain Text" height={80} padding={16} value={this.state.joinMessage?.text} ref={this.settings.text}/>
							<CardComponent type="text" title="Image Url" value={this.state.joinMessage?.image} ref={this.settings.image}/>
							<CardComponent type="text" title="Thumbnail Url" value={this.state.joinMessage?.thumbnail} ref={this.settings.thumbnail}/>
							<CardComponent type="text" title="Icon Url" value={this.state.joinMessage?.icon} ref={this.settings.icon}/>
							<CardComponent type="colour" title="Colour" value={this.state.joinMessage?.colour} ref={this.settings.colour}/>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/joinmessage`);
		this.state.joinMessage = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		this.state.textChannels = await response?.json();

		this.setState({});
	}

	onChanged(){
		this.getInput();
		this.props.onChanged();
	}

	getInput(){
		this.state.joinMessage = {
			enabled: this.settings.enabled.current.getValue(),
			direct: this.settings.direct.current.getValue(),
			channelId: this.settings.channelId.current.getValue(),
			title: this.settings.title.current.getValue(),
			content: this.settings.content.current.getValue(),
			footer: this.settings.footer.current.getValue(),
			text: this.settings.text.current.getValue(),
			image: this.settings.image.current.getValue(),
			thumbnail: this.settings.thumbnail.current.getValue(),
			icon: this.settings.icon.current.getValue(),
			colour: this.settings.colour.current.getValue()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/joinmessage`, this.state.joinMessage);
		return response.ok;
	}
}

export default JoinMessage;
