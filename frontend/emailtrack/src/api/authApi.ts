import { createApi,fetchBaseQuery } from  '@reduxjs/toolkit/query/react';
import { SD_General } from '../constants/constants';

export const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: fetchBaseQuery({
        baseUrl: process.env.REACT_APP_EMAILSTATS_ENDPOINT,
        prepareHeaders:(headers: Headers, api) => {
            const token = localStorage.getItem(SD_General.tokenKey);
            token && headers.append("Authorization","Bearer " + token); // Pass token so [Authorize] and [Authenticate] can check if user has permission
        }
    }),
    endpoints: (builder) => ({
        deleteUser: builder.mutation({
            query: () => ({
                method: "DELETE",
                url: "/delete-user",
            })
        })
    })
})

export const {useDeleteUserMutation} = authApi
export default authApi;