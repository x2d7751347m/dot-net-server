USE dot_net_api_database;

CREATE TABLE Users (
                       UserId INT AUTO_INCREMENT PRIMARY KEY,
                       FirstName VARCHAR(50),
                       LastName VARCHAR(50),
                       Email VARCHAR(50),
                       Gender VARCHAR(50),
                       Active BOOLEAN
);

CREATE INDEX idx_Users_Active ON Users (Active, Email, FirstName, LastName, Gender) WHERE active = 1;

CREATE TABLE UserSalary (
                            UserId INT,
                            Salary DECIMAL(18, 4),
                            INDEX idx_UserSalary_UserId (UserId)
);

CREATE TABLE UserJobInfo (
                             UserId INT,
                             JobTitle VARCHAR(50),
                             Department VARCHAR(50),
                             INDEX idx_UserJobInfo_JobTitle (JobTitle, Department)
);

CREATE TABLE Auth (
                      Email VARCHAR(50) PRIMARY KEY,
                      PasswordHash BLOB,
                      PasswordSalt BLOB
);

CREATE TABLE Posts (
                       PostId INT AUTO_INCREMENT PRIMARY KEY,
                       UserId INT,
                       PostTitle VARCHAR(255),
                       PostContent TEXT,
                       PostCreated DATETIME,
                       PostUpdated DATETIME,
                       INDEX idx_Posts_UserId_PostId (UserId, PostId)
);

DELIMITER //

CREATE PROCEDURE spUsers_Get(
    IN p_UserId INT,
    IN p_Active BOOLEAN
)
BEGIN
    DROP TEMPORARY TABLE IF EXISTS AverageDeptSalary;

    CREATE TEMPORARY TABLE AverageDeptSalary AS
    SELECT UserJobInfo.Department, AVG(UserSalary.Salary) AS AvgSalary
    FROM Users
             LEFT JOIN UserSalary ON UserSalary.UserId = Users.UserId
             LEFT JOIN UserJobInfo ON UserJobInfo.UserId = Users.UserId
    GROUP BY UserJobInfo.Department;

    CREATE INDEX idx_AverageDeptSalary_Department ON AverageDeptSalary(Department);

    SELECT Users.UserId,
           Users.FirstName,
           Users.LastName,
           Users.Email,
           Users.Gender,
           Users.Active,
           UserSalary.Salary,
           UserJobInfo.Department,
           UserJobInfo.JobTitle,
           AvgSalary.AvgSalary
    FROM Users
             LEFT JOIN UserSalary ON UserSalary.UserId = Users.UserId
             LEFT JOIN UserJobInfo ON UserJobInfo.UserId = Users.UserId
             LEFT JOIN AverageDeptSalary AS AvgSalary ON AvgSalary.Department = UserJobInfo.Department
    WHERE Users.UserId = IFNULL(p_UserId, Users.UserId)
      AND IFNULL(Users.Active, 0) = IFNULL(p_Active, Users.Active);
END //

CREATE PROCEDURE spUser_Upsert(
    IN p_FirstName VARCHAR(50),
    IN p_LastName VARCHAR(50),
    IN p_Email VARCHAR(50),
    IN p_Gender VARCHAR(50),
    IN p_JobTitle VARCHAR(50),
    IN p_Department VARCHAR(50),
    IN p_Salary DECIMAL(18, 4),
    IN p_Active BOOLEAN,
    IN p_UserId INT
)
BEGIN
    IF NOT EXISTS (SELECT * FROM Users WHERE UserId = p_UserId) THEN
        IF NOT EXISTS (SELECT * FROM Users WHERE Email = p_Email) THEN
            INSERT INTO Users(FirstName, LastName, Email, Gender, Active)
            VALUES (p_FirstName, p_LastName, p_Email, p_Gender, p_Active);

            SET @OutputUserId = LAST_INSERT_ID();

            INSERT INTO UserSalary(UserId, Salary)
            VALUES (@OutputUserId, p_Salary);

            INSERT INTO UserJobInfo(UserId, Department, JobTitle)
            VALUES (@OutputUserId, p_Department, p_JobTitle);
        END IF;
    ELSE
        UPDATE Users
        SET FirstName = p_FirstName,
            LastName = p_LastName,
            Email = p_Email,
            Gender = p_Gender,
            Active = p_Active
        WHERE UserId = p_UserId;

        UPDATE UserSalary
        SET Salary = p_Salary
        WHERE UserId = p_UserId;

        UPDATE UserJobInfo
        SET Department = p_Department,
            JobTitle = p_JobTitle
        WHERE UserId = p_UserId;
    END IF;
END //

CREATE PROCEDURE spUser_Delete(
    IN p_UserId INT
)
BEGIN
    DECLARE v_Email VARCHAR(50);

    SELECT Email INTO v_Email FROM Users WHERE UserId = p_UserId;

    DELETE FROM UserSalary WHERE UserId = p_UserId;
    DELETE FROM UserJobInfo WHERE UserId = p_UserId;
    DELETE FROM Users WHERE UserId = p_UserId;
    DELETE FROM Auth WHERE Email = v_Email;
END //

CREATE PROCEDURE spPosts_Get(
    IN p_UserId INT,
    IN p_SearchValue TEXT,
    IN p_PostId INT
)
BEGIN
    SELECT PostId, UserId, PostTitle, PostContent, PostCreated, PostUpdated
    FROM Posts
    WHERE UserId = IFNULL(p_UserId, UserId)
      AND PostId = IFNULL(p_PostId, PostId)
      AND (p_SearchValue IS NULL
        OR PostContent LIKE CONCAT('%', p_SearchValue, '%')
        OR PostTitle LIKE CONCAT('%', p_SearchValue, '%'));
END //

CREATE PROCEDURE spPosts_Upsert(
    IN p_UserId INT,
    IN p_PostTitle VARCHAR(255),
    IN p_PostContent TEXT,
    IN p_PostId INT
)
BEGIN
    IF NOT EXISTS (SELECT * FROM Posts WHERE PostId = p_PostId) THEN
        INSERT INTO Posts(UserId, PostTitle, PostContent, PostCreated, PostUpdated)
        VALUES (p_UserId, p_PostTitle, p_PostContent, NOW(), NOW());
    ELSE
        UPDATE Posts
        SET PostTitle = p_PostTitle,
            PostContent = p_PostContent,
            PostUpdated = NOW()
        WHERE PostId = p_PostId;
    END IF;
END //

CREATE PROCEDURE spPost_Delete(
    IN p_PostId INT,
    IN p_UserId INT
)
BEGIN
    DELETE FROM Posts
    WHERE PostId = p_PostId AND UserId = p_UserId;
END //

CREATE PROCEDURE spLoginConfirmation_Get(
    IN p_Email VARCHAR(50)
)
BEGIN
    SELECT PasswordHash, PasswordSalt
    FROM Auth
    WHERE Email = p_Email;
END //

CREATE PROCEDURE spRegistration_Upsert(
    IN p_Email VARCHAR(50),
    IN p_PasswordHash BLOB,
    IN p_PasswordSalt BLOB
)
BEGIN
    INSERT INTO Auth(Email, PasswordHash, PasswordSalt)
    VALUES (p_Email, p_PasswordHash, p_PasswordSalt)
    ON DUPLICATE KEY UPDATE
                         PasswordHash = p_PasswordHash,
                         PasswordSalt = p_PasswordSalt;
END //

DELIMITER ;