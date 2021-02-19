import React from "react";

import Navbar from "../../components/layout/navbar";
import Sidebar from "../../components/layout/sidebar";
import Footer from "../../components/layout/footer";
import { CheckBackend } from "../_layout";
import ResetPage from "../../components/effects/reset";

import "../../styles/layout.css";

export default function Layout(props){
	return(
		<>
			<Sidebar>
				<CheckBackend>
					{props.children}
				</CheckBackend>
			</Sidebar>
		</>
	);
}
