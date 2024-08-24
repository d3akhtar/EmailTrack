export interface summarizedStats {
    email : string,
    receivedMessages: number;
    sentMessages: number;
    responseRate: number;
    averageResponseTime: number;
}

export interface detailedStats {
    receivedMessages: number
    sentMessages: number
    recipients: number
    senders: number
    responseRate: number
    averageResponseTime: number
    quickestResponseTime: number
    averageFirstResponseTime: number
    timesBeforeFirstResponse: { [Name: string]: number}
    receivedMessagesTiming: { [Name: string]: weeklyStatObject}
    sentMessagesTiming: { [Name: string]: weeklyStatObject}
    interactions: { [Name: string]: interaction}
    messagesSentByDay: { [Name: string]: number}
    messagesReceivedByDay: { [Name: string]: number}
    emailsRepliedBreakdown: emailsReplied
    sentBreakdownStats: sentBreakdown
    receivedBreakdownStats: receivedBreakdown
}

export interface interaction {
    totalInteractions: number
    sentCount: number
    receiveCount: number
    bestContactTime: number
    responseTime: number
}

export interface emailsReplied {
    repliesSent: replyBreakdown,
    repliesReceived: replyBreakdown
}

export interface replyBreakdown {
    type: string
    replied: number
    notReplied: number
}

export interface sentBreakdown {
    startedThreads: number
    toExistingThreads: number
}

export interface receivedBreakdown {
    directMessages: number
    cc: number
    others: number
}

export interface weeklyStatObject {
    sunday : number
    monday: number
    tuesday: number
    wednesday: number
    thursday: number
    friday: number
    saturday:number
}