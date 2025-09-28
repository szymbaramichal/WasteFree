using System;

namespace WasteFree.Business.Features.Inbox.Dtos;

public class InboxMessageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedDateUtc { get; set; }
}
