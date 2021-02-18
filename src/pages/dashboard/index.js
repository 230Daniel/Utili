import { Link, useParams } from "react-router-dom";
import Helmet from "react-helmet";

export default function Index(props){
	var { guild_id } = useParams();
	return(
		<>
			<Helmet>
				<title>Dashboard - Utili</title>
			</Helmet>
			<Link to="/dashboard/100/test">Dashboard Index</Link>
			{guild_id}
		</>
	);
}
