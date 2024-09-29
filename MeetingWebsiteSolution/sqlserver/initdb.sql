USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'MeetingDb')
BEGIN
    CREATE DATABASE MeetingDb;
END
GO
