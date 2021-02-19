import React from "react";
import { Link } from "react-router-dom";

class MyFooter extends React.Component{
	render(){
		return(
			<div className="footer">
				<div className="container">
					<div className="footer-left">
						© Daniel Baynton 2021 <br/>
					</div>
					<div className="footer-right">
						<Link className="link-white" to="/terms">Terms</Link>•
						<Link className="link-white" to="/privacy">Privacy</Link>•
						<Link className="link-white" to="/contact">Contact</Link>
					</div>
				</div>
			</div>
		);
	}
}

export default MyFooter;
