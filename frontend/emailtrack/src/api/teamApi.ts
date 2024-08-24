import { createApi,fetchBaseQuery } from  '@reduxjs/toolkit/query/react';
import { SD_General } from '../constants/constants';

export const teamApi = createApi({
    reducerPath: "teamApi",
    baseQuery: fetchBaseQuery({
        baseUrl: process.env.REACT_APP_EMAILSTATS_ENDPOINT + "/team/",
        prepareHeaders:(headers: Headers, api) => {
            const token = localStorage.getItem(SD_General.tokenKey);
            token && headers.append("Authorization","Bearer " + token); // Pass token so [Authorize] and [Authenticate] can check if user has permission
        }
    }),
    tagTypes: ["stats"],
    endpoints: (builder) => ({
        getTeam: builder.query({
            query: () => ({
                url: ""
            }),
            providesTags: ["stats"]
        }),
        inviteToTeam: builder.mutation({
            query: (email : string) => ({
                method: "POST",
                url: "invite",
                params: {
                    email
                }
            }),
            invalidatesTags: ['stats']
        }),
        createTeam: builder.mutation({
            query: () => ({
                method: "POST",
                url: "create",
            }),
            invalidatesTags: ['stats']
        }),
        joinTeam: builder.mutation({
            query: (teamJoinCode : string) => ({
                method: "POST",
                url: "join",
                params: {
                    teamJoinCode
                }
            }),
        }),
        getTeamStats: builder.query({
            query: (body : any) => ({
                url: "stats",
                params: {
                    minDate: body.minDate.toDateString(),
                    maxDate: body.maxDate.toDateString()
                },
            }),
            providesTags: ['stats']
        }),
        removeTeamMember: builder.mutation({
            query: (email : string) => ({
                url: "",
                method: "DELETE",
                params: {
                    email
                }
            }),
            invalidatesTags: ['stats']
        }),
        declineTeamInvitation: builder.mutation({
            query: (teamJoinLink : string) => ({
                url: "decline",
                method: "POST",
                params: {
                    teamJoinLink
                }
            }),
            invalidatesTags: ['stats']
        })
    })
})

export const {useGetTeamQuery,useCreateTeamMutation,useInviteToTeamMutation,useJoinTeamMutation,useGetTeamStatsQuery,useDeclineTeamInvitationMutation,useRemoveTeamMemberMutation} = teamApi;
export default teamApi;