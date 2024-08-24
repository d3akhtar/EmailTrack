using System.Net;
using System.Text.Json;
using EmailStatsService.Data;
using EmailStatsService.DTO;
using EmailStatsService.Helpers;
using EmailStatsService.Model;
using ZstdSharp.Unsafe;
using static EmailStatsService.DTO.GmailApiResponseDTO;

namespace EmailStatsService.GmailApi
{
    public class GmailApiService : IGmailApiService
    {
        private const string GMAIL_USER_MESSAGES_LIST_SENT_ENDPOINT = "https://gmail.googleapis.com/gmail/v1/users/{0}/messages?labelIds=SENT&maxResults=100&pageToken={1}";
        private const string GMAIL_USER_MESSAGES_LIST_RECEIVED_ENDPOINT = "https://gmail.googleapis.com/gmail/v1/users/{0}/messages?labelsIds=INBOX&maxResults=100&pageToken={1}";
        private const string GMAIL_GET_EMAIL_ENDPOINT = "https://gmail.googleapis.com/gmail/v1/users/{0}/messages/{1}?format=metadata&metadataHeaders=From&metadataHeaders=To&metadataHeaders=Cc";
        private readonly string[] _firstResponseTimeRanges = new string[]
        {
            "<15m",
            "1h", // 15m to 1h
            "4h", // 1h to 4h, etc...
            "12h",
            "1d",
            "2d",
            ">2d"
        };
        private readonly IGmailRepo _gmailRepo;
        private string currentUserEmail;

        private HttpClient _httpClient;

        public GmailApiService(IGmailRepo gmailRepo)
        {
            _gmailRepo = gmailRepo;
        }

        public DetailedEmailStatisticsDTO GetDetailedUserEmailStatistics(User user, DateTime minDate, DateTime maxDate)
        {
            currentUserEmail = user.Email;

            DetailedEmailStatisticsDTO detailedEmailStatisticsDTO = new();
            detailedEmailStatisticsDTO.Email = user.Email;

            Dictionary<string,WeeklyStatObject<int>> receivedMessagesTiming = GetTimingDictionary();
            Dictionary<string,WeeklyStatObject<int>> sentMessagesTiming = GetTimingDictionary();
            Dictionary<string,InteractionInfoDTO> interactions = new();
            Dictionary<string, List<Gmail>> emailThreadsDictionary = new();
            Dictionary<string,int> messagesSentByDay = new();
            Dictionary<string,int> messagesReceivedByDay = new();
            int sentCount = 0, receiveCount = 0;

            List<Gmail> gmails = user.Gmails.Where(g => g.WithinDateRange(minDate,maxDate)).ToList();

            UpdateDetailedStatsUsingGmailStatObjects(
                ref detailedEmailStatisticsDTO,
                ref gmails,
                ref sentCount, 
                ref receiveCount, 
                ref messagesSentByDay, 
                ref messagesReceivedByDay, 
                ref interactions,
                ref sentMessagesTiming,
                ref receivedMessagesTiming,
                ref emailThreadsDictionary
            );

            UpdateDetailedStatsUsingGmailThreads(ref emailThreadsDictionary, ref detailedEmailStatisticsDTO);

            return detailedEmailStatisticsDTO;
        }

        private void UpdateDetailedStatsUsingGmailThreads(
            ref Dictionary<string,List<Gmail>> emailThreadsDictionary,
            ref DetailedEmailStatisticsDTO detailedEmailStatisticsDTO
        )
        {
            ExamineEmailThreads(
                ref emailThreadsDictionary, 
                out float responseRate, 
                out double averageResponseTime, 
                out double quickestResponseTime,
                out double averageFirstResponseTime,
                out Dictionary<string, int> timesBeforeFirstResponse,
                out SentBreakdown sentBreakdown,
                out ReceivedBreakdown receivedBreakdown,
                out EmailsReplied emailsReplied
            );

            detailedEmailStatisticsDTO.QuickestResponseTime = quickestResponseTime;
            detailedEmailStatisticsDTO.AverageResponseTime = averageResponseTime;
            detailedEmailStatisticsDTO.AverageFirstResponseTime = averageFirstResponseTime;
            detailedEmailStatisticsDTO.TimesBeforeFirstResponse = timesBeforeFirstResponse;
            detailedEmailStatisticsDTO.SentBreakdownStats = sentBreakdown;
            detailedEmailStatisticsDTO.ReceivedBreakdownStats = receivedBreakdown;
            detailedEmailStatisticsDTO.EmailsRepliedBreakdown = emailsReplied;
        }

