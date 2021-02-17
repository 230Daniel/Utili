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

function load(){
	fetch("https://localhost:5001/test/hello")
	.then((response) => response.json())
	.then((json) => { return json; });
}
