using System;
using System.Collections.Generic;

namespace CoursesApp.Data;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string TeacherId { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual User Teacher { get; set; }
}