        private void UpdateDetailedStatsUsingGmailStatObjects(
            ref DetailedEmailStatisticsDTO detailedEmailStatisticsDTO,
            ref List<Gmail> gmails,
            ref int sentCount,
            ref int receiveCount,
            ref Dictionary<string,int> messagesSentByDay, 
            ref Dictionary<string,int> messagesReceivedByDay, 
            ref Dictionary<string,InteractionInfoDTO> interactions,
            ref Dictionary<string,WeeklyStatObject<int>> sentMessagesTiming,
            ref Dictionary<string,WeeklyStatObject<int>> receivedMessagesTiming,
            ref Dictionary<string,List<Gmail>> emailThreadsDictionary
        )
        {
            foreach (var g in gmails)
            {
                UpdateSingleEmailBasedVariablesAndThreadsDictionary(
                    ref sentCount, 
                    ref receiveCount, 
                    ref messagesSentByDay, 
                    ref messagesReceivedByDay, 
                    ref interactions,
                    ref sentMessagesTiming,
                    ref receivedMessagesTiming,
                    ref emailThreadsDictionary,
                    g);
            }

            detailedEmailStatisticsDTO.ReceivedMessages = receiveCount;
            detailedEmailStatisticsDTO.SentMessages = sentCount;
            detailedEmailStatisticsDTO.ReceivedMessagesTiming = receivedMessagesTiming;
            detailedEmailStatisticsDTO.SentMessagesTiming = sentMessagesTiming;
            detailedEmailStatisticsDTO.Interactions = interactions;
            detailedEmailStatisticsDTO.MessagesSentByDay = messagesSentByDay;
            detailedEmailStatisticsDTO.MessagesReceivedByDay = messagesReceivedByDay;
        }

        // Update variables from the first for loop
        private void UpdateSingleEmailBasedVariablesAndThreadsDictionary(
            ref int sentCount,
            ref int receiveCount,
            ref Dictionary<string,int> messagesSentByDay, 
            ref Dictionary<string,int> messagesReceivedByDay, 
            ref Dictionary<string,InteractionInfoDTO> interactions,
            ref Dictionary<string,WeeklyStatObject<int>> sentMessagesTiming,
            ref Dictionary<string,WeeklyStatObject<int>> receivedMessagesTiming,
            ref Dictionary<string,List<Gmail>> emailThreadsDictionary,
            Gmail g)
        {
            string shortDate = DateTimeOffset.FromUnixTimeMilliseconds(g.InternalDate).Date.ToShortDateString();
            if (g.Type == "Sent") {
                sentCount++;
                UpdateMessagesSentByDayDictionary(ref messagesSentByDay,shortDate);
                UpdateInteractionsDictionarySentCount(ref interactions, g);
                UpdateTimingDictionaryBasedOffGmail(ref sentMessagesTiming, g);
            }
            else if (g.Type == "Received"){
                receiveCount++;
                UpdateMessagesSentByDayDictionary(ref messagesReceivedByDay,shortDate);
                UpdateInteractionsDictionaryReceiveCount(ref interactions, g);
                UpdateTimingDictionaryBasedOffGmail(ref receivedMessagesTiming, g);
            }

            if (!emailThreadsDictionary.ContainsKey(g.ThreadId)) emailThreadsDictionary.Add(g.ThreadId, new List<Gmail>() { g });
            else emailThreadsDictionary[g.ThreadId].Add(g);
        }

        private void UpdateMessagesSentByDayDictionary(ref Dictionary<string,int> d, string shortDate)
        {
            if (!d.ContainsKey(shortDate)) d.Add(shortDate,1);
            else d[shortDate]++;
        }

        private void UpdateInteractionsDictionarySentCount(ref Dictionary<string,InteractionInfoDTO> interactions, Gmail g)
        {
            string parsedEmail = ParseEmail(g.To);
            if (!interactions.ContainsKey(parsedEmail)) interactions.Add(parsedEmail, new InteractionInfoDTO()); 
            interactions[parsedEmail].SentCount++;
            interactions[parsedEmail].AddMessage(g);
        }

