import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class ChannelMirroring extends React.Component {
	constructor(props) {
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			channelMirroring: null,
			textChannels: null
		};
		this.settings = {
			channelAdder: React.createRef(),
			channels: []
		};
	}

	render() {
		var channels = this.state.textChannels?.map(x => { return { id: x.id, value: x.name }; });
		var mirroringChannels = channels?.filter(x => this.state.channelMirroring?.some(y => y.channelId == x.id));
		return (
			<>
				<Helmet>
					<title>Channel Mirroring - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Channel Mirroring</div>
					<div className="dashboard-subtitle">Copies new messages from one channel to another</div>
					<Load loaded={this.state.channelMirroring !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent
								prompt="Add a channel..."
								values={channels}
								selected={mirroringChannels}
								onSelected={(id) => this.onChannelAdded(id)}
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder} />
						</Card>
						<div className="inline">
							{this.state.channelMirroring?.map((row, i) => {
								return (
									<Card title={row.channelName} size={350} titleSize={150} inputSize={200} key={row.channelId} onChanged={this.props.onChanged} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent type="select-value" title="Mirror to" values={channels} value={row.destinationChannelId} ref={this.settings.channels[i].destinationChannelId} />
										<CardComponent type="select" title="Author mode" options={["Show in webhook username", "Mention in message (no ping)"]} value={row.authorDisplayMode} ref={this.settings.channels[i].authorDisplayMode} />
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
		var response = await get(`dashboard/${this.guildId}/channel-mirroring`);
		this.state.channelMirroring = await response?.json();
		response = await get(`discord/${this.guildId}/text-channels`);
		this.state.textChannels = await response?.json();

		this.state.channelMirroring = this.state.channelMirroring.filter(x => this.state.textChannels.some(y => y.id == x.channelId));
		for (var i = 0; i < this.state.channelMirroring.length; i++) {
			this.settings.channels.push({ destinationChannelId: React.createRef(), authorDisplayMode: React.createRef() });
			this.state.channelMirroring[i]["channelName"] = this.getChannelName(this.state.channelMirroring[i].channelId);
		}
		this.state.channelMirroring.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelAdded(channel) {
		this.settings.channels.push({ destinationChannelId: React.createRef() });
		this.state.channelMirroring.push({
			channelId: channel.id,
			destinationChannelId: 0,
			authorDisplayMode: 0,
			channelName: this.getChannelName(channel.id)
		});
		this.state.channelMirroring.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id) {
		var index = this.state.channelMirroring.map(x => x.channelId).indexOf(id);
		this.settings.channels.splice(index, 1);
		this.state.channelMirroring.splice(index, 1);
		this.setState({});
		this.props.onChanged();
	}

	getInput() {
		var rows = this.state.channelMirroring;
		for (var i = 0; i < rows.length; i++) {
			var card = this.settings.channels[i];
			rows[i].destinationChannelId = card.destinationChannelId.current.getValue();
			rows[i].authorDisplayMode = card.authorDisplayMode.current.getValue();
		}
		this.state.channelMirroring = rows;
		this.setState({});
	}

	async save() {
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/channel-mirroring`, this.state.channelMirroring);
		return response.ok;
	}

	getChannelName(id) {
		return this.state.textChannels.find(x => x.id == id).name;
	}
}

export default ChannelMirroring;
