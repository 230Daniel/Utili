import React from "react";
import Loader from "react-loader-spinner";

class Load extends React.Component{
	render(){
		return(
			<div style={{display: "flex", justifyContent: "center", marginTop: "75px"}}>
				<Loader type="ThreeDots" color="#ffffff" secondaryColor="#eeeeee" height={60} width={60}/>
			</div>
		);
	}
}

export default Load;
