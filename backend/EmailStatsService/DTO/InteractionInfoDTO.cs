using EmailStatsService.Helpers;
using EmailStatsService.Model;

namespace EmailStatsService.DTO
{
    public class InteractionInfoDTO
    {
        private bool messagesHasBeenSorted = false;
        private List<Gmail> messages = new();
        public void AddMessage(Gmail m) => messages.Add(m);
        public int TotalInteractions => SentCount + ReceiveCount;
        public int SentCount { get; set; }
        public int ReceiveCount { get; set; }
        public int BestContactTime // Represented by hour 
        {
            get {
                if (messages.Count == 0) return -1;
                if (!messagesHasBeenSorted) {
                    messages.Sort();
                    messagesHasBeenSorted = true;
                }

                Dictionary<int,int> freqeuncyByHour = new();
                foreach (var m in messages){
                    var hour = DateTimeOffset.FromUnixTimeMilliseconds(m.InternalDate).Hour;
                    if (freqeuncyByHour.ContainsKey(hour)) freqeuncyByHour[hour]++;
                    else freqeuncyByHour.Add(hour,1);
                }
                int mostFrequentHour = -1;
                int highestValue = -1;
                foreach (var kv in freqeuncyByHour){
                    if (kv.Value > highestValue){
                        highestValue = kv.Value;
                        mostFrequentHour = kv.Key;
                    }
                }
                return mostFrequentHour;
            }
        }
        public double ResponseTime 
        {
            get {
                if (messages.Count == 0) return -1;
                if (!messagesHasBeenSorted) {
                    messages.Sort();
                    messagesHasBeenSorted = true;
                }

                int totalResponses = 0;
                double totalResponseTime = 0;
                for (int i = 1; i < messages.Count; i++){
                    if (messages[i].Type == "Sent" &&  messages[i-1].Type == "Received")
                    {
                        totalResponses++;
                        totalResponseTime = ResponseTimeCalculator.GetMinutesBetweenTwoGmails(messages[i], messages[i-1]);                  
                    }
                }

                return totalResponses == 0 ? -1:totalResponseTime / totalResponses;
            }
        }
    }
}