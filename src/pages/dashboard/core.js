import React from "react";
import Helmet from "react-helmet";
import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";
import Info from "../../components/dashboard/info";

class Core extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			core: null,
			textChannels: null
		};
		this.settings = {
			nickname: React.createRef(),
			prefix: React.createRef(),
			enableCommands: React.createRef(),
			excludedChannels: React.createRef()
		}
	}

	render(){
		var values = this.state.textChannels?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Utili Dashboard</div>
					<div className="dashboard-subtitle">Select a feature to configure from the sidebar</div>
					<div className="dashboard-description">
						<p>Welcome to the Utili Dashboard.<br/>
						The dashboard can only be accessed by people with the <b>Manage Server</b> permission.</p>
					</div>
					<Load loaded={this.state.core !== null}>
							<Card title="Core Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
								{/*<CardComponent type="text" title="Nickname" value={this.state.core?.nickname} ref={this.settings.nickname}></CardComponent>*/}
								<CardComponent type="text" title="Command Prefix" value={this.state.core?.prefix} ref={this.settings.prefix}></CardComponent>
								<CardComponent type="checkbox" title="Enable Commands" value={this.state.core?.enableCommands} ref={this.settings.enableCommands}></CardComponent>
							</Card>
							<Card title={this.state.core?.enableCommands ? "Block commands in..." : "Allow commands in..."} size={400} onChanged={this.props.onChanged}>
								<CardListComponent prompt="Add a channel..." values={values} selected={this.state.core?.excludedChannels} ref={this.settings.excludedChannels}></CardListComponent>
							</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/core`);
		var core = await response?.json();
		response = await get(`discord/${this.guildId}/channels/text`);
		var textChannels = await response?.json();
		core.excludedChannels = core.excludedChannels.filter(x => textChannels.some(y => x == y.id));

		this.setState({core: core, textChannels: textChannels});
		
	}

	getInput(){
		this.state.core = {
			nickname: this.settings.nickname.current.getValue(),
			prefix: this.settings.prefix.current.getValue(),
			enableCommands: this.settings.enableCommands.current.getValue(),
			excludedChannels: this.settings.excludedChannels.current.getSelected()
		};
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/core`, this.state.core);
		return response.ok;
	}
}

export default Core;
