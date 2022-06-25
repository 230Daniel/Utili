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

class VoteChannels extends React.Component {
	constructor(props) {
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
		};
	}

	render() {
		var channels = this.state.textChannels?.map(x => { return { id: x.id, value: x.name }; });
		var voteChannels = channels?.filter(x => this.state.voteChannels?.some(y => y.channelId == x.id));

		// Mode is a flags enum
		/*
			[Flags]
			public enum VoteChannelMode
			{
				All = 1,
				Images = 2,
				Videos = 4,
				Music = 8,
				Attachments = 16,
				Links = 32,
				Embeds = 64
			}
		*/
		// At some point I want to properly allow multi-selection instead of these premade presets
		var modeValues = [
			{ id: 1, value: "All Messages" },
			{ id: 2, value: "Images" },
			{ id: 4, value: "Videos" },
			{ id: 2 + 4, value: "Media" },
			{ id: 8, value: "Music" },
			{ id: 16, value: "Attachments" },
			{ id: 32, value: "URLs" },
			{ id: 32 + 2 + 4, value: "URLs and Media" },
			{ id: 64, value: "Embeds" }
		];

		return (
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
								selected={voteChannels}
								onSelected={(id) => this.onChannelAdded(id)}
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder} />
						</Card>
						<div className="inline">
							{this.state.voteChannels?.map((row, i) => {
								return (
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.channelId} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent title="React to" type="select-value" value={row.mode} values={modeValues} hideNone ref={this.settings.channels[i].mode} />
										<CardRemoverComponent values={row.emojis} render={(emoji) => this.renderEmoji(emoji)} ref={this.settings.channels[i].emojis} />
									</Card>
								);
							})}
						</div>
					</Load>
				</Fade>
			</>
		);
	}

	renderDescription() {
		if (this.state.premium) {
			return (
				<p>On your server, you can add up to 5 emojis in each vote channel.</p>
			);
		} else {
			return (
				<div>
					<p>On your server, you can add up to 2 emojis in each vote channel.<br />
						<b>Premium:</b> Add up to 5 emojis in each vote channel.</p>
				</div>
			);
		}
	}

	renderEmoji(emoji) {
		let regex = new RegExp("^<:.+:([0-9]+)>$");
		if (regex.test(emoji)) {
			var id = regex.exec(emoji)[1];
			return (
				<img src={`https://cdn.discordapp.com/emojis/${id}`} width="24px" />
			);
		} else {
			return (
				<span style={{ fontSize: "24px" }}>{emoji}</span>
			);
		}
	}

	async componentDidMount() {
		var response = await get(`dashboard/${this.guildId}/vote-channels`);
		this.state.voteChannels = await response?.json();
		response = await get(`discord/${this.guildId}/text-channels`);
		this.state.textChannels = await response?.json();
		response = await get(`premium/guild/${this.guildId}`);
		this.state.premium = await response?.json();

		this.state.voteChannels = this.state.voteChannels.filter(x => this.state.textChannels.some(y => y.id == x.channelId));
		for (var i = 0; i < this.state.voteChannels.length; i++) {
			this.settings.channels.push({ mode: React.createRef(), emojis: React.createRef() });
			this.state.voteChannels[i]["channelName"] = this.getChannelName(this.state.voteChannels[i].channelId);

			if (this.state.premium) this.state.voteChannels[i].emojis = this.state.voteChannels[i].emojis.slice(0, 5);
			else this.state.voteChannels[i].emojis = this.state.voteChannels[i].emojis.slice(0, 2);
		}

		this.state.voteChannels.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelAdded(channel) {
		this.settings.channels.push({ mode: React.createRef(), emojis: React.createRef() });
		this.state.voteChannels.push({
			channelId: channel.id,
			mode: 1,
			emojis: [],
			channelName: this.getChannelName(channel.id)
		});
		this.state.voteChannels.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id) {
		this.settings.channels.pop();
		this.state.voteChannels = this.state.voteChannels.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput() {
		var rows = this.state.voteChannels;
		for (var i = 0; i < rows.length; i++) {
			var card = this.settings.channels[i];
			rows[i].mode = card.mode.current.getValue();
			rows[i].emojis = card.emojis.current.getValues();
		}
		this.state.voteChannels = rows;
		this.setState({});
	}

	async save() {
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/vote-channels`, this.state.voteChannels);
		return response.ok;
	}

	getChannelName(id) {
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default VoteChannels;
