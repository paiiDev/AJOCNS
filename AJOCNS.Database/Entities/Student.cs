using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string Srn { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? FatherName { get; set; }

    public string? Address { get; set; }

    public int? GrecordId { get; set; }

    public int MajorId { get; set; }

    public string? GraduationStatus { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public virtual GraduationRecord? Grecord { get; set; }

    public virtual Major Major { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