        private void UpdateInteractionsDictionaryReceiveCount(ref Dictionary<string,InteractionInfoDTO> interactions, Gmail g)
        {
            string parsedEmail = ParseEmail(g.From);
            if (!interactions.ContainsKey(parsedEmail)) interactions.Add(parsedEmail, new InteractionInfoDTO()); 
            interactions[parsedEmail].ReceiveCount++;
            interactions[parsedEmail].AddMessage(g);
        }
        private string ParseEmail(string emailField)
        {
            int firstAngleBracketIndex = emailField.IndexOf('<');
            if (firstAngleBracketIndex == -1) return emailField;
            else return emailField.Substring(firstAngleBracketIndex+1,emailField.IndexOf('>') - firstAngleBracketIndex - 1);
        }
        private string ConvertIntHourToTimeStringFormat(int hour)
        {
            if (hour < 10) return "0" + hour.ToString() + ":00";
            else return hour.ToString() + ":00";
        }
        private string GetTimeframeOfMessage(Gmail gmail)
            => ConvertIntHourToTimeStringFormat(DateTimeOffset.FromUnixTimeMilliseconds(gmail.InternalDate).Hour);

        private void UpdateTimingDictionaryBasedOffGmail(ref Dictionary<string,WeeklyStatObject<int>> timingDictionary, Gmail g)
        {
            switch(DateTimeOffset.FromUnixTimeMilliseconds(g.InternalDate).DayOfWeek)
            {
                case DayOfWeek.Sunday: timingDictionary[GetTimeframeOfMessage(g)].Sunday++; break;
                case DayOfWeek.Monday: timingDictionary[GetTimeframeOfMessage(g)].Monday++; break;
                case DayOfWeek.Tuesday: timingDictionary[GetTimeframeOfMessage(g)].Tuesday++; break;
                case DayOfWeek.Wednesday: timingDictionary[GetTimeframeOfMessage(g)].Wednesday++; break;
                case DayOfWeek.Thursday: timingDictionary[GetTimeframeOfMessage(g)].Thursday++; break;
                case DayOfWeek.Friday: timingDictionary[GetTimeframeOfMessage(g)].Friday++; break;
                case DayOfWeek.Saturday: timingDictionary[GetTimeframeOfMessage(g)].Saturday++; break;
                default: throw new ArgumentException("Invalid day"); 
            }
        }
        private Dictionary<string,WeeklyStatObject<int>> GetTimingDictionary()
        {
            Dictionary<string,WeeklyStatObject<int>> receivedMessagesTiming = new();
            for (int i = 0; i < 24; i++)
                receivedMessagesTiming.Add(ConvertIntHourToTimeStringFormat(i), new());
            
            return receivedMessagesTiming;
        }

        public SummarizedEmailStatisticsDTO GetSummarizedUserEmailStatistics(User user, DateTime minDate, DateTime maxDate)
        {
            currentUserEmail = user.Email;
            SummarizedEmailStatisticsDTO summarizedEmailStatisticsDTO = new();
            summarizedEmailStatisticsDTO.Email = user.Email;

            List<Gmail> gmailStatObjects = user.Gmails.Where(g => g.WithinDateRange(minDate,maxDate)).ToList();
            Dictionary<string, List<Gmail>> emailThreadsDictionary = new();

            UpdateSummarizedStats(ref emailThreadsDictionary, ref gmailStatObjects, ref summarizedEmailStatisticsDTO);

            return summarizedEmailStatisticsDTO;
        }

        private void UpdateSummarizedStats(ref Dictionary<string, List<Gmail>> emailThreadsDictionary, ref List<Gmail> gmailStatObjects, ref SummarizedEmailStatisticsDTO summarizedEmailStatisticsDTO)
        {
            UpdateSentAndReceivedEmailCountsAndGetThreads(ref emailThreadsDictionary, ref gmailStatObjects, ref summarizedEmailStatisticsDTO);
            
            UpdateResponseStatistics(ref emailThreadsDictionary, ref summarizedEmailStatisticsDTO);
        }
        private void UpdateResponseStatistics(ref Dictionary<string, List<Gmail>> emailThreadsDictionary, ref SummarizedEmailStatisticsDTO summarizedEmailStatisticsDTO)
        {
            ExamineEmailThreads(ref emailThreadsDictionary, out float responseRate, out double averageResponseTime, out _, out _, out _, out _, out _, out _);
            
            summarizedEmailStatisticsDTO.ResponseRate = responseRate;
            summarizedEmailStatisticsDTO.AverageResponseTime = averageResponseTime;
        }

