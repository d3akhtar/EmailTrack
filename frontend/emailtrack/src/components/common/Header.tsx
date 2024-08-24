import React from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { Link, useNavigate } from 'react-router-dom'
import user from '../../interfaces/user'
import { clearUser } from '../../redux/slices/userSlice'

function Header() {
    const loggedInUser : user = useSelector((state : any) => state.userStore)
    const nav = useNavigate();
    const dispatch = useDispatch()

    return (
        <nav className="navbar bg-light navbar-expand navbar-light w-100 px-3" style={{display: false ? "block":"flex",position:"fixed", backgroundColor:"#e3f2fd", zIndex:10}}>
            <Link className="navbar-brand" to="/ShortUrls"><span className='lead fs-5'>EmailTrack</span></Link> 
            <div className="collapse navbar-collapse justify-content-between" id="navbarSupportedContent">
                <ul className="navbar-nav mr-auto">
                    <li className="nav-item">
                        <Link className="nav-link" to="/main"><span className='text-center'>Report</span></Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/team"><span className='text-center'>Team</span></Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/settings"><span className='text-center'>Settings</span></Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/about"><span className='text-center'>About</span></Link>
                    </li>   
                </ul>
                {loggedInUser ? 
                    (
                        <ul className='navbar-nav ml-auto'>
                            <li className="nav-item">
                                <a onClick={() => {
                                    dispatch(clearUser())
                                    nav("/")
                                }} className="nav-link" style={{cursor:"pointer"}}><span className='text-center'>Logout</span></a>
                            </li>
                            <li className="nav-item">
                                <span className="nav-link"><span className='badge text-center text-dark'>{`${loggedInUser.username}`}</span></span>
                            </li>
                        </ul>
                    ):
                    (
                        <ul className='navbar-nav ml-auto'>
                            <li className="nav-item">
                                <Link className="nav-link" to="/ShortUrls/Register"><span className='text-center'>Register</span></Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/ShortUrls/Login"><span className='text-center'>Login</span></Link>
                            </li>
                        </ul>
                    )
                }
            </div>
        </nav>
      )
}

export default Header