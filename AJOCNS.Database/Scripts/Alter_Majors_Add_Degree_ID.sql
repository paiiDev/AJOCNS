-- Alter_Majors_Add_Degree_ID.sql
-- Links Majors to Degrees (Major.Degree_ID -> Degrees.Degree_ID).
--
-- Safe to run on any database; each step only runs when the target does not
-- already exist.
--
-- 1. ALTER TABLE Majors ADD Degree_ID INT NULL
-- 2. Backfill by known codes:
--      Electronic Power  -> EP
--      Electronic        -> EC
--      Civil Engineering -> Civil
--      Mechanical        -> Mech
--    Fallback: DegreeName LIKE '%' + MajorName + '%' (e.g. Computer Science -> Bachelor of Computer Science)
-- 3. Add constraint FK_Majors_Degrees
-- 4. Repair existing bad records: re-point any GraduationRecord whose degree
--    differs from its student's current major's degree.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Majors') AND name = N'Degree_ID')
BEGIN
    ALTER TABLE dbo.Majors ADD Degree_ID INT NULL;

    UPDATE m
    SET m.Degree_ID = d.Degree_ID
    FROM dbo.Majors m
    INNER JOIN dbo.Degrees d
        ON (d.DegreeCode = 'EP'    AND m.MajorName = 'Electronic Power')
        OR (d.DegreeCode = 'EC'    AND m.MajorName = 'Electronic')
        OR (d.DegreeCode = 'Civil' AND m.MajorName = 'Civil Engineering')
        OR (d.DegreeCode = 'Mech'  AND m.MajorName = 'Mechanical')
        OR (m.Degree_ID IS NULL AND d.DegreeName LIKE '%' + m.MajorName + '%');

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Majors_Degrees')
    BEGIN
        ALTER TABLE dbo.Majors
        ADD CONSTRAINT FK_Majors_Degrees
            FOREIGN KEY (Degree_ID) REFERENCES dbo.Degrees(Degree_ID);
    END

    UPDATE gr
    SET gr.Degree_ID = m.Degree_ID
    FROM dbo.Graduation_Records gr
    INNER JOIN dbo.Students s ON s.Student_ID = gr.Student_ID
    INNER JOIN dbo.Majors m ON m.Major_ID = s.Major_ID
    WHERE m.Degree_ID IS NOT NULL
      AND (gr.Degree_ID IS NULL OR gr.Degree_ID <> m.Degree_ID);
END