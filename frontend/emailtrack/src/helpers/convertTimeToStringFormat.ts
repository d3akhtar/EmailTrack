const convertTimeToStringFormat = (time : number) : string => 
    time > 0 ? `${time.toFixed(0)} min ${((time % 1) * 60).toFixed(0)} sec`:"-"


export default convertTimeToStringFormat