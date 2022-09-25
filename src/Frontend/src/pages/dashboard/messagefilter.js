import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class MessageFilter extends React.Component {
	constructor(props) {
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
		};
	}

	render() {
		var channels = this.state.textChannels?.map(x => { return { id: x.id, value: x.name }; });
		var filterChannels = channels?.filter(x => this.state.messageFilter?.some(y => y.channelId == x.id));

		// Mode is a flags enum
		/*
			[Flags]
			public enum MessageFilterMode
			{
				All = 1,
				Images = 2,
				Videos = 4,
				Music = 8,
				Attachments = 16,
				Links = 32,
				RegEx = 64
			}
		*/
		// At some point I want to properly allow multi-selection instead of these premade presets
		var modeValues = [
			{ id: 1, value: "Unrestricted" },
			{ id: 2, value: "Images" },
			{ id: 4, value: "Videos" },
			{ id: 2 + 4, value: "Media" },
			{ id: 8, value: "Music" },
			{ id: 16, value: "Attachments" },
			{ id: 32, value: "URLs" },
			{ id: 32 + 2 + 4, value: "URLs and Media" },
			{ id: 64, value: "RegEx (advanced)" }
		];

		return (
			<>
				<Helmet>
					<title>Message Filter - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Message Filter</div>
					<div className="dashboard-subtitle">Force a certain type of message in each channel</div>
					<div className="dashboard-description">
						<p>Utili will delete any message that doesn't fit the rule for its channel.</p>
						<p><b>Message:</b> The message that will be sent if someone's message is deleted - Leave blank for a default message</p>
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
								ref={this.settings.channelAdder} />
						</Card>
						<div className="inline">
							{this.state.messageFilter?.map((row, i) => {
								return (
									<Card title={row.channelName} size={400} titleSize={200} inputSize={200} key={row.channelId} onChanged={() => this.onChanged()} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent title="Mode" type="select-value" value={row.mode} values={modeValues} hideNone ref={this.settings.channels[i].mode} />
										<CardComponent title="RegEx (C#)" type="text" value={row.regEx} visible={row.mode & 64} ref={this.settings.channels[i].regEx} />
										<CardComponent title="Message" type="text" value={row.deletionMessage} ref={this.settings.channels[i].deletionMessage} />
										<CardComponent title="Enforce in threads" type="checkbox" value={row.enforceInThreads} ref={this.settings.channels[i].enforceInThreads} />
									</Card>
								);
							})}
						</div>
					</Load>
				</Fade>
			</>
		);
	}

	async componentDidMount() {
		var response = await get(`dashboard/${this.guildId}/message-filter`);
		this.state.messageFilter = await response?.json();
		response = await get(`discord/${this.guildId}/message-channels`);
		this.state.textChannels = await response?.json();

		this.state.messageFilter = this.state.messageFilter.filter(x => this.state.textChannels.some(y => y.id == x.channelId));
		for (var i = 0; i < this.state.messageFilter.length; i++) {
			this.settings.channels.push({ mode: React.createRef(), regEx: React.createRef(), deletionMessage: React.createRef(), enforceInThreads: React.createRef() });
			this.state.messageFilter[i]["channelName"] = this.getChannelName(this.state.messageFilter[i].channelId);
		}
		this.state.messageFilter.orderBy(x => x.channelName);
		this.setState({});
	}

	onChanged() {
		this.getInput();
		this.setState({});
		this.props.onChanged();
	}

	onChannelAdded(channel) {
		this.settings.channels.push({ mode: React.createRef(), regEx: React.createRef(), deletionMessage: React.createRef(), enforceInThreads: React.createRef() });
		this.state.messageFilter.push({
			channelId: channel.id,
			mode: 1,
			regEx: "",
			deletionMessage: "",
			enforceInThreads: false,
			channelName: this.getChannelName(channel.id)
		});
		this.state.messageFilter.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id) {
		this.settings.channels.pop();
		this.state.messageFilter = this.state.messageFilter.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput() {
		var rows = this.state.messageFilter;
		for (var i = 0; i < rows.length; i++) {
			var card = this.settings.channels[i];
			rows[i].mode = card.mode.current.getValue();
			rows[i].regEx = card.regEx.current.getValue();
			rows[i].deletionMessage = card.deletionMessage.current.getValue();
			rows[i].enforceInThreads = card.enforceInThreads.current.getValue();
		}
		this.state.messageFilter = rows;
		this.setState({});
	}

	async save() {
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/message-filter`, this.state.messageFilter);
		return response.ok;
	}

	getChannelName(id) {
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default MessageFilter;
