namespace CampaignManager.Shared.DTOs;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class SendMessageRequest
{
    public Guid ReceiverId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ConversationDto
{
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int UnreadCount { get; set; }
    public ChatMessageDto? LastMessage { get; set; }
}
