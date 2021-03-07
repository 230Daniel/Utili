import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";

class JoinRoles extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			joinRoles: null,
			roles: null
		};
		this.settings = {
			waitForVerification: React.createRef(),
			joinRoles: React.createRef()
		}
	}

	render(){
		var values = this.state.roles?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Join Roles - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Join Roles</div>
					<div className="dashboard-subtitle">Add roles to users when they join the server</div>
					<div className="dashboard-description">
						<p>A maximum of 5 join roles can be set.</p>
						<p><b>Wait for verification:</b> Don't give the join roles until:</p>
						<ul>
							<li>Membership screening has been completed (if enabled)</li>
							<li>10 minutes have passed (if verification level is set to high)</li>
						</ul>
					</div>
					<Load loaded={this.state.joinRoles !== null}>
						<Card title="Join Roles Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="checkbox" title="Wait for verification" value={this.state.joinRoles?.waitForVerification} ref={this.settings.waitForVerification}></CardComponent>
						</Card>
						<Card title="Join Roles" size={400} onChanged={this.props.onChanged}>
							<CardListComponent prompt="Add a role..." values={values} selected={this.state.joinRoles?.joinRoles} ref={this.settings.joinRoles} max={5} noReorder={true}></CardListComponent>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/joinroles`);
		this.state.joinRoles = await response?.json();
		response = await get(`discord/${this.guildId}/roles`);
		this.state.roles = await response?.json();
		this.state.joinRoles.joinRoles = this.state.joinRoles.joinRoles.filter(x => this.state.roles.some(y => x == y.id))
		this.setState({});
	}

	getInput(){
		this.state.joinRoles = {
			waitForVerification: this.settings.waitForVerification.current.getValue(),
			joinRoles: this.settings.joinRoles.current.getSelected()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/joinroles`, this.state.joinRoles);
		return response.ok;
	}
}

export default JoinRoles;
