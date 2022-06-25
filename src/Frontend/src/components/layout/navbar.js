import React from "react";
import Cookies from "universal-cookie";
import { Link } from "react-router-dom";
import { Navbar, Nav } from "react-bootstrap";

import Guilds from "./guilds";
import User from "./user";
import ThemeSelector from "./themeSelector";
import { get } from "../../api/auth";

class MyNavbar extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			background: "dark",
			displayPremium: true
		};
	}

	render() {
		return (
			<div>
				<Navbar variant={this.state.background} expand="lg">
					<div className="container">
						{this.props.buttonLeft ?
							<button className="navbar-toggler" onClick={() => this.props.onButtonLeftClick()}><span className="navbar-toggler-icon"></span></button>
							: null}
						<Navbar.Brand to="/" as={Link}>
							<img src="https://file.utili.xyz/UtiliSmall.png" width="40px" className="d-inline-block align-middle" alt="Utili Logo" />
							Utili
						</Navbar.Brand>
						<Navbar.Toggle aria-controls="my-navbar" />
						<Navbar.Collapse id="my-navbar">
							<Nav className="mr-auto">
								<Nav.Link to="/commands" as={Link}>Commands</Nav.Link>
								<Nav.Link to="/dashboard" as={Link}>Dashboard</Nav.Link>
								{this.state.displayPremium &&
									<Nav.Link to="/premium" as={Link}>Premium</Nav.Link>
								}
							</Nav>
							{this.props.guilds &&
								<Guilds />}
							<User />
							<ThemeSelector onChange={() => this.setNavbarButtonColour()} />
						</Navbar.Collapse>
					</div>
				</Navbar>
			</div>

		);
	}

	async componentDidMount() {
		this.setNavbarButtonColour();

		if (window._freePremium === undefined) {
			var response = await get(`premium/free`);
			var freePremium = await response?.json();
			window._freePremium = freePremium;
			this.setState({ displayPremium: !freePremium });
		}

		this.setState({ displayPremium: !window._freePremium });
	}

	setNavbarButtonColour() {
		this.setState({ background: this.getNavbarButtonColour() });
	}

	getNavbarButtonColour() {
		var cookies = new Cookies();
		var theme = cookies.get("theme");
		if (!theme) theme = "dark";
		switch (theme) {
			case "dark":
				return "dark";
			case "light":
				return "light";
			default:
				return "light";
		}
	}
}

export default MyNavbar;
