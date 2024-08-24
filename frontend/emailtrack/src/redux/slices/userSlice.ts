import { PayloadAction, createSlice } from "@reduxjs/toolkit";
import { user } from "../../interfaces/_index";
import checkForToken from "../../helpers/checkForToken";
import { jwtDecode } from "jwt-decode";

const emptyUser : user = {
    id: -1,
    email: "",
    username: "",
    getWeeklyCsv: false,
    getMonthlyCsv: false
};

const token = checkForToken();

const currentUser = token ? (() => {
    const decodedToken : user = jwtDecode(token);
    return {
        id: decodedToken.id,
        email: decodedToken.email,
        username: decodedToken.username,
        getWeeklyCsv: (decodedToken.getWeeklyCsv.toString() == "True" ? true:false),
        getMonthlyCsv: (decodedToken.getMonthlyCsv.toString() == "True" ? true:false)
    };
}):(emptyUser)

export const userSlice = createSlice({
    name: "user",
    initialState: currentUser,
    reducers:{
        setUser: (state, action : PayloadAction<user>) => {
            console.log("setting user...");
            state.id = action.payload.id;
            state.email = action.payload.email;
            state.username = action.payload.username;
            state.getWeeklyCsv = action.payload.getWeeklyCsv.toString() == "True" ? true:false;
            state.getMonthlyCsv = action.payload.getMonthlyCsv.toString() == "True" ? true:false;
        },
        clearUser: (state) => {
            console.log("logging off now...");
            state.id = -1;
            state.email = "";
            state.username = "";
            state.getWeeklyCsv = false;
            state.getMonthlyCsv = false;
        }
    }
})

export const {setUser,clearUser} = userSlice.actions;
export const userReducer = userSlice.reducer;