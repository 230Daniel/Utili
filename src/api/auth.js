import Cookies from "universal-cookie";

export function getBackend(){
	switch(process.env.NODE_ENV){
		case "production":
			return "https://api.utili.xyz";
		case "development":
		case "test":
			return "https://localhost:5001";
	}
}

export function getClientId(){
	switch(process.env.NODE_ENV){
		case "production":
			return "655155797260501039";
		case "development":
		case "test":
			return "790254880945602600";
	}
}

export async function getDetails(){
	var response = await fetch(`${getBackend()}/auth`, { mode: "cors", credentials: "include" });
	return await response.json();
}

export async function signOut(){
	await fetch(`${getBackend()}/auth/signout`, { method: "POST", credentials: "include" });
}

export async function signIn(){
	const cookies = new Cookies();
	cookies.set("return_path", window.location.pathname, { path: "/return", maxAge: 60, sameSite: "strict" });
	window.location.href = `${getBackend()}/auth/signin`;
}

export async function get(endpoint){
	var result = await fetch(`${getBackend()}/${endpoint}`, { method: "GET", credentials: "include" });
	switch(result.status){
		case 401:
			signIn();
			break;
		case 403:
			window.location.pathname = "dashboard";
			break;
		case 404:
			if(endpoint.includes("dashboard")){
				window.location.pathname = `/invite/${endpoint.split("/")[1]}`;
			}
			break;
		default:
			break;
	}
	return result;
}

export async function post(endpoint, body){
	var result = await fetch(`${getBackend()}/${endpoint}`, { 
		method: "POST",
		headers: {
			"Content-Type": "application/json"
		},
		credentials: "include", 
		body: JSON.stringify(body)
	 });
	switch(result.status){
		case 401:
			signIn();
			break;
		case 403:
			window.location.pathname = "dashboard";
			break;
		case 404:
			if(endpoint.includes("dashboard")){
				window.location.pathname = `/invite/${endpoint.split("/")[1]}`;
			}
			break;
		default:
			break;
	}
	return result;
}
