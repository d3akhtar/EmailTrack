import React, { useState } from 'react'
import SideBar from '../components/common/SideBar'
import { LineChart, Line, CartesianGrid, XAxis, YAxis, AreaChart, Tooltip, Area, Legend, PieChart, Pie, ResponsiveContainer, Label, BarChart, Bar, Cell } from 'recharts';
import { HeatMapGrid } from 'react-grid-heatmap'
import emailStatsApi, { useDetailedQuery } from '../api/emailStatsApi';
import { detailedStats, interaction } from '../interfaces/stat';
import apiResponse from '../interfaces/apiResponse';
import LoadingScreen from '../components/common/LoadingScreen';
import InteractionTableRow from '../components/mainPage/InteractionTableRow';
import getHeatmapValuesFromData from '../helpers/getHeatmapValuesFromData';
import convertIntoAreaChartDataFormat from '../helpers/convertIntoAreaChartDataFormat';
import convertIntoBarGraphDataFormat from '../helpers/convertIntoBarGraphDataFormat';
import convertTimeToStringFormat from '../helpers/convertTimeToStringFormat';
import { DateRange, RangeKeyDict } from 'react-date-range';
import MiniLoader from '../components/common/MiniLoader';
import withAuth from '../firstclass/withAuth';

// For heatmaps
const xLabels = ['S', 'M', 'T', 'W', 'T', 'F', 'S']
const yLabels = [
    '00:00', '01:00', '02:00', '03:00', '04:00', '05:00',
    '06:00', '07:00', '08:00', '09:00', '10:00', '11:00',
    '12:00', '13:00', '14:00', '15:00', '16:00', '17:00',
    '18:00', '19:00', '20:00', '21:00', '22:00', '23:00'
]


