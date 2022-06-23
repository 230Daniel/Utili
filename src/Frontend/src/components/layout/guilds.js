import React from "react";
import { withRouter } from "react-router-dom";
import { NavDropdown } from "react-bootstrap";

import { get } from "../../api/auth";

class Guilds extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params[0].split("/")[0];
		this.state = {
			guilds: null
		};
	}

	render(){
		if(this.state.guilds == null) return null;
		var guild = this.state.guilds.find(x => x.id == this.guildId);
		if(guild == null) return null;
		return(
			<NavDropdown className="guild-dropdown" title={<><img src={guild.iconUrl}/>{guild.name}</>}>
				{this.state.guilds.filter(x => x.isManageable).map(guild =>{
					return(
						<NavDropdown.Item href={`/dashboard/${guild.id}`} className={`guild-dropdown-link-mutual`} key={guild.id}><img src={guild.iconUrl}/>{guild.name}</NavDropdown.Item>
					);
				})}
			</NavDropdown>
		);
	}

	async componentDidMount(){
		var response = await get(`discord/guilds`);
		var json = await response.json();
		this.setState({guilds: json});
	}
}

export default withRouter(Guilds);
