import { useParams } from "react-router-dom";
import Helmet from "react-helmet";

export default function Index(props){
	var { guild_id } = useParams();
	return(
		<>
			<Helmet>
				<title>Utili Dashboard</title>
			</Helmet>
			Dashboard Core
			{guild_id}
		</>
	);
}
