import convertTimeToStringFormat from "../../helpers/convertTimeToStringFormat";
import { interaction } from "../../interfaces/stat";
import React from 'react'


interface InteractionsTableRowProps {
    interaction: interaction
    email: string
}

function InteractionTableRow({interaction, email}: InteractionsTableRowProps) {
  return (
    <tr>
        <td>{email}</td>
        <td>{interaction.totalInteractions}</td>
        <td>{interaction.sentCount}</td>
        <td>{interaction.receiveCount}</td>
        <td>{interaction.bestContactTime}</td>
        <td>{convertTimeToStringFormat(interaction.responseTime)}</td>
    </tr>
  )
}

export default InteractionTableRow