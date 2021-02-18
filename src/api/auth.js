import Cookies from "universal-cookie";
import backend from "../config/backend.json";

export async function getDetails(){
	var response = await fetch(`${backend.host}/auth`, { mode: "cors", credentials: "include" });
	return await response.json();
}

export async function signOut(){
	await fetch(`${backend.host}/auth/signout`, { method: "POST", credentials: "include" });
}

export async function signIn(){
	const cookies = new Cookies();
	cookies.set("return_path", window.location.pathname, { path: "/return", maxAge: 60, sameSite: "strict" });
	window.location.href = `${backend.host}/auth/signin`;
}

export async function get(endpoint){
	var result = await fetch(`${backend.host}/${endpoint}`, { method: "GET", credentials: "include" });
	switch(result.status){
		case 401:
			signIn();
			break;
		case 403:
			return null;
		case 404:
			if(endpoint.includes("dashboard")){
				const cookies = new Cookies();
				cookies.set("return_path", window.location.pathname, { path: "/return", maxAge: 60, sameSite: "strict" });
				window.location.href = "https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
                        `&client_id=${backend.discord.clientId}` +
                        `&guild_id=${endpoint.split("/")[1]}` +
                        `&redirect_uri=http%3A%2F%2F${window.location.host}%2Freturn`;
			} else return result;
			break;
		default:
			return result;
	}
}

export async function post(endpoint, body){
	var result = await fetch(`${backend.host}/${endpoint}`, { method: "POST", credentials: "include", body: JSON.stringify(body) });
	switch(result.status){
		case 401:
			signIn();
			break;
		case 403:
			if(endpoint.includes("dashboard")){
				window.location.pathname = "dashboard";
			}
			break;
		case 404:
			if(endpoint.includes("dashboard")){
				window.location.href = "https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
                        "&client_id=790254880945602600" +
                        `&guild_id=${endpoint.split("/")[1]}` +
                        `&redirect_uri=https%3A%2F%2F${window.location.host}%2Freturn`;
			} else return result;
			break;
		default:
			return result;
	}
}
