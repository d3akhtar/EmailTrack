import React, { useState } from 'react'
import teamMember from '../../interfaces/teamMember'
import { useRemoveTeamMemberMutation } from '../../api/teamApi';

interface TeamMemberTableRowProps {
    teamMember : teamMember
}

function TeamMemberTableRow({teamMember} : TeamMemberTableRowProps) {
  const [deleteButtonClicked,toggleDeleteButtonClicked] = useState<boolean>(false);
  const [removeTeamMember] = useRemoveTeamMemberMutation() 
  return (
    <tr>
        <td>{teamMember.email}</td>
        <td>{teamMember.role}</td>
        <td>{teamMember.status}</td>
        <td>
          {
            teamMember.role == "Member" ?
            (
              <>
                {
                  deleteButtonClicked ?
                  (
                    <>
                      <button onClick={() => toggleDeleteButtonClicked(false)} className='btn btn-sm btn-warning text-dark'><i className="bi bi-x-circle"></i></button>
                      <button onClick={async () => await removeTeamMember(teamMember.email)} className='btn btn-sm btn-danger text-white mx-1'><i className="bi bi-trash3-fill"></i></button>
                    </>
                  ):
                  (
                    <button onClick={() => toggleDeleteButtonClicked(true)} className='btn btn-sm btn-danger text-white'><i className="bi bi-trash3-fill"></i></button>
                  )
                }
              </>
            )
            :
            (
              <></>
            )
          }
        </td>
    </tr>
  )
}

export default TeamMemberTableRow