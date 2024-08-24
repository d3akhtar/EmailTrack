const convertIntoBarGraphDataFormat = (data : { [Name: string]: number}) => {
    const keys = Object.keys(data).sort((a : any,b : any) => a-b)
    
    return keys.map((key:string) => { 
        return {
            "name": key,
            "Frequency": data[key]
        }}
    )
}

export default convertIntoBarGraphDataFormat