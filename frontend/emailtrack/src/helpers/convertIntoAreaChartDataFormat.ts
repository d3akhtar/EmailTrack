const convertIntoAreaChartDataFormat = (data: { [Name: string]: number}) => {
    const dates = Object.keys(data);
    const dateObjects = dates.map((d:string) => new Date(d))
    dateObjects.sort((a : any,b : any) => a-b)
    const sortedKeys = dateObjects.map((d:Date) => d.toLocaleDateString())
    return sortedKeys.map((key:string) => { 
        return {
            "name": key,
            "Messages": data[key]
        }}
    )
}

export default convertIntoAreaChartDataFormat