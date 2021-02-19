import React from "react";
import Helmet from "react-helmet";
import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";

class Core extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			core: null
		};
		this.settings = {
			nickname: React.createRef(),
			prefix: React.createRef(),
			enableCommands: React.createRef()
		}
	}

	render(){
		return(
			<>
				<Helmet>
					<title>Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Utili Dashboard</div>
					<div className="dashboard-subtitle">Select a feature to configure from the sidebar</div>
					<Load loaded={this.state.core !== null}>
						<Card title="Core Settings" size={400} titleSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="text" title="Nickname" value={this.state.core?.nickname} ref={this.settings.nickname}></CardComponent>
							<CardComponent type="text" title="Command Prefix" value={this.state.core?.prefix} ref={this.settings.prefix}></CardComponent>
							<CardComponent type="checkbox" title="Enable Commands" value={this.state.core?.enableCommands} ref={this.settings.enableCommands}></CardComponent>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/core`);
		var json = await response?.json();
		this.setState({core: json});
	}

	getInput(){
		this.state.core = {
			nickname: this.settings.nickname.current.getValue(),
			prefix: this.settings.prefix.current.getValue(),
			enableCommands: this.settings.enableCommands.current.getValue(),
			excludedChannels: []
		};
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/core`, this.state.core);
		return response.ok;
	}
}

export default Core;
