namespace EmailStatsService.DTO
{
    public class DetailedEmailStatisticsDTO
    {
        public string Email { get; set; }
        public int ReceivedMessages { get; set; }
        public int SentMessages { get; set; }
        public int Recipients => Interactions.Count(kv => kv.Value.SentCount > 0);
        public int Senders => Interactions.Count(kv => kv.Value.ReceiveCount > 0);
        public float ResponseRate { get; set; }
        public double AverageResponseTime { get; set; } // In minutes
        public double QuickestResponseTime { get; set; } // In minutes
        public double AverageFirstResponseTime { get; set; }
        public Dictionary<string, int> TimesBeforeFirstResponse { get; set; }
        public Dictionary<string,WeeklyStatObject<int>> ReceivedMessagesTiming { get; set; }
        public Dictionary<string,WeeklyStatObject<int>> SentMessagesTiming { get; set; }
        public Dictionary<string,InteractionInfoDTO> Interactions { get; set; }
        public Dictionary<string,int> MessagesSentByDay { get; set; }
        public Dictionary<string,int> MessagesReceivedByDay { get; set; }
        public EmailsReplied EmailsRepliedBreakdown { get; set; }
        public SentBreakdown SentBreakdownStats { get; set; }
        public ReceivedBreakdown ReceivedBreakdownStats { get; set; }
    }

    public class EmailsReplied
    {
        public ReplyBreakdown RepliesSent { get; set; } = new();
        public ReplyBreakdown RepliesReceived { get; set; } = new();
    }

    public class ReplyBreakdown
    {
        public string Type { get; set; } // either "Sent" or "Received"
        public int Replied { get; set; }
        public int NotReplied { get; set; }
    }

    public class SentBreakdown
    {
        public int StartedThreads { get; set; } = 0;
        public int ToExistingThreads { get; set; } = 0;
    }

    public class ReceivedBreakdown 
    {
        public int DirectMessages { get; set; } = 0;
        public int Cc { get; set; } = 0;
        public int Others { get; set; } = 0;
    }

    public class WeeklyStatObject<T>
    {
        public T Sunday { get; set; }
        public T Monday { get; set; }
        public T Tuesday { get; set; }
        public T Wednesday { get; set; }
        public T Thursday { get; set; }
        public T Friday { get; set; }
        public T Saturday { get; set; }
    }
}