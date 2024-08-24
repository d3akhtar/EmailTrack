import React from 'react'

interface Props {
    children: React.ReactNode
}

function LoadingScreen({children} : Props) {
    return (
        <>
            <div 
                style={{
                    position: "fixed",
                    top: "0",
                    left: "0",
                    width: "100vw",
                    height: "100vh",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center"
                    }}>
                    <div className={`row spinner-border text-info me-4`} style={{scale: `150%`}}></div>
                    <div className='row d-flex justify-content-center'>{children}</div>
            </div>
        </>
      )
}

export default LoadingScreen