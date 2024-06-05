using System;
using System.Collections.Generic;

namespace CoursesApp.Data;

public partial class Enrollment
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int CourseId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
