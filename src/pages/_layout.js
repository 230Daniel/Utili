import Navbar from "../components/layout/navbar";
import Footer from "../components/layout/footer";

import "../styles/layout.css";

export default function Layout(props){
	return(
		<>
			<main>
				<Navbar/>
				<div className="container">
					{props.children}
				</div>
			</main>
			<footer>
				<Footer/>
			</footer>
		</>
	);
}
