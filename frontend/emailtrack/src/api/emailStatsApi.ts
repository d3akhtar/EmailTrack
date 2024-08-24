import { createApi,fetchBaseQuery } from  '@reduxjs/toolkit/query/react';
import { SD_General } from '../constants/constants';

export const emailStatsApi = createApi({
    reducerPath: "emailStatsApi",
    baseQuery: fetchBaseQuery({
        baseUrl: process.env.REACT_APP_EMAILSTATS_ENDPOINT,
        prepareHeaders:(headers: Headers, api) => {
            const token = localStorage.getItem(SD_General.tokenKey);
            token && headers.append("Authorization","Bearer " + token); // Pass token so [Authorize] and [Authenticate] can check if user has permission
        }
    }),
    tagTypes: ["stats"],
    endpoints: (builder) => ({
        detailed: builder.query({
            query: (body : any) => ({
                url: "/detailed",
                params: {
                    minDate: body.minDate.toDateString(),
                    maxDate: body.maxDate.toDateString()
                },
                providesTags: ["stats"],
            })
        }),
        summarized: builder.query({
            query: (body : any) => ({
                url: "/summarized",
                params: {
                    minDate: body.minDate.toDateString(),
                    maxDate: body.maxDate.toDateString()
                },
                providesTags: ["stats"]
            })
        }),
        changeUserSettings: builder.mutation({
            query: (body : any) => ({
                method: "PUT",
                url: "/change-settings",
                body: body
            }),
            invalidatesTags: ['stats']
        })
    })
})

export const {useDetailedQuery,useSummarizedQuery,useChangeUserSettingsMutation} = emailStatsApi;
export default emailStatsApi;