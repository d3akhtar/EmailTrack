import React, { useState } from 'react'
import SideBar from '../components/common/SideBar'
import { useNavigate } from 'react-router-dom'
import { useSummarizedQuery } from '../api/emailStatsApi';
import { LoadingScreen, MiniLoader } from '../components/common/_index';
import { summarizedStats } from '../interfaces/stat';
import apiResponse from '../interfaces/apiResponse';
import convertTimeToStringFormat from '../helpers/convertTimeToStringFormat';
import { useCreateTeamMutation, useGetTeamStatsQuery } from '../api/teamApi';
import { DateRange, RangeKeyDict } from 'react-date-range';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import withAuth from '../firstclass/withAuth';

function TeamPage() {
    const nav = useNavigate();

    const currDate = new Date()
    const minDate = new Date(currDate.setMonth(currDate.getMonth() - 2))
    const maxDate = new Date()

    const [selectionRange,setSelectionRange] = useState<any>({
        startDate: minDate,
        endDate: maxDate,
        key: 'selection',
    });

    const handleSelect = (ranges : RangeKeyDict) => {
        setSelectionRange(ranges.selection)
    }

    const {data, isFetching, isSuccess, isError, error} = useGetTeamStatsQuery({
        minDate: selectionRange.startDate,
        maxDate: selectionRange.endDate
    });
    
    const [createTeam] = useCreateTeamMutation();

    var teamSummarizedStats : summarizedStats[]
    var averages : summarizedStats
    
    if (!isFetching && isSuccess){
        teamSummarizedStats = (data as apiResponse).stats as summarizedStats[]
        averages = {
            averageResponseTime : teamSummarizedStats.reduce((acc : number, stat : summarizedStats) => acc + stat.averageResponseTime,0 ) / teamSummarizedStats.length,
            email : "Team Average",
            receivedMessages : teamSummarizedStats.reduce((acc : number, stat : summarizedStats) => acc + stat.receivedMessages,0 ) / teamSummarizedStats.length,
            sentMessages : teamSummarizedStats.reduce((acc : number, stat : summarizedStats) => acc + stat.sentMessages,0 ) / teamSummarizedStats.length,
            responseRate : teamSummarizedStats.reduce((acc : number, stat : summarizedStats) => acc + stat.responseRate,0 ) / teamSummarizedStats.length
        }
        console.log(teamSummarizedStats)
    }
    if (isError){
        console.log(error)
    }

    return (
        <div className='bg-dark' style={{height:"100%"}}>
            <div className='text-white'>
                <div className='row w-100 bg-dark' style={{height:"100%"}}>
                    <div className='col-md-2 d-flex'>
                        <SideBar/>
                    </div>
                    <div className='col-md-10 py-5 px-3 d-flex justify-content-center'>
                        <div className='p-5 w-75' style={{backgroundColor: "#32363d", borderRadius:"15px"}}>
                            <div className='row d-flex justify-content-center mb-5'>
                                <span className='lead fs-4 text-center'>Select Analysis Date Range</span>
                                <span className='lead fs-6 mb-5 text-center'>You can only select the range within the last two months</span>
                                <div className='d-flex justify-content-center'>
                                    <DateRange
                                        ranges={[selectionRange]}
                                        onChange={(e) => handleSelect(e)}
                                        minDate={minDate}
                                        maxDate={maxDate}
                                        scroll={{
                                            enabled: true
                                        }}
                                    />
                                </div>   
                            </div>
                            {
                                isFetching ? 
                                (
                                    <div className='row mt-5'>
                                        <MiniLoader size={150} message={"Fetching team  stats..."}/>
                                    </div>
                                ):
                                (
                                    isError ?
                                    (
                                        <div className='row d-flex align-items-center justify-content-center no-scroll'>
                                            <div className='row d-flex justify-content-center'>    
                                                <span className='text-danger text-center lead fs-4 mb-3'>{`${(error as any).data.message}`}</span>
                                                <button onClick={async () => await createTeam(null)} className='btn btn-primary w-25 mx-2 my-1'>
                                                    Create A Team
                                                </button>
                                            </div>
                                        </div>
                                    )
                                    :
                                    (
                                        <>
                                            <div className='row d-flex justify-content-center'>
                                                <span className='lead text-center fs-1'>My Team</span>
                                                <button onClick={() => nav("/settings")} className='btn btn-primary mt-2 w-25'>Manage Team</button>
                                            </div>
                                            <div className='row d-flex justify-content-center mt-5 p-5 shadow'>
                                                <table className="table-dark">
                                                    <thead>
                                                        <tr>
                                                            <th className='lead' scope="col">Name</th>
                                                            <th className='lead' scope="col">Received Messages</th>
                                                            <th className='lead' scope="col">Sent Messages</th>
                                                            <th className='lead' scope="col">Response Rate</th>
                                                            <th className='lead' scope="col">Avg Response Time</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        <tr className='fw-bold text-info'>
                                                            <td>Team Average</td>
                                                            <td>{averages!.receivedMessages.toFixed(2)}</td>
                                                            <td>{averages!.sentMessages.toFixed(2)}</td>
                                                            <td>{`${averages!.responseRate.toFixed(2)}%`}</td>
                                                            <td>{convertTimeToStringFormat(averages!.averageResponseTime)}</td>
                                                        </tr>
                                                        <tr className='fw-bold text-info fs-6'>
                                                            <td style={{height:"15px"}}></td>
                                                        </tr>
                                                        {
                                                            teamSummarizedStats!.map((summarizedStats:summarizedStats,key:number) => 
                                                                <tr key={key}>
                                                                    <td>{summarizedStats.email}</td>
                                                                    <td>{summarizedStats!.receivedMessages}</td>
                                                                    <td>{summarizedStats!.sentMessages}</td>
                                                                    <td>{`${summarizedStats.responseRate < 0 ? "-":summarizedStats!.responseRate.toFixed(2) + "%"}`}</td>
                                                                    <td>{convertTimeToStringFormat(summarizedStats!.averageResponseTime)}</td>
                                                                </tr>
                                                            )
                                                        }
                                                    </tbody>
                                                </table>
                                            </div>
                                        </>
                                    )
                                )
                            }
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default withAuth(TeamPage)