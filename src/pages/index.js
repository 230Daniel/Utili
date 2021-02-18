import Helmet from "react-helmet";
import Fade from "../components/effects/fade";

import "../styles/index.css";

export default function Index(props){
	return(
		<>
			<Helmet>
				<title>Utili</title>
			</Helmet>
			<Fade>
				<div className="index-container">
					<div className="left">
						<img src="https://file.utili.xyz/Utili.png"/>
					</div>
					<div className="right">
						<div className="text-white" display="block">
							<div className="title">Utili</div>
							<span className="subtitle">A Discord bot with </span>
							<span className="subtitle text-highlight">join messages</span> <br/>
							<a class="subtitle a-blue" href="/dashboard">Get started ➔</a>
						</div>
					</div>
				</div>
			</Fade>
		</>
	);
}
