DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
                       UserId INT AUTO_INCREMENT PRIMARY KEY,
                       FirstName VARCHAR(50),
                       LastName VARCHAR(50),
                       Email VARCHAR(50) UNIQUE,
                       Gender VARCHAR(50),
                       Active BOOLEAN
);

DROP TABLE IF EXISTS UserSalary;

CREATE TABLE UserSalary (
                            UserId INT UNIQUE,
                            Salary DECIMAL(18, 4)
);

DROP TABLE IF EXISTS UserJobInfo;

CREATE TABLE UserJobInfo (
                             UserId INT UNIQUE,
                             JobTitle VARCHAR(50),
                             Department VARCHAR(50)
);

-- USE dot_net_api_database;

-- SELECT UserId
--     , FirstName
--     , LastName
--     , Email
--     , Gender
--     , Active
-- FROM Users;

-- SELECT UserId
--     , Salary
-- FROM UserSalary;

-- SELECT UserId
--     , JobTitle
--     , Department
-- FROM UserJobInfo;