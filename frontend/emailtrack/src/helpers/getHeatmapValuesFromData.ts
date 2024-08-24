import { weeklyStatObject } from "../interfaces/stat";

const getHeatmapValuesFromData = (data: { [Name: string]: weeklyStatObject}) => {
    const heatmapValues = [];
    var times : string[] = Object.keys(data)
    var offsetHours = new Date().getTimezoneOffset() / 60;
    // console.log(offsetHours);
    for (var i = 0; i < 24; i++)
    {
        const heatmapRow = [];
        const key = times[i + offsetHours < 24 ? i+offsetHours:i+offsetHours-24]
        const value = data[key];
        
        heatmapRow.push(value.sunday);
        heatmapRow.push(value.monday);
        heatmapRow.push(value.tuesday);
        heatmapRow.push(value.wednesday);
        heatmapRow.push(value.thursday);
        heatmapRow.push(value.friday);
        heatmapRow.push(value.saturday);

        heatmapValues.push(heatmapRow);
    }

    return heatmapValues;
}

export default getHeatmapValuesFromData