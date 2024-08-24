import React, { useState } from 'react'
import { useInviteToTeamMutation } from '../../api/teamApi'
import apiResponse from '../../interfaces/apiResponse';

interface InviteUserFieldProps {
    onClose: Function
}

function InviteUserField({onClose} : InviteUserFieldProps) {
    const [inviteUser] = useInviteToTeamMutation();
    
    const [errorMessage,setErrorMessage] = useState<string>("");
    const [successMessage,setSuccessMessage] = useState<string>("")
    const [invitingUser,setInvitingUser] = useState<boolean>(false);
    const [emailInput,setEmailInput] = useState<string>("");

    const handleInviteUser = async () => {
        setErrorMessage("")
        setSuccessMessage("")
        setInvitingUser(true)

        const res : any = await inviteUser(emailInput);
        var apiResponse : apiResponse;
        if (res.error){
            apiResponse = res.error.data as apiResponse
            console.log(apiResponse)
            setErrorMessage(apiResponse.message!)
        }
        else{
            apiResponse = res.data as apiResponse
            setSuccessMessage(apiResponse.message!)
        }

        setInvitingUser(false)
    }
    
    return (
        <div className='' style={{position:"fixed", top:"300px", left:"500px", width:"2000px", height:"1000px"}}>
            <span onClick={() => onClose()} className='text-danger' style={{position:"relative", top:"3%", left:"0.5%", cursor:"pointer"}}><i className="bi bi-x-octagon-fill"></i></span>
            <div className='bg-light border shadow p-5' style={{width:"50%", height:"30%"}}>
                <label htmlFor='invite-email' className='lead fs-3'>Invite user</label>
                <input type='email' placeholder='Enter email here...' onChange={(e) => setEmailInput(e.currentTarget.value)} value={emailInput} id='invite-email' className='form-control mt-3'></input>
                <div className='d-flex align-items-center mt-3'>
                    <button onClick={handleInviteUser} disabled={!emailInput.includes("@") || invitingUser} className='btn btn-success'>Invite</button>
                    { errorMessage? <span className='text-danger ms-3 fw-bold'>{errorMessage}</span>:<></> }
                    { successMessage? <span className='text-success ms-3 fw-bold'>{successMessage}</span>:<></> }
                    { invitingUser? <span className='ms-3 fw-bold'>{successMessage}Inviting User...</span>:<></> }
                </div>
            </div> 
        </div>
  )
}

export default InviteUserField