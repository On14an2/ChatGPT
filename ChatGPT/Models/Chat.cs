using System;
using System.Collections.Generic;

namespace ChatGPT.Models;

public partial class Chat
{
    public int ChatId { get; set; }

    public int? UserId { get; set; }

    public string? Role { get; set; }

    public string? Message { get; set; }

    public virtual User? User { get; set; }
}
