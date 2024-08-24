import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import apiResponse from '../interfaces/apiResponse';
import { SD_General } from '../constants/constants';
import { useDispatch } from 'react-redux';
import { clearUser, setUser } from '../redux/slices/userSlice';
import user from '../interfaces/user';
import { jwtDecode } from 'jwt-decode';
import {handleGoogleAuth,getExternalProfileInfo, checkForToken} from '../helpers/_index'
import { useDeclineTeamInvitationMutation, useJoinTeamMutation } from '../api/teamApi';
import LoadingScreen from '../components/common/LoadingScreen';


function LoginPage() {
  const nav = useNavigate();

  const dispatch = useDispatch();
  const [teamJoinLink,setTeamJoinLink] = useState<string>("");
  const [hasTeamInvite,setHasTeamInvite] = useState<boolean>(false);
  const [declineButtonClicked,setDeclinedButtonClicked] = useState<boolean>(false);
  const [joinButtonClicked,setJoinButtonClicked] = useState<boolean>(false);
  const [joiningTeam,setJoiningTeam] = useState<boolean>(false);
  const [errorMessage,setErrorMessage] = useState<string>("");
  const [loading,setLoading] = useState<boolean>(false);
  
  const [joinTeam] = useJoinTeamMutation()
  const [declineTeamInvitation] = useDeclineTeamInvitationMutation()

  const handleJoinButtonClick = async () => {
    setJoinButtonClicked(true)
    setJoiningTeam(true)
    setErrorMessage("")

    const res : any = await joinTeam(teamJoinLink)

    if (res.error){
      const apiResponse : apiResponse = res.error.data as apiResponse
      setErrorMessage(apiResponse.message!)
    }
    setJoiningTeam(false)
  }
  const handleLogout = () => {
    dispatch(clearUser())
    localStorage.removeItem(SD_General.tokenKey)
  }

  useEffect(() => {
    const accessTokenRegex = /access_token=([^&]+)/;
    const refreshTokenRegex = /refresh_token=([^&]+)/;
    const teamJoinLinkRegex = /teamJoinLink=([^&]+)/;
    const isMatchAccessTokenRegex = window.location.href.match(accessTokenRegex);
    const isMatchRefreshTokenRegex = window.location.href.match(refreshTokenRegex);
    const isMatchTeamJoinLinkRegex = window.location.href.match(teamJoinLinkRegex);

    const runGetExternalProfileInfoMethod = async (accessToken:string, refreshToken:string) => {
      setLoading(true)
      const userInfo:user = await getExternalProfileInfo(accessToken,refreshToken);
      setLoading(false)

      dispatch(setUser({
        id: userInfo.id,
        email: userInfo.email,
        username: userInfo.username,
        getWeeklyCsv: userInfo.getWeeklyCsv,
        getMonthlyCsv: userInfo.getMonthlyCsv
      }));

      if (isMatchTeamJoinLinkRegex){
        setHasTeamInvite(true)
      }
      else{
        nav("/main");
      }
    }

    if (isMatchTeamJoinLinkRegex){
      handleLogout()
      const teamJoinLink = isMatchTeamJoinLinkRegex[1];
      setTeamJoinLink(teamJoinLink);
    }
    else{
      if (checkForToken()) window.location.replace("/main")
    }

    if (isMatchAccessTokenRegex && isMatchRefreshTokenRegex) {
      const accessToken = isMatchAccessTokenRegex[1];
      const refreshToken = isMatchRefreshTokenRegex[1];
      runGetExternalProfileInfoMethod(accessToken,refreshToken)
    }
  }, []);

  return (
    <div className='vh-100 bg-dark d-flex justify-content-center align-items-center'>
        {
          loading ?
          (
            <LoadingScreen>
              <span className='lead fs-3 text-white'>Fetching gmails. This may take some time...</span>
            </LoadingScreen>
          ):
          (
            <>
              {
                !hasTeamInvite ? 
                (
                  <div className='text-light text-center'>
                    <div className='lead fs-2'>Continue With...</div>
                    <button onClick={() => handleGoogleAuth(teamJoinLink)} className='btn btn-light w-100 mt-3 mx-1'><i className="bi bi-google me-2"></i>Gmail</button>
                    <button onClick={() => nav("/")} className='btn btn-secondary w-100 mt-5 mx-1'><i className="bi bi-house me-2"></i>Back To Home</button>
                  </div>
                )
                :
                (
                  <>
                    {
                      !joinButtonClicked && !declineButtonClicked ?
                      (
                        <div className='text-light text-center'>
                          <div className='lead fs-2'>You have been invited to join a team. Select your action.</div>
                          <button onClick={() => handleJoinButtonClick()} className='btn btn-success w-50 mt-5 mx-1'>Join</button>
                          <button onClick={async () => {
                            setDeclinedButtonClicked(true)
                            await declineTeamInvitation(teamJoinLink)
                          }} className='btn btn-danger w-50 mt-2 mx-1'>Decline</button>
                        </div>
                      )
                      :
                      (
                        <>
                          {
                            joinButtonClicked ?
                            (
                              <>
                                {
                                  joiningTeam ? 
                                  (
                                    <div className='text-light text-center'>
                                      <div className='lead fs-2'>Joining team...</div>
                                    </div>
                                  )
                                  :
                                  (
                                    <>
                                      {
                                        errorMessage == "" ? 
                                        (
                                          <div className='text-light text-center'>
                                            <div className='lead fs-2'>You accepted the invitation successfully. Select your action.</div>
                                            <button onClick={() => { 
                                              handleLogout()
                                              nav("/")
                                            }} className='btn btn-secondary w-50 mt-2 mx-1'>Logout</button>
                                            <button onClick={() => nav("/main")} className='btn btn-success w-50 mt-2 mx-1'>Go To Main Page</button>
                                          </div>
                                        )
                                        :
                                        (
                                          <div className='text-light text-center'>
                                            <div className='lead fs-2'>An error occurred when you accepted the invitation</div>
                                            <div className='lead mt-2 fs-4 text-danger'>{errorMessage}</div>
                                            <button onClick={() => handleJoinButtonClick()} className='btn btn-warning w-50 mt-5 mx-1'>Try again</button>
                                            <button onClick={() => { 
                                              handleLogout()
                                              nav("/")
                                            }} className='btn btn-secondary w-50 mt-2 mx-1'>Logout</button>
                                            <button onClick={() => nav("/main")} className='btn btn-success w-50 mt-2 mx-1'>Go To Main Page</button>
                                          </div>
                                        )
                                      }
                                    </>
                                  )
                                }
                              </>
                            )
                            :
                            (
                              <div className='text-light text-center'>
                                <div className='lead fs-2'>You declined the invitation. Select your action.</div>
                                <button onClick={() => { 
                                              handleLogout()
                                              nav("/")
                                            }} className='btn btn-secondary w-50 mt-2 mx-1'>Logout</button>
                                <button onClick={() => nav("/main")} className='btn btn-success w-50 mt-2 mx-1'>Go To Main Page</button>
                              </div>
                            )
                          }
                        </>
                      )
                    }
                  </>
                )
              }
            </>
          )
        }
    </div>
  )
}

export default LoginPage