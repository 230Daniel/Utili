import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class Notices extends React.Component {
	constructor(props) {
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			notices: null,
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
		var noticesChannels = channels?.filter(x => this.state.notices?.some(y => y.channelId == x.id));
		return (
			<>
				<Helmet>
					<title>Sticky Notices - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Sticky Notices</div>
					<div className="dashboard-subtitle">Keeps a message at the bottom of a channel</div>
					<div className="dashboard-description">
						<ul>
							<li>You can press <b>enter</b> in the larger text boxes to add a new line</li>
						</ul>
						<p>Use this command to preview your notice:</p>
						<ul>
							<li>notice preview</li>
						</ul>
						<p><b>Delay:</b> The time that the channel must be inactive for the notice to be re-sent.</p>
					</div>
					<Load loaded={this.state.notices !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent
								prompt="Add a channel..."
								values={channels}
								selected={noticesChannels}
								onSelected={(id) => this.onChannelAdded(id)}
								onUnselected={(id) => this.onChannelRemoved(id)}
								ref={this.settings.channelAdder} />
						</Card>
						<div className="inline">
							{this.state.notices?.map((row, i) => {
								return (
									<Card title={row.channelName} size={600} titleSize={200} inputSize={400} key={row.channelId} onChanged={() => this.onCardChanged(i)} onRemoved={() => this.onChannelRemoved(row.channelId)}>
										<CardComponent type="checkbox" title="Enabled" value={row.enabled} ref={this.settings.channels[i].enabled} />
										<CardComponent type="timespan" title="Delay" value={Duration.fromISO(row.delay)} ref={this.settings.channels[i].delay} />
										<CardComponent type="checkbox" title="Pin Message" value={row.pin} visible={!row.channelIsVoice} ref={this.settings.channels[i].pin} />
										<CardComponent type="text" title="Title" value={row.title} ref={this.settings.channels[i].title} />
										<CardComponent type="text-multiline" title="Content" height={80} padding={16} value={row.content} ref={this.settings.channels[i].content} />
										<CardComponent type="text" title="Footer" value={row.footer} ref={this.settings.channels[i].footer} />
										<CardComponent type="text-multiline" title="Plain Text" height={80} padding={16} value={row.text} ref={this.settings.channels[i].text} />
										<CardComponent type="text" title="Image Url" value={row.image} ref={this.settings.channels[i].image} />
										<CardComponent type="text" title="Thumbnail Url" value={row.thumbnail} ref={this.settings.channels[i].thumbnail} />
										<CardComponent type="text" title="Icon Url" value={row.icon} ref={this.settings.channels[i].icon} />
										<CardComponent type="colour" title="Colour" value={row.colour} ref={this.settings.channels[i].colour} />
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
		var response = await get(`dashboard/${this.guildId}/notices`);
		this.state.notices = await response?.json();
		response = await get(`discord/${this.guildId}/message-channels`);
		this.state.textChannels = await response?.json();

		this.state.notices = this.state.notices.filter(x => this.state.textChannels.some(y => y.id == x.channelId));
		for (var i = 0; i < this.state.notices.length; i++) {
			this.settings.channels.push({
				enabled: React.createRef(),
				delay: React.createRef(),
				pin: React.createRef(),
				title: React.createRef(),
				content: React.createRef(),
				footer: React.createRef(),
				text: React.createRef(),
				image: React.createRef(),
				thumbnail: React.createRef(),
				icon: React.createRef(),
				colour: React.createRef()
			});
			var channel = this.getChannel(this.state.notices[i].channelId);
			this.state.notices[i]["channelName"] = channel.name;
			this.state.notices[i]["channelIsVoice"] = channel.isVoice;
		}
		this.state.notices.orderBy(x => x.channelName);
		this.setState({});
	}

	onCardChanged(i) {
		this.state.notices[i].changed = true;
		this.props.onChanged();
	}

	onChannelAdded(channel) {
		this.settings.channels.push({
			enabled: React.createRef(),
			delay: React.createRef(),
			pin: React.createRef(),
			title: React.createRef(),
			content: React.createRef(),
			footer: React.createRef(),
			text: React.createRef(),
			image: React.createRef(),
			thumbnail: React.createRef(),
			icon: React.createRef(),
			colour: React.createRef()
		});

		var channel = this.getChannel(channel.id);

		this.state.notices.push({
			channelId: channel.id,
			enabled: false,
			delay: "PT5M",
			pin: true,
			title: "",
			content: "",
			footer: "",
			text: "",
			image: "",
			thumbnail: "",
			icon: "",
			colour: "43b581",
			channelName: channel.name,
			channelIsVoice: channel.isVoice
		});
		this.state.notices.orderBy(x => x.channelName);
		this.setState({});
	}

	onChannelRemoved(id) {
		this.settings.channels.pop();
		this.state.notices = this.state.notices.filter(x => x.channelId != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput() {
		var rows = this.state.notices;
		for (var i = 0; i < rows.length; i++) {
			var card = this.settings.channels[i];
			rows[i].enabled = card.enabled.current.getValue();
			rows[i].delay = card.delay.current.getValue();
			rows[i].pin = card.pin.current.getValue();
			rows[i].title = card.title.current.getValue();
			rows[i].content = card.content.current.getValue();
			rows[i].footer = card.footer.current.getValue();
			rows[i].text = card.text.current.getValue();
			rows[i].image = card.image.current.getValue();
			rows[i].thumbnail = card.thumbnail.current.getValue();
			rows[i].icon = card.icon.current.getValue();
			rows[i].colour = card.colour.current.getValue();
		}
		this.state.notices = rows;
		this.setState({});
	}

	async save() {
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/notices`, this.state.notices);
		for (var i = 0; i < this.state.notices.length; i++) {
			this.state.notices[i].changed = false;
		}
		return response.ok;
	}

	getChannel(id) {
		return this.state.textChannels.find(x => x.id == id);
	}
}

export default Notices;
