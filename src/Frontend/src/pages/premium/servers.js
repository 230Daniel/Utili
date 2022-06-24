import React from "react";
import { Helmet } from "react-helmet";
import { get, post } from "../../api/auth";

import Fade from "../../components/effects/fade"
import Load from "../../components/load";
import Divider from "../../components/layout/divider";
import Subscriptions from "../../components/subscriptions";
import Card from "../../components/dashboard/card";

import "../../styles/premium.css";
import CardComponent from "../../components/dashboard/cardComponent";


class PremiumServers extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			guilds: [],
			saveStatus: ""
		};
		this.settings = {
			slots: []
		}
	}

	render(){
		var values = this.state.guilds.map(x => {return {id: x.id, value: x.name}})
		return (
			<>
				<Helmet>
					<title>Premium Servers - Utili</title>
				</Helmet>
				<Fade>
					<div className="container premium">
						<h1>Premium Servers</h1>
						<Load loaded={this.state.slots}>
							<Subscriptions alwaysDisplay={true}/>
							<Divider top={25} bottom={25}>Premium Slots</Divider>
							<div className="inline" style={{justifyContent: "center"}}>
								{this.state.slots?.map((slot, i) =>{
									return(
										<Card title={`Slot ${i + 1}`} size={300} titleSize={0} inputSize={300} key={slot.slotId} onChanged={() => this.requireSave()}>
											<CardComponent type="select-value" value={slot.guildId} values={values} ref={this.settings.slots[i].guildId}/>
										</Card>
									)
								})}
							</div>
						</Load>
						{this.renderSaveButton()}
					</div>
				</Fade>
			</>
		)
	}

	async componentDidMount(){
		window.addEventListener("beforeunload", this.onUnload);
		var response = await get("premium/slots");
		var slots = await response.json();
		
		slots.orderByInt(x => x.slotId);

		this.state.slots = slots;

		response = await get("discord/guilds");
		this.state.guilds = await response.json();

		for(var i = 0; i < this.state.slots.length; i++){
			this.settings.slots.push({ guildId: React.createRef() });
		}

		this.setState({});
	}

	getInput(){
		var slots = this.state.slots;
		for(var i = 0; i < slots.length; i++){
			var card = this.settings.slots[i];
			slots[i].guildId = card.guildId.current.getValue();
		}
		this.state.slots = slots;
		this.setState({});
	}

	async saveBody(){
		this.getInput();
		var response = await post(`premium/slots`, this.state.slots);
		return response.ok;
	}

	doesRequireSave(){
		switch(this.state.saveStatus){
			case "waiting":
			case "saving":
			case "error":
				return true;
			default:
				return false;
		}
	}

	shouldSaveButtonBeVisible(){
		switch(this.state.saveStatus){
			case "waiting":
			case "saving":
			case "saved":
			case "error":
				return true;
			default:
				return false;
		}
	}

	renderSaveButton(){
		return(
			<div className={`saveNotification ${this.shouldSaveButtonBeVisible() ? "visible" : ""}`}>
				<button className={`savebutton-${this.state.saveStatus}`} onClick={async () => await this.save()}>{this.getSaveButtonContent()}</button>
			</div>
		);
	}

	getSaveButtonContent(){
		switch(this.state.saveStatus){
			case "waiting":
				return "Save Changes";
			case "saving":
				return "Saving...";
			case "saved":
			case "hidden":
				return "Changes Saved";
			case "error":
				return "Error";
		}
	}

	requireSave(){
		if(!this.doesRequireSave()){
			this.setState({saveStatus: "waiting"});
		}
	}

	async save(){
		if(!this.doesRequireSave()) return;
		this.setState({saveStatus: "saving"});
		var success = await this.saveBody();
		if(success){
			this.setState({saveStatus: "saved"});
			setTimeout(() => {
				if(!this.doesRequireSave())	this.setState({saveStatus: "hidden"});
			}, 500);
		}
		else this.setState({saveStatus: "error"});
	}

	onUnload = e => {
		if(this.doesRequireSave()){
			e.preventDefault();
			e.returnValue = "You have unsaved changes";
			return "You have unsaved changes";
		}
	 }
 
	 componentWillUnmount() {
		 window.removeEventListener("beforeunload", this.onUnload);
	 }
}

export default PremiumServers;