        private void ExamineEmailThreads(
            ref Dictionary<string,List<Gmail>> emailThreadsDictionary, 
            out float responseRate, out double averageResponseTime, 
            out double quickestResponseTime, 
            out double averageFirstResponseTime, 
            out Dictionary<string, int> timesBeforeFirstResponse,
            out SentBreakdown sentBreakdown,
            out ReceivedBreakdown receivedBreakdown,
            out EmailsReplied emailsReplied
        )
        {
            timesBeforeFirstResponse = new();
            foreach(var s in _firstResponseTimeRanges)
            {
                timesBeforeFirstResponse.Add(s, 0);
            }

            averageFirstResponseTime = -1;
            quickestResponseTime = -1;
            double responseTimeTotal = 0;
            int totalResponses = 0;
            int totalReceivedThreads = 0;
            double firstResponseTimeTotal = 0;
            int firstResponses = 0;
            sentBreakdown = new();
            receivedBreakdown = new();
            emailsReplied = new();

            foreach (var k in emailThreadsDictionary.Keys)
            {
                if (emailThreadsDictionary[k].Count == 1){
                    if (emailThreadsDictionary[k][0].Type == "Received") {
                        totalReceivedThreads++;
                        UpdatedReceivedBreakdownBasedOnGmail(ref receivedBreakdown,emailThreadsDictionary[k][0]);
                    }
                    else sentBreakdown.StartedThreads++;
                    emailThreadsDictionary.Remove(k);
                }
                else {
                    // Console.WriteLine("threadId: " + k);
                    emailThreadsDictionary[k].Sort();
                    if (emailThreadsDictionary[k][0].Type == "Received"){
                        totalReceivedThreads++;
                        UpdatedReceivedBreakdownBasedOnGmail(ref receivedBreakdown,emailThreadsDictionary[k][0]);
                        bool isFirstResponse = true;
                        for (int i = 1; i < emailThreadsDictionary[k].Count; i++){
                            if (emailThreadsDictionary[k][i].Type == "Sent"){
                                sentBreakdown.ToExistingThreads++;
                                if (emailThreadsDictionary[k][i-1].Type == "Received")
                                {
                                    double responseTime = ResponseTimeCalculator.GetMinutesBetweenTwoGmails(emailThreadsDictionary[k][i], emailThreadsDictionary[k][i-1]);
                                    responseTimeTotal += responseTime;
                                    totalResponses++;
                                    if (responseTime < quickestResponseTime || quickestResponseTime == -1) quickestResponseTime = responseTime;
                                    if (isFirstResponse) {
                                        firstResponseTimeTotal += responseTime;
                                        firstResponses++;
                                        timesBeforeFirstResponse[GetFirstResponseTimeRange(responseTime)]++;
                                        isFirstResponse = false;
                                    }
                                }
                            }
                            else UpdatedReceivedBreakdownBasedOnGmail(ref receivedBreakdown,emailThreadsDictionary[k][i]);
                        }
                    }
                    else sentBreakdown.StartedThreads++;
                }
            }

            responseRate = totalReceivedThreads > 0 ? (float)totalResponses / totalReceivedThreads * 100:-1;
            averageResponseTime = totalResponses > 0 ? responseTimeTotal / totalResponses:-1;
            averageFirstResponseTime = firstResponses > 0 ? firstResponseTimeTotal / firstResponses:-1;

            emailsReplied.RepliesSent.Type = "Sent";
            emailsReplied.RepliesSent.Replied = sentBreakdown.ToExistingThreads;
            emailsReplied.RepliesSent.NotReplied = sentBreakdown.StartedThreads;

            emailsReplied.RepliesReceived.Type = "Received";
            emailsReplied.RepliesReceived.Replied = receivedBreakdown.Cc + receivedBreakdown.Others;
            emailsReplied.RepliesReceived.NotReplied = receivedBreakdown.DirectMessages;
        }

        private void UpdatedReceivedBreakdownBasedOnGmail(ref ReceivedBreakdown receivedBreakdown, Gmail g)
        {
            if (ParseEmail(g.To) == currentUserEmail) receivedBreakdown.DirectMessages++;
            else if (ParseEmail(g.Cc) == currentUserEmail) receivedBreakdown.Cc++;
            else receivedBreakdown.Others++;
        }

