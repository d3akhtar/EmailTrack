import { useDispatch } from "react-redux";
import apiResponse from "../interfaces/apiResponse";
import { jwtDecode } from "jwt-decode";
import user from "../interfaces/user";
import { SD_General } from "../constants/constants";
import { setUser } from "../redux/slices/userSlice";
import { useNavigate } from "react-router-dom";
import { METHODS } from "http";


const getExternalProfileInfo = async (accessToken: string, refreshToken:string) : Promise<user> => {
    var result = await fetch(`${process.env.REACT_APP_EMAILSTATS_ENDPOINT}/external-login?thirdPartyName=google&accessToken=${accessToken}&refreshToken=${refreshToken}`, {
        method: "POST"
    });
    var response : apiResponse = await result.json();
    console.log(response);

    localStorage.setItem(SD_General.tokenKey,response.token!);
    
    return jwtDecode(response.token!);
}

export default getExternalProfileInfo;