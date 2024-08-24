namespace EmailStatsService.DTO
{
    public class GmailApiResponseDTO
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
        public List<GmailMessageListObject>? Messages { get; set;}
        public List<string>? LabelIds { get; set; }
        public GmailMessagePayload? Payload { get; set; }
        public string? InternalDate { get; set ; }
        public string? NextPageToken { get; set; }
    }

    public class GmailMessageListObject
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
    }

    public class GmailMessagePayload
    {
        public List<GmailPayloadHeader> Headers { get; set; }
    }

    public class GmailPayloadHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}