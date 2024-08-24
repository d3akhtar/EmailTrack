import React, { useState } from 'react'
import SideBar from '../components/common/SideBar'
import { useNavigate } from 'react-router-dom'
import { useDeleteUserMutation } from '../api/authApi';
import { useGetTeamQuery } from '../api/teamApi';
import teamMember from '../interfaces/teamMember';
import apiResponse from '../interfaces/apiResponse';
import TeamMemberTableRow from '../components/settingsPage/TeamMemberTableRow';
import user from '../interfaces/user';
import { useDispatch, useSelector } from 'react-redux';
import InviteUserField from '../components/settingsPage/InviteUserField';
import { useChangeUserSettingsMutation } from '../api/emailStatsApi';
import { setUser } from '../redux/slices/userSlice';
import { jwtDecode } from 'jwt-decode';
import { SD_General } from '../constants/constants';
import withAuth from '../firstclass/withAuth';

function SettingsPage() {
    const nav = useNavigate();
    const dispatch = useDispatch()
    const [deleteLoggedInUser] = useDeleteUserMutation();
    const [deleteUserButtonClicked,setDeleteUserButtonClicked] = useState<boolean>(false);
    const [invitingUser,toggleInvitingUser] = useState<boolean>(false);
    const {data,isFetching,isSuccess,isError,error} = useGetTeamQuery(null);
    const loggedInUser : user = useSelector((state : any) => state.userStore);

    console.log(loggedInUser)

    const [weeklyCsvToggle,toggleWeeklyCsv] = useState<boolean>(loggedInUser.getWeeklyCsv);
    const [monthlyCsvToggle,toggleMonthlyCsv] = useState<boolean>(loggedInUser.getMonthlyCsv);

    const [searchQuery,setSearchQuery] = useState<string>("")

    const [changeUserSettings] = useChangeUserSettingsMutation()

    var teamMembers : teamMember[] = [];

    if (!isFetching && isSuccess){
        teamMembers = (data as apiResponse).team!.filter((m : teamMember) => m.email.includes(searchQuery))
        console.log(teamMembers)
    }

    const handleSave = async () => {
        const res : any = await changeUserSettings(
        {
            "getWeeklyCsv": weeklyCsvToggle,
            "getMonthlyCsv": monthlyCsvToggle
        })
        var apiResponse : apiResponse;
        if (res.error){
            apiResponse = res.error.data as apiResponse
            console.log(apiResponse)
        }
        else{
            apiResponse = res.data as apiResponse

            const userInfo:user = jwtDecode(apiResponse.token!)

            localStorage.setItem(SD_General.tokenKey,apiResponse.token!);
            
            dispatch(setUser({
                id: userInfo.id,
                email: userInfo.email,
                username: userInfo.username,
                getWeeklyCsv: userInfo.getWeeklyCsv,
                getMonthlyCsv: userInfo.getMonthlyCsv
            }));
        }
    }

    return (
        <div className='bg-dark' style={{height:"100%"}}>
            {
                invitingUser ? 
                (
                    <InviteUserField onClose={() => toggleInvitingUser(false)}/>
                )
                :
                (
                    <></>
                )
            }
            <div className='text-white'>
                <div className='row w-100 bg-dark' style={{height:"100%"}}>
                    <div className='col-md-2 d-flex'>
                        <SideBar/>
                    </div>
                    <div className='col-md-10 p-5 d-flex justify-content-center'>
                        <div className='p-5 w-75' style={{backgroundColor: "#32363d", borderRadius:"15px"}}>
                            <div className='row d-flex justify-content-center'>
                                <span className='lead fs-1'>Settings</span>
                                <div className='d-flex justify-content-between'>
                                    <div>
                                        <div className='lead fs-3 mt-3'>Profile</div>
                                        <div className='p mt-1'>Manage your profile settings here:</div>
                                    </div>
                                    <button onClick={() => handleSave()}
                                    disabled={weeklyCsvToggle == loggedInUser.getWeeklyCsv && monthlyCsvToggle == loggedInUser.getMonthlyCsv} className='btn btn-warning mx-3 my-5'>Save Changes</button>
                                </div>
                            </div>
                            <div className='row d-flex justify-content-center p-1'>
                                <label className='me-4'>Full Name</label>
                                <input disabled className='form-control my-1' value={loggedInUser.username}></input>
                                <p className='me-4'>Your name is synced from your google account</p>
                            </div>
                            <div className='row d-flex justify-content-center p-1'>
                                <label className='me-4'>Email Address</label>
                                <input disabled className='form-control my-1' value={loggedInUser.email}></input>
                                <p className='me-4'>Your email is synced from your google account</p>
                            </div>
                            <div className='row d-flex justify-content-center'>
                                <div className='d-flex justify-content-between'>
                                    <div>
                                        <div className='mt-3 lead text-info'>Weekly Report</div>
                                        <div className='p mt-1'>Get a CSV file of your weekly interactions in your email every week</div>
                                    </div>
                                    <button onClick={() => toggleWeeklyCsv(!weeklyCsvToggle)} className={`toggle-btn ${weeklyCsvToggle ? "toggled":""} mt-5`}>
                                        <div className='thumb'></div>
                                    </button>
                                </div>
                            </div>
                            <div className='row d-flex justify-content-center'>
                                <div className='d-flex justify-content-between'>
                                    <div>
                                        <div className='mt-3 lead text-info'>Monthly Report</div>
                                        <div className='p mt-1'>Get a CSV file of your monthly interactions in your email every month</div>
                                    </div>
                                    <button onClick={() => toggleMonthlyCsv(!monthlyCsvToggle)} className={`toggle-btn ${monthlyCsvToggle ? "toggled":""} mt-5`}>
                                        <div className='thumb'></div>
                                    </button>
                                </div>
                            </div>
                            <div className='row d-flex justify-content-center'>
                                <div className='d-flex justify-content-between'>
                                    <div>
                                        <div className='mt-3 lead text-danger'>Delete Account</div>
                                        <div className='p mt-1'>Remove your account and all data associated with it</div>
                                    </div>
                                    {
                                        !deleteUserButtonClicked ? 
                                        (
                                            <button onClick={() => setDeleteUserButtonClicked(true)} className='btn btn-danger mx-3 my-4'>Delete Account</button>
                                        )
                                        :
                                        (
                                            <div className='btn-group'>
                                                <button onClick={async () => {
                                                    await deleteLoggedInUser(null);
                                                    localStorage.removeItem(SD_General.tokenKey)
                                                    nav("/")
                                                }} className='btn btn-danger mx-1 my-4'>Confirm</button>
                                                <button onClick={() => setDeleteUserButtonClicked(false)} className='btn btn-warning mx-1 my-4'>Cancel</button>
                                            </div>
                                        )
                                    }
                                </div>
                            </div>
                            <div className='row my-5 border'></div>
                            <div className='row d-flex justify-content-center'>
                                <span className='lead fs-1'>Workspace</span>
                                <div className='d-flex justify-content-between'>
                                    <div>
                                        <div className='lead fs-3 mt-3'>My Team</div>
                                        <div className='p mt-1'>Start/Manage your team here:</div>
                                    </div>
                                </div>
                            </div>
                            <div className='row d-flex justify-content-center'>
                                <div className='d-flex justify-content-between mt-2'>
                                    <input disabled={teamMembers.length == 0} onChange={(e) => setSearchQuery(e.currentTarget.value)} value={searchQuery} className='form-control w-25' placeholder='Search for team members'></input>
                                    <button disabled={teamMembers.length == 0} onClick={() => toggleInvitingUser(true)} className='btn btn-success mx-1'>Invite Members</button>
                                </div>
                            </div>
                            <div className='row d-flex justify-content-center mt-5 p-5 shadow'>
                                <table className="table-dark">
                                    <thead>
                                        <tr>
                                            <th scope="col">Name</th>
                                            <th scope="col">Role</th>
                                            <th scope="col">Status</th>
                                            <th scope="col">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {
                                            teamMembers.map((member : teamMember) => {
                                                return <TeamMemberTableRow teamMember={member}/>
                                            })
                                        }
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default withAuth(SettingsPage)