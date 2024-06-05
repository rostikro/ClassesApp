using System;
using System.Collections.Generic;

namespace CoursesApp.Data;

public partial class Submission
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime SubmissionDate { get; set; }

    public string? Url { get; set; }

    public int? Grade { get; set; }

    public string? Feedback { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
