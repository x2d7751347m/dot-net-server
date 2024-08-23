USE DotNetCourseDatabase;
GO

CREATE OR ALTER PROCEDURE spUsers_Get
/*EXEC spUsers_Get @UserId=3*/
    @UserId INT = NULL
    , @Active BIT = NULL
AS
BEGIN
    DROP TABLE IF EXISTS #AverageDeptSalary

    SELECT UserJobInfo.Department
        , AVG(UserSalary.Salary) AvgSalary
        INTO #AverageDeptSalary
    FROM Users AS Users 
        LEFT JOIN UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary_Department ON #AverageDeptSalary(Department)

    SELECT [Users].[UserId],
        [Users].[FirstName],
        [Users].[LastName],
        [Users].[Email],
        [Users].[Gender],
        [Users].[Active],
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvgSalary.AvgSalary
    FROM Users AS Users 
        LEFT JOIN UserSalary AS UserSalary
            ON UserSalary.UserId = Users.UserId
        LEFT JOIN UserJobInfo AS UserJobInfo
            ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary AS AvgSalary
            ON AvgSalary.Department = UserJobInfo.Department
        WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
            AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END
GO

CREATE OR ALTER PROCEDURE spUser_Upsert
	@FirstName NVARCHAR(50),
	@LastName NVARCHAR(50),
	@Email NVARCHAR(50),
	@Gender NVARCHAR(50),
	@JobTitle NVARCHAR(50),
	@Department NVARCHAR(50),
    @Salary DECIMAL(18, 4),
	@Active BIT = 1,
	@UserId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM Users WHERE UserId = @UserId)
        BEGIN
        IF NOT EXISTS (SELECT * FROM Users WHERE Email = @Email)
            BEGIN
                DECLARE @OutputUserId INT

                INSERT INTO Users(
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]
                ) VALUES (
                    @FirstName,
                    @LastName,
                    @Email,
                    @Gender,
                    @Active
                )

                SET @OutputUserId = @@IDENTITY

                INSERT INTO UserSalary(
                    UserId,
                    Salary
                ) VALUES (
                    @OutputUserId,
                    @Salary
                )

                INSERT INTO UserJobInfo(
                    UserId,
                    Department,
                    JobTitle
                ) VALUES (
                    @OutputUserId,
                    @Department,
                    @JobTitle
                )
            END
        END
    ELSE 
        BEGIN
            UPDATE Users 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Gender = @Gender,
                    Active = @Active
                WHERE UserId = @UserId

            UPDATE UserSalary
                SET Salary = @Salary
                WHERE UserId = @UserId

            UPDATE UserJobInfo
                SET Department = @Department,
                    JobTitle = @JobTitle
                WHERE UserId = @UserId
        END
END
GO

CREATE OR ALTER PROCEDURE spUser_Delete
    @UserId INT
AS
BEGIN
    DECLARE @Email NVARCHAR(50);

    SELECT  @Email = Users.Email
      FROM  Users
     WHERE  Users.UserId = @UserId;

    DELETE  FROM UserSalary
     WHERE  UserSalary.UserId = @UserId;

    DELETE  FROM UserJobInfo
     WHERE  UserJobInfo.UserId = @UserId;

    DELETE  FROM Users
     WHERE  Users.UserId = @UserId;

    DELETE  FROM Auth
     WHERE  Auth.Email = @Email;
END;
GO



CREATE OR ALTER PROCEDURE spPosts_Get
/*EXEC spPosts_Get @UserId = 1003, @SearchValue='Second'*/
/*EXEC spPosts_Get @PostId = 2*/
    @UserId INT = NULL
    , @SearchValue NVARCHAR(MAX) = NULL
    , @PostId INT = NULL
AS
BEGIN
    SELECT [Posts].[PostId],
        [Posts].[UserId],
        [Posts].[PostTitle],
        [Posts].[PostContent],
        [Posts].[PostCreated],
        [Posts].[PostUpdated] 
    FROM Posts AS Posts
        WHERE Posts.UserId = ISNULL(@UserId, Posts.UserId)
            AND Posts.PostId = ISNULL(@PostId, Posts.PostId)
            AND (@SearchValue IS NULL
                OR Posts.PostContent LIKE '%' + @SearchValue + '%'
                OR Posts.PostTitle LIKE '%' + @SearchValue + '%')
END
GO

CREATE OR ALTER PROCEDURE spPosts_Upsert
    @UserId INT
    , @PostTitle NVARCHAR(255)
    , @PostContent NVARCHAR(MAX)
    , @PostId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT * FROM Posts WHERE PostId = @PostId)
        BEGIN
            INSERT INTO Posts(
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]
            ) VALUES (
                @UserId,
                @PostTitle,
                @PostContent,
                GETDATE(),
                GETDATE()
            )
        END
    ELSE
        BEGIN
            UPDATE Posts 
                SET PostTitle = @PostTitle,
                    PostContent = @PostContent,
                    PostUpdated = GETDATE()
                WHERE PostId = @PostId
        END
END
GO

CREATE OR ALTER PROCEDURE spPost_Delete
    @PostId INT
    , @UserId INT 
AS
BEGIN
    DELETE FROM Posts 
        WHERE PostId = @PostId
            AND UserId = @UserId
END
GO



CREATE OR ALTER PROCEDURE spLoginConfirmation_Get
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT [Auth].[PasswordHash],
        [Auth].[PasswordSalt] 
    FROM Auth AS Auth 
        WHERE Auth.Email = @Email
END;
GO

CREATE OR ALTER PROCEDURE spRegistration_Upsert
    @Email NVARCHAR(50),
    @PasswordHash VARBINARY(MAX),
    @PasswordSalt VARBINARY(MAX)
AS 
BEGIN
    IF NOT EXISTS (SELECT * FROM Auth WHERE Email = @Email)
        BEGIN
            INSERT INTO Auth(
                [Email],
                [PasswordHash],
                [PasswordSalt]
            ) VALUES (
                @Email,
                @PasswordHash,
                @PasswordSalt
            )
        END
    ELSE
        BEGIN
            UPDATE Auth 
                SET PasswordHash = @PasswordHash,
                    PasswordSalt = @PasswordSalt
                WHERE Email = @Email
        END
END
GO