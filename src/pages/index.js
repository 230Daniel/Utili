import Helmet from "react-helmet";

export default function Index(props){
	return(
		<>
			<Helmet>
				<title>Utili</title>
			</Helmet>
			Normal Index
		</>
	);
}

function load(){
	fetch("https://localhost:5001/test/hello")
	.then((response) => response.json())
	.then((json) => { return json; });
}
