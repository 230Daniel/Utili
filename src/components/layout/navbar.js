import React from "react";
import { Link } from "react-router-dom";
import { Navbar, Nav } from "react-bootstrap";

import User from "./user";
import ThemeSelector from "./themeSelector";

class MyNavbar extends React.Component{
	render(){
		return(
			<div>
				<Navbar expand="lg">
					<div className="container">
						<Navbar.Brand to="/" as={Link}>
						<img src="https://file.utili.xyz/UtiliSmall.png" width="40px" className="d-inline-block align-middle" alt="Utili Logo"/>
							Utili
						</Navbar.Brand>
						<Navbar.Toggle aria-controls="my-navbar" />
						<Navbar.Collapse id="my-navbar">
							<Nav className="mr-auto">
								<Nav.Link to="/commands" as={Link}>Commands</Nav.Link>
								<Nav.Link to="/dashboard" as={Link}>Dashboard</Nav.Link>
								<Nav.Link to="/premium" as={Link}>Premium</Nav.Link>
							</Nav>
							<User/>
							<ThemeSelector/>
						</Navbar.Collapse>
					</div>
				</Navbar>
			</div>
			
		);
	}
}

export default MyNavbar;
