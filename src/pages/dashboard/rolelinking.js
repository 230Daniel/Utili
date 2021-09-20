import React from "react";
import Helmet from "react-helmet";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

import Card from "../../components/dashboard/card";
import CardComponent from "../../components/dashboard/cardComponent";
import CardAdderComponent from "../../components/dashboard/cardAdderComponent";

class RoleLinking extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			roleLinking: null,
			roles: null,
			premium: null
		};
		this.settings = {
			linkAdder: React.createRef(),
			links: []
		}
	}

	render(){
		var roles = this.state.roles?.map(x => {return {id: x.id, value: x.name}});
		return(
			<>
				<Helmet>
					<title>Role Linking - Utili Dashboard</title>
				</Helmet>
				<Fade>
					<div className="dashboard-title">Role Linking</div>
					<div className="dashboard-subtitle">Add/remove a role when someone is given/stripped of a role</div>
					<div className="dashboard-description">
						<p>These rules apply whenever a user's roles change.</p>
						{this.renderDescription()}
					</div>
					<Load loaded={this.state.roleLinking !== null}>
						<Card onChanged={this.props.onChanged}>
							<CardAdderComponent
								prompt="Add a rule about..." 
								values={roles} 
								selected={[]}
								onSelected={(id) => this.onLinkAdded(id)} 
								ref={this.settings.linkAdder}
								disabled={!this.state.premium && this.state.roleLinking?.length >= 2}/>
						</Card>
						<div className="inline">
							{this.state.roleLinking?.map((row, i) =>{
								var m1 = row.mode == 0 || row.mode == 1 ? 0 : 1;
								var m2 = row.mode == 0 || row.mode == 2 ? 0 : 1;
								return(
									<Card title={this.getRoleName(row.roleId)} size={400} titleSize={200} inputSize={200} key={row.id} onChanged={this.props.onChanged} onRemoved={() => this.onLinkRemoved(row.id)}>
										<CardComponent type="select" title={`When ${this.getRoleName(row.roleId)} is`} value={m1} options={["Added", "Removed"]} ref={this.settings.links[i].m1}></CardComponent>
										<div className="inline">
											<CardComponent type="select" forceWidth={200} onChanged={this.props.onChanged} value={m2} options={["Add", "Remove"]} ref={this.settings.links[i].m2}></CardComponent>
											<CardComponent type="select-value" forceWidth={200} onChanged={this.props.onChanged} noPaddingLeft={true} values={roles} value={row.linkedRoleId} ref={this.settings.links[i].linkedRoleId}></CardComponent>
										</div>
										
									</Card>
								);
							})}
						</div>
					</Load>
				</Fade>
			</>
		);
	}

	renderDescription(){
		if(this.state.premium){
			return null;
		} else {
			return(
				<div>
					<p>On your server, you can have up to two rules.<br/>
					<b>Premium:</b> Have unlimited rules.</p>
				</div>
			);
		}
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/role-linking`);
		this.state.roleLinking = await response?.json();
		console.log(this.state.roleLinking);
		response = await get(`discord/${this.guildId}/roles`);
		this.state.roles = await response?.json();
		response = await get(`premium/guild/${this.guildId}`);
		this.state.premium = await response?.json();

		if(!this.state.premium) this.state.roleLinking = this.state.roleLinking.orderBy(x => x.linkId).slice(0, 2);

		this.state.roleLinking = this.state.roleLinking.filter(x => this.state.roles.some(y => y.id == x.roleId))
		for(var i = 0; i < this.state.roleLinking.length; i++){
			this.settings.links.push({ m1: React.createRef(), m2: React.createRef(), linkedRoleId: React.createRef() });
		}

		this.setState({});
	}

	onLinkAdded(role){
		var id = 0;
		while(this.state.roleLinking.some(x => x.id == id)) id++;

		this.settings.links.push({ m1: React.createRef(), m2: React.createRef(), linkedRoleId: React.createRef() });
		this.state.roleLinking.push({
			id: id,
			roleId: role.id,
			linkedRoleId: "0",
			mode: 0
		});
		this.setState({});
	}

	onLinkRemoved(id){
		this.settings.links.pop();
		this.state.roleLinking = this.state.roleLinking.filter(x => x.id != id);
		this.setState({});
		this.props.onChanged();
	}

	getInput(){
		var rows = this.state.roleLinking;
		for(var i = 0; i < rows.length; i++){
			var card = this.settings.links[i];
			rows[i].linkedRoleId = card.linkedRoleId.current.getValue();

			var m1 = card.m1.current.getValue();
			var m2 = card.m2.current.getValue();
			if(m1 == 0 && m2 == 0) rows[i].mode = 0;
			if(m1 == 0 && m2 == 1) rows[i].mode = 1;
			if(m1 == 1 && m2 == 0) rows[i].mode = 2;
			if(m1 == 1 && m2 == 1) rows[i].mode = 3;
		}
		this.state.roleLinking = rows;
		this.setState({});
	}

	async save(){
		this.getInput();
		var response = await post(`dashboard/${this.guildId}/role-linking`, this.state.roleLinking);
		return response.ok;
	}

	getRoleName(id){
		return this.state.roles.find(x => x.id == id).name;
	}
}

export default RoleLinking;
