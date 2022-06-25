import React from "react";
import { Link } from "react-router-dom";
import Helmet from "react-helmet";
import { get } from "../../api/auth";

import "../../styles/dashboard-index.css";
import Fade from "../../components/effects/fade";
import Load from "../../components/load";

class Index extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			guilds: null
		};
	}

	render() {
		return (
			<>
				<Helmet>
					<title>Dashboard - Utili</title>
				</Helmet>
				<Fade>
					<h1>Utili Dashboard</h1>
					<h2>Choose a server</h2>
					<Load loaded={this.state.guilds !== null}>
						<div className="guild-container">
							<div className="guilds">
								{this.state.guilds?.filter(x => x.isManageable).map((guild, i) => {
									return (
										<Link className="guild" to={`dashboard/${guild.id}`} key={i}>
											<div className="guild-icon">
												<img width="200px" src={guild.iconUrl} />
											</div>
											<div className="guild-name">
												{guild.name}
											</div>
										</Link>
									);
								})}
							</div>
						</div>
					</Load>
				</Fade>
			</>
		);
	}

	async componentDidMount() {
		var response = await get(`discord/guilds`);
		var json = await response.json();
		this.setState({ guilds: json });
	}
}

export default Index;
