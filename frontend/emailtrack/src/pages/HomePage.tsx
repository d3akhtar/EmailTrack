import React from 'react'
import { useNavigate } from 'react-router-dom'
import checkForToken from '../helpers/checkForToken';

function HomePage() {
    const nav = useNavigate();
    return (
        <div className='vh-100 bg-dark d-flex justify-content-center align-items-center'>
            <div className='text-light text-center'>
                <div className='lead fs-1'>All-In-One Email Analytics For Gmail</div>
                <div className='p fs-3'>Monitor statistics such as received and sent email counts on one platform</div>
                {
                    checkForToken() != null ?
                    (
                        <button onClick={() => nav("/main")} className='btn btn-light w-25 mt-5 mx-1'>Go To Stats</button>
                    )
                    :
                    (
                        <button onClick={() => nav("/login")} className='btn btn-light w-25 mt-5 mx-1'>Log In</button>
                    )
                }
            </div>
        </div>
  )
}

export default HomePage