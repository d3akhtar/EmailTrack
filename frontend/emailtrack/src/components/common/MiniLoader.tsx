import React from 'react'

interface MiniLoaderProps {
    size: number
    message: string
}

function MiniLoader({size,message} : MiniLoaderProps) {
  return (
    <div className='d-flex align-items-center justify-content-center'>
        <div className={`spinner-border text-info`} style={{scale: `${size}%`}}></div>
        <span className='lead fs-3 ms-4 text-center'>{message}</span>
    </div>
  )
}

export default MiniLoader