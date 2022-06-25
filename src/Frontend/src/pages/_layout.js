import React from "react";

import Navbar from "../components/layout/navbar";
import Footer from "../components/layout/footer";

import ResetPage from "../components/effects/reset";

import "../styles/layout.css";
import LoadAntiForgery from "../components/loadAntiForgery";

export default function Layout(props) {
	return (
		<>
			<main>
				<Navbar />
				<ResetPage />
				<LoadAntiForgery>
					{props.children}
				</LoadAntiForgery>
			</main>
			<footer>
				<Footer />
			</footer>
		</>
	);
}
