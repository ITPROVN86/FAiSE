using System;
using System.Collections.Generic;

namespace FAiSEBussiness.Models;

public partial class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string Mail { get; set; } = null!;

    public bool Status { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
