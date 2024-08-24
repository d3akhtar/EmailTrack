namespace EmailStatsService.DTO
{
    public class TeamMemberDTO
    {
        public enum TeamRole {
            Admin,
            Member,
        }

        public enum TeamStatus {
            Invited,
            Active,
        }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}