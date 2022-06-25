import React from "react";
import Helmet from "react-helmet";

import Fade from "../components/effects/fade";
import "../styles/error.css";

export default function Error(props) {
	return (
		<>
			<Helmet>
				<title>{props.shortDescription} - Utili</title>
			</Helmet>
			<Fade>
				<div className="error-container">
					<div className="error-code">
						{props.code}
					</div>
					<div className="error-horizontal-seperator">
						｜
					</div>
					<div className="error-short-description">
						{props.shortDescription}
					</div>
				</div> <br />
				<div className="error-long-description">
					{props.longDescription}
				</div>
			</Fade>
		</>
	);
}
