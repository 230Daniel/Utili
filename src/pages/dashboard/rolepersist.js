import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";

class RolePersist extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			rolePersist: null,
			roles: null
		};
		this.settings = {
			enabled: React.createRef(),
			excludedRoles: React.createRef()
		}
	}

	render(){
		var values = this.state.roles?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Role Persist - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Role Persist</div>
					<div className="dashboard-subtitle">Keep a user's roles if they leave and rejoin</div>
					<div className="dashboard-description">
						<p>Users which leave and rejoin will have their roles given back to them.</p>
					</div>
					<Load loaded={this.state.rolePersist !== null}>
						<Card title="Role Persist Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="checkbox" title="Enabled" value={this.state.rolePersist?.enabled} ref={this.settings.enabled}></CardComponent>
						</Card>
						<Card title="Excluded Roles" size={400} onChanged={this.props.onChanged}>
							<CardListComponent prompt="Exclude a role..." values={values} selected={this.state.rolePersist?.excludedRoles} ref={this.settings.excludedRoles} noReorder={true}></CardListComponent>
						</Card>
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/rolepersist`);
		this.state.rolePersist = await response?.json();
		response = await get(`discord/${this.guildId}/roles`);
		this.state.roles = await response?.json();

		this.setState({});
	}

	getInput(){
		this.state.rolePersist = {
			enabled: this.settings.enabled.current.getValue(),
			excludedRoles: this.settings.excludedRoles.current.getSelected()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/rolepersist`, this.state.rolePersist);
		return response.ok;
	}
}

export default RolePersist;
