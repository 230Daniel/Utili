import {getBackend} from "./auth";

export async function ping(){
	try{
		var response = await fetch(`${getBackend()}/status`, { method: "GET" });
		return response.ok;
	}
	catch{
		return false;
	}
}