        // responseTime is in minutes
        private string GetFirstResponseTimeRange(double responseTime)
        {
            return responseTime switch
            {
                <15 => _firstResponseTimeRanges[0],
                (>=15) and (<60) => _firstResponseTimeRanges[1],
                (>=60) and (<240) => _firstResponseTimeRanges[2],
                (>=240) and (<720) => _firstResponseTimeRanges[3],
                (>=720) and (<1440) => _firstResponseTimeRanges[4],
                (>=1440) and (<2880) => _firstResponseTimeRanges[5],
                _ => _firstResponseTimeRanges[6]
            };
        }
        private void UpdateSentAndReceivedEmailCountsAndGetThreads(ref Dictionary<string, List<Gmail>> emailThreadsDictionary, ref List<Gmail> gmailStatObjects, ref SummarizedEmailStatisticsDTO summarizedEmailStatisticsDTO)
        {
            int sentCount = 0, receiveCount = 0;

            foreach (var g in gmailStatObjects)
            {
                if (g.Type == "Sent") sentCount++;
                else if (g.Type == "Received") receiveCount++;

                if (!emailThreadsDictionary.ContainsKey(g.ThreadId)) emailThreadsDictionary.Add(g.ThreadId, new List<Gmail>() { g });
                else emailThreadsDictionary[g.ThreadId].Add(g);
            }

            summarizedEmailStatisticsDTO.ReceivedMessages = receiveCount;
            summarizedEmailStatisticsDTO.SentMessages = sentCount;
        }

        private async Task<List<GmailStatDTO>> GetGmailStatObjects(User user, DateTime minDate)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {user.AccessToken}");

            var gmailMetadata = await GetGmailMetadataHttpResponse(user,minDate);

            return await ConvertMetadataToGmailStatObjectList(gmailMetadata, minDate);
        }

        private async Task<List<GmailStatDTO>> ConvertMetadataToGmailStatObjectList(IEnumerable<HttpResponseMessage> gmailMetadata, DateTime minDate)
        {
            var gmailStatObjects = new List<GmailStatDTO>();
            foreach (var g in gmailMetadata)
            {
                var gmailApiResponseObject = await g.Content.ReadFromJsonAsync<GmailApiResponseDTO>();
                    if (gmailApiResponseObject.Payload != null){
                        if (GmailApiResponseBeforeDate(gmailApiResponseObject,minDate)) 
                            gmailStatObjects.Add(GmailStatDTO.GetGmailStatFromApiResponse(gmailApiResponseObject));
                    }
                    else 
                        Console.WriteLine("null: " + g.StatusCode);
            }
            return gmailStatObjects;
        }

