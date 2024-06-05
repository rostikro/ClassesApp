using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CoursesApp.Data;

public partial class User : IdentityUser
{
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
