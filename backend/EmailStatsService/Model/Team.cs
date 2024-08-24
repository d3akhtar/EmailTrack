using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailStatsService.Model
{
    public class Team
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int OwnerUserId { get; set;}
        public ICollection<User> Users { get; set; } = new List<User>();
        public string TeamJoinCode { get; set; }
    }
}