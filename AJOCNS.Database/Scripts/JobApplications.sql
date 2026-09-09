/* Database-first setup script. Execute this against the existing AJOCNS database. */
IF OBJECT_ID(N'dbo.JobApplications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobApplications
    (
        JobApplication_Id int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_JobApplications PRIMARY KEY,
        JobPostId int NOT NULL,
        User_Id int NOT NULL,
        CoverLetter nvarchar(1000) NOT NULL,
        ResumeUrl nvarchar(500) NULL,
        AppliedDate datetime2 NOT NULL
            CONSTRAINT DF_JobApplications_AppliedDate DEFAULT (getutcdate()),
        Status nvarchar(50) NOT NULL
            CONSTRAINT DF_JobApplications_Status DEFAULT ('Pending'),
        CONSTRAINT UQ_JobApplications_JobPost_User UNIQUE (JobPostId, User_Id),
        CONSTRAINT FK_JobApplications_JobPosts
            FOREIGN KEY (JobPostId) REFERENCES dbo.JobPosts(JobPost_Id) ON DELETE CASCADE,
        CONSTRAINT FK_JobApplications_Users
            FOREIGN KEY (User_Id) REFERENCES dbo.Users(User_ID)
    );
END;
GO

/* These indexes are safe to run after the table has been created. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobApplications_JobPostId')
    CREATE INDEX IX_JobApplications_JobPostId ON dbo.JobApplications(JobPostId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobApplications_UserId')
    CREATE INDEX IX_JobApplications_UserId ON dbo.JobApplications(User_Id);
GO
