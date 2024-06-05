using System;
using System.Collections.Generic;

namespace CoursesApp.Data;

public partial class Comment
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int TaskId { get; set; }

    public string Comment1 { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
