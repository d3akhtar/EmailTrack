using EmailStatsService.Model;
using static EmailStatsService.DTO.GmailApiResponseDTO;

namespace EmailStatsService.DTO
{
    public class GmailStatDTO : IEquatable<GmailStatDTO>, IComparable<GmailStatDTO>
    {
        private static string FROM_HEADER_NAME = "From";
        private static string TO_HEADER_NAME = "To";
        private static string CC_HEADER_NAME = "Cc";
        public enum GmailType {
            Sent,
            Received,
            Unknown
        }

        public string Id { get; set; }
        public string ThreadId { get; set; }
        public long InternalDate { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Type { get; set; }

        public static GmailStatDTO GetGmailStatFromApiResponse(GmailApiResponseDTO gmailApiResponseDTO)
        {
            GmailType type;

            if (gmailApiResponseDTO.LabelIds.Contains("SENT")) type = GmailType.Sent;
            else if (gmailApiResponseDTO.LabelIds.Contains("INBOX")) type = GmailType.Received;
            else type = GmailType.Unknown;

            return new GmailStatDTO
            {
                Id = gmailApiResponseDTO.Id,
                ThreadId = gmailApiResponseDTO.ThreadId,
                InternalDate = long.Parse(gmailApiResponseDTO.InternalDate),
                From = gmailApiResponseDTO.Payload.Headers.FirstOrDefault(h => h.Name == FROM_HEADER_NAME).Value,
                To = gmailApiResponseDTO.Payload.Headers.FirstOrDefault(h => h.Name == TO_HEADER_NAME).Value,
                Cc = gmailApiResponseDTO.Payload.Headers.FirstOrDefault(h => h.Name == CC_HEADER_NAME)?.Value ?? "",
                Type = type.ToString()
            };
        }

        public Gmail ConvertToGmailModelForUser(User user) =>
            new Gmail()
            {
                Id = this.Id,
                ThreadId = this.ThreadId,
                InternalDate = this.InternalDate,
                From = this.From,
                To = this.To,
                Cc = this.Cc,
                Type = this.Type,
                UserId = user.Id
            };

        public int CompareTo(GmailStatDTO other)
        {
            var a = this.InternalDate;
            var b = other.InternalDate;

            if (a > b) return 1;
            else if (a == b) return 0;
            else return -1;
        }

        public bool Equals(GmailStatDTO other) => this.InternalDate == other.InternalDate;
    }
}