import { SD_General } from "../constants/constants";

const checkForToken = ():string | null => {
    return localStorage.getItem(SD_General.tokenKey);
}

export default checkForToken;