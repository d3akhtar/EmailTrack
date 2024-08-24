import React from 'react'
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom'
import { clearUser } from '../../redux/slices/userSlice';
import { SD_General } from '../../constants/constants';

function SideBar() {
    const nav = useNavigate();
    const dispatch = useDispatch();
  return (
    <div className="flex-shrink-0 p-3 bg-white vh-100" style={{width: "8%",height:"100vh", position:"fixed"}}>
        <a className="d-flex align-items-center pb-3 mb-3 link-dark text-decoration-none border-bottom">
            <span style={{fontSize:"1.8vh", cursor: "pointer"}} onClick={() => nav("/")} className="sidebar-label ms-3 lead">EmailTrack</span>
        </a>
        <ul className="list-unstyled ps-0">
        <li className="mb-1">
            <button style={{fontSize:"1.5vh"}} onClick={() => nav("/main")} className="btn btn-toggle align-items-center rounded collapsed" data-bs-toggle="collapse" data-bs-target="#home-collapse" aria-expanded="true">
                <span className='sidebar-label'>Report</span>
            </button>
        </li>
        <li className="mb-1">
            <button style={{fontSize:"1.5vh"}} onClick={() => nav("/team")} className="btn btn-toggle align-items-center rounded collapsed" data-bs-toggle="collapse" data-bs-target="#dashboard-collapse" aria-expanded="false">
                <span className='sidebar-label'>Team</span>
            </button>
        </li>
        <li className="mb-1">
            <button style={{fontSize:"1.5vh"}} onClick={() => nav("/settings")} className="btn btn-toggle align-items-center rounded collapsed" data-bs-toggle="collapse" data-bs-target="#orders-collapse" aria-expanded="false">
                <span className='sidebar-label'>Settings</span>
            </button>
        </li>
        <li className="border-top my-3"></li>
        <li className="mb-1">
            <button style={{fontSize:"1.5vh"}} 
            onClick={() => {
                dispatch(clearUser())
                localStorage.removeItem(SD_General.tokenKey)
                nav("/login")
            }} 
            className="btn btn-toggle align-items-center rounded collapsed" data-bs-toggle="collapse" data-bs-target="#account-collapse" aria-expanded="false">
                <span className='sidebar-label'>Log Out</span>
            </button>
        </li>
        </ul>
  </div>
  )
}

export default SideBar