using System;
using System.Collections.Generic;

namespace FAiSEBussiness.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }
    public string? LinkUrl { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool SubCategoryStatus { get; set; }

    public bool ShowMenuStatus { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual Category? ParentCategory { get; set; }
}
