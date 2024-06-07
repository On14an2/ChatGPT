using System;
using System.Collections.Generic;

namespace ChatGPT.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
}
