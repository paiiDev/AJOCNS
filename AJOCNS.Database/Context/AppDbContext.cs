using System;
using System.Collections.Generic;
using AJOCNS.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace AJOCNS.Database.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Degree> Degrees { get; set; }

    public virtual DbSet<EmploymentRecord> EmploymentRecords { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventRegistration> EventRegistrations { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<ExternalPartner> ExternalPartners { get; set; }

    public virtual DbSet<GraduationRecord> GraduationRecords { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<Mentor> Mentors { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=AJOCNS_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.AcyId).HasName("PK_Acacdmic_Years");

            entity.ToTable("Academic_Years");

            entity.HasIndex(e => e.AcademicYear1, "UQ_Acacdmic_Years_AcademicYear").IsUnique();

            entity.Property(e => e.AcyId).HasColumnName("ACY_ID");
            entity.Property(e => e.AcademicYear1)
                .HasMaxLength(20)
                .HasColumnName("AcademicYear");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Admins_User").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("Admin_ID");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Admins_Users");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.CompanyName, "UQ_Companies_Name").IsUnique();

            entity.Property(e => e.CompanyId).HasColumnName("Company_ID");
            entity.Property(e => e.CompanyName).HasMaxLength(255);
        });

        modelBuilder.Entity<Degree>(entity =>
        {
            entity.Property(e => e.DegreeId).HasColumnName("Degree_ID");
            entity.Property(e => e.DegreeCode).HasMaxLength(100);
            entity.Property(e => e.DegreeName).HasMaxLength(200);
        });

        modelBuilder.Entity<EmploymentRecord>(entity =>
        {
            entity.HasKey(e => e.EmploymentRId);

            entity.HasIndex(e => e.CompanyId, "IX_EmploymentRecords_Company_ID");

            entity.HasIndex(e => e.MentorId, "IX_EmploymentRecords_Mentor_ID");

            entity.HasIndex(e => e.PositionId, "IX_EmploymentRecords_Position_ID");

            entity.Property(e => e.EmploymentRId).HasColumnName("Employment_R_ID");
            entity.Property(e => e.CompanyId).HasColumnName("Company_ID");
            entity.Property(e => e.MentorId).HasColumnName("Mentor_ID");
            entity.Property(e => e.PositionId).HasColumnName("Position_ID");

            entity.HasOne(d => d.Company).WithMany(p => p.EmploymentRecords)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmploymentRecords_Companies");

            entity.HasOne(d => d.Mentor).WithMany(p => p.EmploymentRecords)
                .HasForeignKey(d => d.MentorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmploymentRecords_Mentors");

            entity.HasOne(d => d.Position).WithMany(p => p.EmploymentRecords)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmploymentRecords_Positions");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.ErId);

            entity.HasIndex(e => e.AcyId, "IX_Enrollments_ACY_ID");

            entity.HasIndex(e => e.StudentId, "IX_Enrollments_Student_ID");

            entity.HasIndex(e => new { e.StudentId, e.AcyId }, "UQ_Enrollment_Student_ACY").IsUnique();

            entity.Property(e => e.ErId).HasColumnName("ER_ID");
            entity.Property(e => e.AcyId).HasColumnName("ACY_ID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Enrolled");
            entity.Property(e => e.StudentId).HasColumnName("Student_ID");

            entity.HasOne(d => d.Acy).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.AcyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_AcademicYears");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Students");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => e.CreatedByUserId, "IX_Events_CreatedByUser_Id");

            entity.HasIndex(e => e.EventDate, "IX_Events_EventDate");

            entity.HasIndex(e => e.EventTypeId, "IX_Events_EventType_ID");

            entity.Property(e => e.EventId).HasColumnName("Event_ID");
            entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUser_Id");
            entity.Property(e => e.EventMode).HasMaxLength(50);
            entity.Property(e => e.EventTitle).HasMaxLength(255);
            entity.Property(e => e.EventTypeId).HasColumnName("EventType_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Upcoming");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Events)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_Users");

            entity.HasOne(d => d.EventType).WithMany(p => p.Events)
                .HasForeignKey(d => d.EventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_EventTypes");
        });

        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.HasKey(e => e.EventRegiId);

            entity.HasIndex(e => e.EventId, "IX_EventRegistrations_Event_ID");

            entity.HasIndex(e => e.StudentId, "IX_EventRegistrations_Student_ID");

            entity.HasIndex(e => new { e.StudentId, e.EventId }, "UQ_EventRegistration_Student_Event").IsUnique();

            entity.Property(e => e.EventRegiId).HasColumnName("Event_Regi_ID");
            entity.Property(e => e.EventId).HasColumnName("Event_ID");
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Registered");
            entity.Property(e => e.StudentId).HasColumnName("Student_ID");

            entity.HasOne(d => d.Event).WithMany(p => p.EventRegistrations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventRegistrations_Events");

            entity.HasOne(d => d.Student).WithMany(p => p.EventRegistrations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventRegistrations_Students");
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("Event_Types");

            entity.HasIndex(e => e.EventTypeName, "UQ_Event_Types_EventType").IsUnique();

            entity.Property(e => e.EventTypeId).HasColumnName("EventType_ID");
            entity.Property(e => e.EventTypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<ExternalPartner>(entity =>
        {
            entity.ToTable("External_Partners");

            entity.HasIndex(e => e.CompanyId, "IX_ExternalPartners_Company_ID");

            entity.HasIndex(e => e.PositionId, "IX_ExternalPartners_Position_ID");

            entity.HasIndex(e => e.UserId, "UQ_ExternalPartners_User").IsUnique();

            entity.Property(e => e.ExternalPartnerId).HasColumnName("External_Partner_ID");
            entity.Property(e => e.CompanyId).HasColumnName("Company_ID");
            entity.Property(e => e.Expertise).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.PositionId).HasColumnName("Position_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Company).WithMany(p => p.ExternalPartners)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExternalPartners_Companies");

            entity.HasOne(d => d.Position).WithMany(p => p.ExternalPartners)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExternalPartners_Positions");

            entity.HasOne(d => d.User).WithOne(p => p.ExternalPartner)
                .HasForeignKey<ExternalPartner>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExternalPartners_Users");
        });

        modelBuilder.Entity<GraduationRecord>(entity =>
        {
            entity.HasKey(e => e.GrecordId).HasName("PK_GraduationRecords");

            entity.ToTable("Graduation_Records");

            entity.HasIndex(e => e.Grn, "UQ_GraduationRecords_GRN").IsUnique();

            entity.Property(e => e.GrecordId).HasColumnName("GRecord_Id");
            entity.Property(e => e.AccStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("Acc_Status");
            entity.Property(e => e.DegreeId).HasColumnName("Degree_ID");
            entity.Property(e => e.Grn)
                .HasMaxLength(100)
                .HasColumnName("GRN");
            entity.Property(e => e.OfficialName).HasMaxLength(255);
            entity.Property(e => e.StudentId).HasColumnName("Student_ID");

            entity.HasOne(d => d.Degree).WithMany(p => p.GraduationRecords)
                .HasForeignKey(d => d.DegreeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Degree_GraduationRecoreds");

            entity.HasOne(d => d.Student).WithMany(p => p.GraduationRecords)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_Graduation_Records_Students");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasIndex(e => e.MajorName, "UQ_Majors_MajorName").IsUnique();

            entity.Property(e => e.MajorId).HasColumnName("Major_ID");
            entity.Property(e => e.DegreeId).HasColumnName("Degree_ID");
            entity.Property(e => e.IsFoundation)
                .HasDefaultValue(false)
                .HasColumnName("isFoundation");
            entity.Property(e => e.MajorName).HasMaxLength(150);

            entity.HasOne(d => d.Degree).WithMany(p => p.Majors)
                .HasForeignKey(d => d.DegreeId)
                .HasConstraintName("FK_Majors_Degrees");
        });

        modelBuilder.Entity<Mentor>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Mentors_User").IsUnique();

            entity.Property(e => e.MentorId).HasColumnName("Mentor_ID");
            entity.Property(e => e.AlumniGrn)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("Alumni_GRN");
            entity.Property(e => e.AlumniGy).HasColumnName("Alumni_GY");
            entity.Property(e => e.Expertise).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithOne(p => p.Mentor)
                .HasForeignKey<Mentor>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mentors_Users");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasIndex(e => e.Position1, "UQ_Positions_Position").IsUnique();

            entity.Property(e => e.PositionId).HasColumnName("Position_ID");
            entity.Property(e => e.Position1)
                .HasMaxLength(150)
                .HasColumnName("Position");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.MajorId, "IX_Students_Major_ID");

            entity.HasIndex(e => e.Srn, "UQ_Students_SRN").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_Students_User").IsUnique();

            entity.Property(e => e.StudentId).HasColumnName("Student_ID");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.FatherName).HasMaxLength(255);
            entity.Property(e => e.GraduationStatus).HasMaxLength(50);
            entity.Property(e => e.MajorId).HasColumnName("Major_ID");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Srn)
                .HasMaxLength(100)
                .HasColumnName("SRN");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Major).WithMany(p => p.Students)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Majors");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.IsFirstLogin)
                .HasDefaultValue(true)
                .HasColumnName("isFirstLogin");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
