export default interface teamMember {
    email : string,
    role : "Admin" | "Member",
    status : "Invited" | "Active",
}