function MainPage() {
    const currDate = new Date()
    const minDate = new Date(currDate.setMonth(currDate.getMonth() - 2))
    const maxDate = new Date()

    const [selectionRange,setSelectionRange] = useState<any>({
        startDate: minDate,
        endDate: maxDate,
        key: 'selection',
    });

    const {data, isFetching, isSuccess, isError, error} = useDetailedQuery({
        minDate: selectionRange.startDate,
        maxDate: selectionRange.endDate
    });

    const handleSelect = (ranges : RangeKeyDict) => {
        setSelectionRange(ranges.selection)
    }

    var detailedStats : detailedStats
    var interactedEmails : string[]
    
    if (!isFetching && isSuccess){
        detailedStats = (data as apiResponse).stats as detailedStats
        console.log("detailedStats")
        console.log(detailedStats)
        interactedEmails = Object.keys(detailedStats!.interactions)
    }

    return (
        <>
            { 
                <div className='bg-dark' style={{height:"100%"}}>
                    <div className='text-white'>
                        <div className='row w-100 bg-dark' style={{height:"100%"}}>
                            <div className='col-md-1 d-flex'>
                                <SideBar/>
                            </div>
                            <div className='col-md-11 mt-2 p-5'>
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
                                            <MiniLoader size={150} message={"Fetching stats..."}/>
                                        </div>
                                    ):
                                    (
                                        <>
                                            <div className='p-5' style={{backgroundColor: "#32363d", borderRadius:"15px"}}>
                                                <div className='row d-flex justify-content-center'>
                                                    <div className='col col-md-2 text-center' style={{borderRight: "1px solid white"}}>
                                                        <div className=''>Messages Sent</div>
                                                        <div className=''>{detailedStats!.sentMessages}</div>
                                                    </div>
                                                    <div className='col col-md-2 text-center' style={{borderRight: "1px solid white"}}>
                                                        <div className=''>Recipients</div>
                                                        <div className=''>{detailedStats!.recipients}</div>
                                                    </div>
                                                    <div className='col col-md-2 text-center' style={{borderRight: "1px solid white"}}>
                                                        <div className='lead'>Avg Response Time</div>
                                                        <div className=''>{convertTimeToStringFormat(detailedStats!.averageResponseTime)}</div>
                                                    </div>
                                                    <div className='col col-md-2 text-center' style={{borderRight: "1px solid white"}}>
                                                        <div className=''>Messages Received</div>
                                                        <div className=''>{detailedStats!.receivedMessages}</div>
                                                    </div>
                                                    <div className='col col-md-2 text-center'>
                                                        <div className=''>Senders</div>
                                                        <div className=''>{detailedStats!.senders}</div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div className='row mt-5'>
                                                <div className='col col-md-3 container d-flex text-center'>
                                                    <div className='m-1'>
                                                        <label className='mb-3'>Timing Of Sent Messages</label>
                                                        <HeatMapGrid
                                                        data={getHeatmapValuesFromData(detailedStats!.sentMessagesTiming)}
                                                        xLabels={xLabels}
                                                        yLabels={yLabels}
                                                        cellHeight='18px'
                                                        square={true}
                                                        />
                                                    </div>
                                                    <div className='m-1'>
                                                        <label className='mb-3'>Timing Of Received Messages</label>
                                                        <HeatMapGrid
                                                        data={getHeatmapValuesFromData(detailedStats!.receivedMessagesTiming)}
                                                        xLabels={xLabels}
                                                        yLabels={yLabels}
                                                        cellHeight='18px'
                                                        square={true}
                                                        cellStyle={(_x, _y, ratio) => ({
                                                            background: `rgb(65, 137, 254, ${ratio})`,
                                                            fontSize: ".8rem",
                                                            color: `rgb(0, 0, 0, ${ratio / 2 + 0.4})`
                                                        })}
                                                        />
                                                    </div>
                                                </div>
                                                <div className='col offset-md-1 col-md-7 mt-3'>
                                                    <div className='row text-center shadow'>
                                                        <label>Messages Sent By Day</label>
                                                        <AreaChart width={900} height={230} data={convertIntoAreaChartDataFormat(detailedStats!.messagesSentByDay)}
                                                            margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                                                            <defs>
                                                                <linearGradient id="colorPv" x1="0" y1="0" x2="0" y2="1">
                                                                <stop offset="5%" stopColor="#82ca9d" stopOpacity={0.8}/>
                                                                <stop offset="95%" stopColor="#82ca9d" stopOpacity={0}/>
                                                                </linearGradient>
                                                            </defs>
                                                            <XAxis dataKey="name"/>
                                                            <YAxis />
                                                            <Tooltip />
                                                            <Legend/>
                                                            <Area type="monotone" dataKey="Messages" stroke="#82ca9d" fillOpacity={1} fill="url(#colorPv)" />
                                                        </AreaChart>
                                                    </div>
                                                    <div className='row text-center mt-3 shadow'>
                                                        <label>Messages Received By Day</label>
                                                        <AreaChart width={900} height={230} data={convertIntoAreaChartDataFormat(detailedStats!.messagesReceivedByDay)}
                                                            margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                                                            <defs>
                                                                <linearGradient id="colorKv" x1="0" y1="0" x2="0" y2="1">
                                                                <stop offset="5%" stopColor="#8284ca" stopOpacity={0.8}/>
                                                                <stop offset="95%" stopColor="#8284ca" stopOpacity={0}/>
                                                                </linearGradient>
                                                            </defs>
                                                            <XAxis dataKey="name" />
                                                            <YAxis/>
                                                            <Tooltip />
                                                            <Legend/>
                                                            <Area label={""} type="monotone" dataKey="Messages" stroke="#8284ca" fillOpacity={1} fill="url(#colorKv)" />
                                                        </AreaChart>
                                                    </div>
                                                </div>
                                            </div>
                                            <div className='row d-flex mt-5'>
                                                <div className='col col-md-6 text-center shadow'>
                                                    <label className='mb-3'>Times Before First Response</label>
                                                    <BarChart width={730} height={250} data={convertIntoBarGraphDataFormat(detailedStats!.timesBeforeFirstResponse)}
                                                        margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
                                                        <XAxis dataKey="name" />
                                                        <YAxis />
                                                        <Tooltip />
                                                        <Bar dataKey='Frequency' fill="#f54b42"  />
                                                    </BarChart>
                                                </div>
                                                <div className='col col-md-6 text-center shadow pt-5'>
                                                    <div className='row'>
                                                        <span className='lead fs-3'>Quickest Response Time:</span>
                                                        <span className='fs-4'>{convertTimeToStringFormat(detailedStats!.quickestResponseTime)}</span>
                                                    </div>
                                                    <div className='row mt-5'>
                                                        <span className='lead fs-3'>Average First Response Time</span>
                                                        <span className='fs-4'>{convertTimeToStringFormat(detailedStats!.averageFirstResponseTime)}</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div className='row d-flex mt-5 justify-content-center'>
                                                <div className='col col-md-5 shadow'>
                                                <h3>Emails Replied</h3>
                                                    <div className='d-flex justify-content-center '>
                                                        <PieChart width={300} height={250}>
                                                            <Legend/>
                                                            <Pie data=
                                                            {
                                                                [
                                                                    {
                                                                        "name" : "Replied",
                                                                        "value" :  detailedStats!.emailsRepliedBreakdown.repliesSent.replied
                                                                    },
                                                                    {
                                                                        "name" : "Not Replied",
                                                                        "value" :  detailedStats!.emailsRepliedBreakdown.repliesSent.notReplied
                                                                    }
                                                                ]
                                                            } legendType='rect' dataKey="value" nameKey="name" cx="50%" cy="50%" innerRadius={60} outerRadius={80} fill="#82ca9d" label>
                                                            <Cell key='cell-0' fill='#37af65'/>
                                                            <Cell key='cell-1' fill='#82ca9b'/>
                                                            </Pie>
                                                        </PieChart>
                                                        <PieChart width={300} height={250}>
                                                            <Legend/>
                                                            <Pie data=
                                                            {
                                                                [
                                                                    {
                                                                        "name" : "Started Threads",
                                                                        "value" :  detailedStats!.sentBreakdownStats.startedThreads
                                                                    },
                                                                    {
                                                                        "name" : "Existing Threads",
                                                                        "value" :  detailedStats!.sentBreakdownStats.toExistingThreads
                                                                    }
                                                                ]
                                                            } dataKey="value" nameKey="name" cx="50%" cy="50%" innerRadius={60} outerRadius={80} fill="#3b72cd" label>
                                                            <Cell key='cell-0' fill='#3b72cd'/>
                                                            <Cell key='cell-1' fill='#698cd7'/>    
                                                            </Pie>
                                                        </PieChart>
                                                    </div>
                                                </div>
                                                <div className='offset-md-1 col col-md-3 shadow'>
                                                <h3>Sent Breakdown</h3>
                                                    <div className='d-flex justify-content-center '>
                                                        <PieChart width={300} height={250}>
                                                            <Legend/>
                                                            <Pie data=
                                                            {
                                                                [
                                                                    {
                                                                        "name" : "Replied",
                                                                        "value" :  detailedStats!.emailsRepliedBreakdown.repliesSent.replied
                                                                    },
                                                                    {
                                                                        "name" : "Not Replied",
                                                                        "value" :  detailedStats!.emailsRepliedBreakdown.repliesSent.notReplied
                                                                    },
                                                                ]
                                                            } dataKey="value" nameKey="name" cx="50%" cy="50%" innerRadius={60} outerRadius={80} fill="#82ca9d" label>
                                                            <Cell key='cell-0' fill='#37af65'/>
                                                            <Cell key='cell-1' fill='#82ca9b'/>
                                                            </Pie>
                                                        </PieChart>
                                                    </div>
                                                </div>
                                                <div className='col col-md-3 shadow'>
                                                <h3>Received Breakdown</h3>
                                                    <div className='d-flex justify-content-center '>
                                                        <PieChart width={300} height={250}>
                                                            <Legend/>
                                                            <Pie data=
                                                            {
                                                                [
                                                                    {
                                                                        "name" : "Direct Messages",
                                                                        "value" :  detailedStats!.receivedBreakdownStats.directMessages,
                                                                    },
                                                                    {
                                                                        "name" : "Cc",
                                                                        "value" :  detailedStats!.receivedBreakdownStats.cc
                                                                    },
                                                                    {
                                                                        "name" : "Others",
                                                                        "value" :  detailedStats!.receivedBreakdownStats.others
                                                                    }
                                                                ]
                                                            } 
                                                            dataKey="value" nameKey="name" cx="50%" cy="50%" innerRadius={60} outerRadius={80} fill="#3b72cd" label>
                                                            <Cell key='cell-0' fill='#3b72cd'/>
                                                            <Cell key='cell-1' fill='#698cd7'/>
                                                            <Cell key='cell-2' fill='#9bb6e5'/>
                                                            </Pie>
                                                        </PieChart>
                                                    </div>
                                                </div>
                                            </div>
                                            <div className='row d-flex mt-5 justify-content-center p-5 shadow'>
                                                <table className="table-dark">
                                                    <thead>
                                                        <tr>
                                                            <th scope="col">Email</th>
                                                            <th scope="col">Interactions</th>
                                                            <th scope="col">Sent Messages</th>
                                                            <th scope="col">Received Messages</th>
                                                            <th scope="col">Best Contact Time</th>
                                                            <th scope="col">My Response Time</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {
                                                            interactedEmails!.map(email => {
                                                                return <InteractionTableRow email={email} interaction={detailedStats!.interactions[email]}/>
                                                            })        
                                                        }
                                                    </tbody>
                                                </table>
                                            </div>
                                        </>
                                    )
                                }
                            </div>
                        </div>
                    </div>
                </div>
            }
        </>
    )
}

export default withAuth(MainPage)