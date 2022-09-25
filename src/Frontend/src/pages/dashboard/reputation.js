import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";

class Reputation extends React.Component {
	constructor(props) {
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			reputation: null
		};
		this.settings = {
			emojis: []
		};
	}

	render() {
		return (
			<>
				<Helmet>
					<title>Reputation - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Reputation</div>
					<div className="dashboard-subtitle">Let users award each other with rep points using reactions</div>
					<div className="dashboard-description">
						<p>Each emoji is assigned a value.<br />
							When someone's message gets that reaction, their reputation changes by the emoji's value.<br />
							The emoji's value can be negative.</p>
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
							{this.state.reputation?.emojis.map((emoji, i) => {
								return (
									<Card title={emoji.emoji} size={300} titleSize={150} inputSize={150} key={emoji.emoji} onChanged={this.props.onChanged} onRemoved={() => this.onemojiRemoved(emoji.emoji)}>
										<CardComponent title="Value" type="number" value={emoji.value} ref={this.settings.emojis[i].value} />
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
		var response = await get(`dashboard/${this.guildId}/reputation`);
		this.state.reputation = await response?.json();

		for (var i = 0; i < this.state.reputation.emojis.length; i++) {
			this.settings.emojis.push({ value: React.createRef() });
		}
		this.setState({});
	}


	onemojiRemoved(emoji) {
		this.settings.emojis.pop();
		this.state.reputation.emojis = this.state.reputation.emojis.filter(x => x.emoji != emoji);
		this.setState({});
		this.props.onChanged();
	}

	getInput() {
		var emojis = this.state.reputation.emojis;
		for (var i = 0; i < emojis.length; i++) {
			var card = this.settings.emojis[i];
			emojis[i].value = card.value.current.getValue();
		}
		this.state.reputation.emojis = emojis;
		this.setState({});
	}

	async save() {
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/reputation`, this.state.reputation);
		return response.ok;
	}
}

export default Reputation;
