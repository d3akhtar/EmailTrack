using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailStatsService.Model
{
    public class Gmail : IEquatable<Gmail>, IComparable<Gmail>
    {
        [Key]
        [Required]
        public string Id { get; set; }
        [Required]
        public string ThreadId { get; set; }
        [Required]
        public int UserId { get; set; }
        [NotMapped]
        [ForeignKey("UserId")]
        public User User { get; set; }
        [Required]
        public long InternalDate { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        public string? Cc { get; set; }
        [Required]
        public string Type { get; set; }

        public int CompareTo(Gmail other)
        {
            var a = this.InternalDate;
            var b = other.InternalDate;

            if (a > b) return 1;
            else if (a == b) return 0;
            else return -1;
        }

        public bool Equals(Gmail other) => this.InternalDate == other.InternalDate;

        public bool WithinDateRange(DateTime minDate, DateTime maxDate)
        {
            var mailDateTime = DateTimeOffset.FromUnixTimeMilliseconds(this.InternalDate).ToLocalTime().Date;

            return minDate.DayOfYear == maxDate.DayOfYear ?
                mailDateTime.DayOfYear == minDate.DayOfYear:
                minDate - mailDateTime < TimeSpan.Zero && maxDate - mailDateTime > TimeSpan.Zero;
        }

        public bool WithinDateRange(DateTime date)
        {
            var mailDateTime = DateTimeOffset.FromUnixTimeMilliseconds(this.InternalDate).ToLocalTime().Date;

            return date.DayOfYear == mailDateTime.DayOfYear;
        }
    }
}