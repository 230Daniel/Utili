import React from "react";
import Helmet from "react-helmet";
import { Duration } from "luxon";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardListComponent from "../../components/dashboard/cardListComponent";

class InactiveRole extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			inactiveRole: null,
			roles: null,
			premium: null
		};
		this.settings = {
			role: React.createRef(),
			immuneRole: React.createRef(),
			threshold: React.createRef(),
			mode: React.createRef(),
			autoKick: React.createRef(),
			autoKickThreshold: React.createRef()
		}
	}

	render(){
		var values = this.state.roles?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Inactive Role - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Inactive Role</div>
					<div className="dashboard-subtitle">Set a role to be given to inactive users</div>
					<div className="dashboard-description">
						<p>A user's inactivity timer is reset if they:</p>
						<ul>
							<li>Send a message</li>
							<li>Join a voice channel</li>
						</ul>
						<p>Activity data is only recorded from when you select an inactive role.<br/>
						After setting an inactive role, you will have to wait for your threshold to pass before you see users marked as inactive.</p>
						<p>Utili will check your server for inactive users once per hour.</p>
						{this.renderDescription()}
					</div>
					<Load loaded={this.state.inactiveRole !== null}>
						<Card title="Inactive Role Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
							<CardComponent type="select-value" title="Inactive Role" values={values} value={this.state.inactiveRole?.roleId} ref={this.settings.role}></CardComponent>
							<CardComponent type="select" title="Mode" options={["Grant when inactive", "Revoke when inactive"]} value={this.state.inactiveRole?.mode} ref={this.settings.mode}></CardComponent>
							<CardComponent type="select-value" title="Immune Role" values={values} value={this.state.inactiveRole?.immuneRoleId} ref={this.settings.immuneRole}></CardComponent>
							<CardComponent type="timespan" title="Threshold" value={Duration.fromISO(this.state.inactiveRole?.threshold)} ref={this.settings.threshold}></CardComponent>
						</Card>
						<div style={{display: this.state.premium ? "flex" : "none"}}>
							<Card title="Auto-Kick Settings" size={400} titleSize={200} inputSize={200} onChanged={this.props.onChanged}>
								<CardComponent type="checkbox" title="Enabled" value={this.state.inactiveRole?.autoKick} ref={this.settings.autoKick}></CardComponent>
								<CardComponent type="timespan" title="Additional Threshold" value={Duration.fromISO(this.state.inactiveRole?.autoKickThreshold)} ref={this.settings.autoKickThreshold}></CardComponent>
							</Card>
						</div>
						
					</Load>
				</Fade>
			</>
		);
	}

	renderDescription(){
		if(!this.state.premium){
			return(
				<p><b>Premium:</b> Adds the option to automatically kick inactive users after an additional threshold.</p>
			);
		}
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/inactive-role`);
		this.state.inactiveRole = await response?.json();
		response = await get(`discord/${this.guildId}/roles`);
		this.state.roles = await response?.json();
		response = await get(`premium/guild/${this.guildId}`);
		this.state.premium = await response?.json();

		this.setState({});
	}

	getInput(){
		this.state.inactiveRole = {
			roleId: this.settings.role.current.getValue(),
			immuneRoleId: this.settings.immuneRole.current.getValue(),
			threshold: this.settings.threshold.current.getValue(),
			mode: this.settings.mode.current.getValue(),
			autoKick: this.settings.autoKick.current.getValue(),
			autoKickThreshold: this.settings.autoKickThreshold.current.getValue()
		};
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/inactive-role`, this.state.inactiveRole);
		return response.ok;
	}
}

export default InactiveRole;
