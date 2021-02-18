import React from "react";
import { Link } from "react-router-dom";
import Helmet from "react-helmet";
import { get } from "../../api/auth";

import "../../styles/dashboard-index.css";

class Index extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			guilds: []
		};
	}

	render(){
		return(
			<>
				<Helmet>
					<title>Dashboard - Utili</title>
				</Helmet>
				<div className="guild-container">
					<div className="guilds">
						{this.state.guilds.map((guild, i) =>{
							return(
								<Link className="guild" to={`/dashboard/${guild.guildId}/test`}>
									<div className="guild-icon">
										<img width="200px" src={guild.iconUrl}/>
									</div>
									<div className="guild-name">
										{guild.name}
									</div>
								</Link>
							);
						})}
					</div>
				</div>
			</>
		);
	}

	async componentDidMount(){
		var response = await get(`dashboard/guilds`);
		var json = await response.json();
		this.setState({guilds: json});
	}
}

export default Index;
