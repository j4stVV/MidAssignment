﻿namespace Lib.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
}
