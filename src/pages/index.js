
export default function Index(props){
	return(
		<>
			Normal Index
		</>
	);
}

function load(){
	fetch("https://localhost:5001/test/hello")
	.then((response) => response.json())
	.then((json) => { return json; });
}
