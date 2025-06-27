using System;
using System.Collections.Generic;

namespace FAiSEBussiness.Models;

public partial class Blog
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Avatar { get; set; }

    public int CategoryId { get; set; }

    public string? Summary { get; set; }

    public string? Ncontent { get; set; }

    public DateTime DateUpdated { get; set; }

    public int UserId { get; set; }

    public bool Status { get; set; }

    public virtual Category? Category { get; set; } = null!;

    public virtual User? User { get; set; } = null!;
}
