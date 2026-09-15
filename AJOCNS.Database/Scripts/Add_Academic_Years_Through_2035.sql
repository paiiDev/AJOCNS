-- Extends the Academic_Years lookup table so that enrollment years are available up to 2035.
IF OBJECT_ID('tempdb..#NewYears') IS NOT NULL DROP TABLE #NewYears;

CREATE TABLE #NewYears (AcademicYear NVARCHAR(20));

INSERT INTO #NewYears (AcademicYear) VALUES
    (N'2031-2032'),
    (N'2032-2033'),
    (N'2033-2034'),
    (N'2034-2035'),
    (N'2035-2036');

INSERT INTO dbo.[Academic_Years] ([AcademicYear])
SELECT n.[AcademicYear]
FROM #NewYears n
WHERE NOT EXISTS (SELECT 1 FROM dbo.[Academic_Years] a WHERE a.[AcademicYear] = n.[AcademicYear]);

DROP TABLE #NewYears;