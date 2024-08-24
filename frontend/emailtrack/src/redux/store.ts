import { configureStore } from "@reduxjs/toolkit";
import { userReducer } from "./slices/_index";
import emailStatsApi from "../api/emailStatsApi";
import authApi from "../api/authApi";
import teamApi from "../api/teamApi";

const store = configureStore({
    reducer:{
        userStore: userReducer,
        [emailStatsApi.reducerPath]: emailStatsApi.reducer,
        [authApi.reducerPath]: authApi.reducer,
        [teamApi.reducerPath]: teamApi.reducer
    },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware().
    concat(emailStatsApi.middleware).
    concat(authApi.middleware).
    concat(teamApi.middleware)
})

export type RootState = ReturnType<typeof store.getState>;

export default store;