        private async Task<IEnumerable<HttpResponseMessage>> GetGmailMetadataHttpResponse(User user, DateTime minDate)
        {
            var gmailApiResponses = GetUserMessageList(user,minDate).GetAwaiter().GetResult().Select
            (
                async resp => JsonSerializer.Deserialize<GmailApiResponseDTO>
                                (await resp.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            );
            
            var gmailMessageList = new List<GmailMessageListObject>(); 
            foreach(var t in gmailApiResponses) {
                if (t.Result.Messages != null)
                    gmailMessageList.AddRange(t.Result.Messages);
            }
            
            var gmailMetadata = await GetMetadata(user.Email, gmailMessageList);

            return gmailMetadata.Select
            (
                task => task.Result
            );
        }

        private async Task<IEnumerable<HttpContent>> GetUserMessageList(User user, DateTime minDate)
        {
            List<Task<IEnumerable<HttpContent>>> listTasks = new()
            {
                CallUserMessageListEndpoint(user, "Sent", minDate),
                CallUserMessageListEndpoint(user, "Received", minDate),
            };

            await Task.WhenAll(listTasks);

            return listTasks[0].Result.Concat(listTasks[1].Result);
        }

        private async Task<IEnumerable<HttpContent>> CallUserMessageListEndpoint(User user, string msgType, DateTime minDate)
        {
            string endpoint;

            switch(msgType)
            {
                case "Sent": endpoint = GMAIL_USER_MESSAGES_LIST_SENT_ENDPOINT; break;
                case "Received": endpoint = GMAIL_USER_MESSAGES_LIST_RECEIVED_ENDPOINT; break;
                default: throw new ArgumentException("Invalid endpoint type");
            }

            List<HttpContent> messageListResponses = new();
            var receivedList = await _httpClient.GetAsync(string.Format(endpoint, user.Email, ""));

            messageListResponses.Add(receivedList.Content);

            while(true)
            {
                var receivedListFromJson = await receivedList.Content.ReadFromJsonAsync<GmailApiResponseDTO>();
                var nextPageToken = receivedListFromJson.NextPageToken;

                if (string.IsNullOrEmpty(nextPageToken)) return messageListResponses;

                var lastMessageListResponseId = receivedListFromJson.Messages.Last().Id; 
                                
                var lastMessageMetadata = await AttemptToGetMetadataForSingleGmail(user.Email, lastMessageListResponseId);
                var lastMessageGmailApiResponse = JsonSerializer.Deserialize<GmailApiResponseDTO>(await lastMessageMetadata.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var lastMessageDateLinux = long.Parse(lastMessageGmailApiResponse.InternalDate);
                var lastMessageDate = DateTimeOffset.FromUnixTimeMilliseconds(lastMessageDateLinux).Date;
                
                if (lastMessageDate < minDate) return messageListResponses;

                receivedList = await _httpClient.GetAsync(string.Format(endpoint, user.Email, nextPageToken));
                messageListResponses.Add(receivedList.Content);
            }
        }

        private async Task<List<Task<HttpResponseMessage>>> GetMetadata(string email, List<GmailMessageListObject> gmailMessageList)
        {
            var gmailMessageRequests = gmailMessageList.Select
            (
                gmailMessageListObject => AttemptToGetMetadataForSingleGmail(email, gmailMessageListObject.Id)
            ).ToList();

            await Task.WhenAll(gmailMessageRequests);

            return gmailMessageRequests;
        }

        // Used the algorithm described here: https://developers.google.com/sheets/api/limits#exponential
        private async Task<HttpResponseMessage> AttemptToGetMetadataForSingleGmail(string email, string messageId)
        {
            Random rand = new Random();

            int maximumBackoff = 64000; // max wait time is 64 seconds
            int numRetries = 0;

            var res = await _httpClient.GetAsync(String.Format(GMAIL_GET_EMAIL_ENDPOINT, email, messageId));

            while (res.StatusCode != HttpStatusCode.OK)
            {
                // Console.WriteLine("Received too many requests response for messageId " + messageId + ", trying again...");
                int randomNumberMilliseconds = rand.Next(1000);
                int calculatedWaitTime = TwoPow(numRetries) + randomNumberMilliseconds;
                Thread.Sleep(Math.Min(calculatedWaitTime, maximumBackoff));

                res = await _httpClient.GetAsync(String.Format(GMAIL_GET_EMAIL_ENDPOINT, email, messageId));

                if (calculatedWaitTime <= maximumBackoff) numRetries++;
                if (numRetries >= 30) throw new TimeoutException("Maximum number of retries exceeded. Try logging in again");
            }

            return res;
        }

        private int TwoPow(int power) => 1<<power;

        private bool GmailApiResponseBeforeDate(GmailApiResponseDTO g, DateTime minDate)
        {
            var mailDateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(g.InternalDate)).Date;

            return minDate - mailDateTime < TimeSpan.Zero;
        }

        public List<SummarizedEmailStatisticsDTO> GetTeamSummarizedStatistics(User owner, Team team, DateTime minDate, DateTime maxDate)
        {
            List<SummarizedEmailStatisticsDTO> stats = new()
            {
                GetSummarizedUserEmailStatistics(owner, minDate, maxDate)
            };

            foreach (var user in team.Users)
            {
                if (!user.InvitePending){
                    var userEmailStats = GetSummarizedUserEmailStatistics(user,minDate,maxDate);
                    stats.Add(userEmailStats);
                }
            }

            return stats;
        }

        public async Task SaveGmailsForUserAfterRegister(User user)
        {
            var gmailStatDTOs = await GetGmailStatObjects(user, DateTime.Now.AddMonths(-2));

            _gmailRepo.AddGmailRangeForUserFromGmailStatDTOList(gmailStatDTOs,user);
            _gmailRepo.SaveChanges();
        }

        public async Task RefreshUserGmails(User user, DateTime removeDate, DateTime getDate)
        {
            var gmailStatDTOs = await GetGmailStatObjects(user, getDate);

            _gmailRepo.RemoveGmailsForUserAtCertainDate(removeDate, user);
            _gmailRepo.AddGmailRangeForUserFromGmailStatDTOList(gmailStatDTOs, user);

            _gmailRepo.SaveChanges();
        }
    }
}