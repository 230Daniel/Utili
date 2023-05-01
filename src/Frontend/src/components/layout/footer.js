import React from "react";
import { Link } from "react-router-dom";

class MyFooter extends React.Component {
	render() {
		return (
			<div className="footer">
				<div className="container">
					<div className="footer-left">
						© Daniel Baynton 2023
					</div>
					<div className="footer-right">
						{window.__config.officialInstance &&
							<>
								<Link className="link" to="/terms">Terms</Link> •
								<Link className="link" to="/privacy"> Privacy</Link> •
							</>
						}
						<a className="link" href="https://github.com/230Daniel/Utili">GitHub</a> •
						<a className="link" href="https://github.com/230Daniel/Utili/blob/main/LICENSE.txt">License</a> •
						<Link className="link" to="/contact"> Contact</Link>
					</div>
				</div>
			</div>
		);
	}
}

export default MyFooter;
