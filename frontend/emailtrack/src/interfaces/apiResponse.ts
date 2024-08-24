import { detailedStats, summarizedStats } from "./stat";
import teamMember from "./teamMember";
import user from "./user";

export default interface apiResponse {
    message?: string;
    token?: string
    user?: user;
    stats?: detailedStats | summarizedStats | summarizedStats[],
    team?: teamMember[]
}