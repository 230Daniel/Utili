import React from "react";
import Helmet from "react-helmet";
import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import { get, post } from "../../api/auth";

class Core extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			core: null
		};
	}

	render(){
		return(
			<>
				<Helmet>
					<title>Utili Dashboard</title>
				</Helmet>
				<Fade>
					<Load loaded={this.state.core !== null}>
						{JSON.stringify(this.state.core)}
					</Load>
				</Fade>
			</>
		);
	}
	
	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/core`);
		var json = await response.json();
		this.setState({core: json});
	}

	async save(){
		try{
			var response = await post(`dashboard/${this.guildId}/core`, this.state.core);
			return response.ok;
		}
		catch{
			return false;
		}
	}
}

export default Core;
