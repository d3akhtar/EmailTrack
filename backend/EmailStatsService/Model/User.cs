using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmailStatsService.DTO;
using static EmailStatsService.DTO.TeamMemberDTO;

namespace EmailStatsService.Model
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        public DateTime? TokenExpiryDate { get; set; }
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        [NotMapped]
        public Team? Team { get; set; }
        public bool InvitePending { get; set; } = false;
        public bool GetWeeklyCsv { get; set; } = false;
        public bool GetMonthlyCsv { get; set; } = false;
        public ICollection<Gmail> Gmails { get; set; } = new List<Gmail>();

        public UserReadDTO GetUserReadDTO()
        {
            return new UserReadDTO
            {
                Username = Username,
                Email = Email
            };
        }

        public TeamMemberDTO GetTeamMemberDTO()
        {
            return new TeamMemberDTO
            {
                Email = Email,
                Role = GetTeamRole().ToString(),
                Status = GetTeamStatus().ToString()
            };
        }

        private TeamRole GetTeamRole()
        {
            if (TeamId != null) return TeamRole.Member;
            else return TeamRole.Admin;
        }

        private TeamStatus GetTeamStatus()
        {
            if (InvitePending) return TeamStatus.Invited;
            else return TeamStatus.Active;
        }

        public bool AccessTokenExpired() => DateTime.Today - TokenExpiryDate > TimeSpan.Zero;
    
    }
}