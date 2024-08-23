using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IConfiguration config) : ControllerBase
{
    DataContextDapper _dapper = new(config);

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT NOW()");
    }

    [HttpGet("Users")]
    public IEnumerable<User> GetUsers()
    {
        string sql = @"SELECT UserId,
                FirstName,
                LastName,
                Email,
                Gender,
                Active 
            FROM Users";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

    [HttpGet("User/{userId:int}")]
    public User GetSingleUser(int userId)
    {
        string sql = @"
            SELECT UserId,
                FirstName,
                LastName,
                Email,
                Gender,
                Active 
            FROM Users
            WHERE UserId = " + userId.ToString(); //"7"
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }
    
    [HttpPut("User")]
    public IActionResult EditUser(User user)
    {
        string sql = @"
            UPDATE Users
            SET FirstName = '" + user.FirstName + 
                         "', LastName = '" + user.LastName +
                         "', Email = '" + user.Email + 
                         "', Gender = '" + user.Gender + 
                         "', Active = '" + user.Active + 
                         "' WHERE UserId = " + user.UserId;
        
        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        } 

        throw new Exception("Failed to Update User");
    }


    [HttpPost("User")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"
            INSERT INTO Users(
                FirstName,
                LastName,
                Email,
                Gender,
                Active
            ) VALUES (
                '"" + user.FirstName + 
                ""', '"" + user.LastName +
                ""', '"" + user.Email + 
                ""', '"" + user.Gender + 
                ""', '"" + user.Active + 
            ""')";
        
        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        } 

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("User/{userId:int}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
            DELETE FROM Users 
            WHERE UserId = " + userId.ToString();
        
        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        } 

        throw new Exception("Failed to Delete User");
    }
    
    
    [HttpGet("UserSalary/{userId:int}")]
    public IEnumerable<UserSalary> GetUserSalary(int userId)
    {
        return _dapper.LoadData<UserSalary>(@"
        SELECT UserSalary.UserId
                , UserSalary.Salary
        FROM  UserSalary
            WHERE UserId = " + userId.ToString());
    }

    [HttpPost("UserSalary")]
    public IActionResult PostUserSalary(UserSalary userSalaryForInsert)
    {
        string sql = @"
        INSERT INTO UserSalary (
            UserId,
            Salary
        ) VALUES (" + userSalaryForInsert.UserId.ToString()
                    + ", " + userSalaryForInsert.Salary
                    + ")";

        if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
        {
            return Ok(userSalaryForInsert);
        }
        throw new Exception("Adding User Salary failed on save");
    }

    [HttpPut("UserSalary")]
    public IActionResult PutUserSalary(UserSalary userSalaryForUpdate)
    {
        string sql = "UPDATE UserSalary SET Salary=" 
                     + userSalaryForUpdate.Salary
                     + " WHERE UserId=" + userSalaryForUpdate.UserId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userSalaryForUpdate);
        }
        throw new Exception("Updating User Salary failed on save");
    }

    [HttpDelete("UserSalary/{userId:int}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        string sql = "DELETE FROM UserSalary WHERE UserId=" + userId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Deleting User Salary failed on save");
    }
    
    [HttpGet("UserJobInfo/{userId:int}")]
    public IEnumerable<UserJobInfo> GetUserJobInfo(int userId)
    {
        return _dapper.LoadData<UserJobInfo>(@"
        SELECT  UserJobInfo.UserId
                , UserJobInfo.JobTitle
                , UserJobInfo.Department
        FROM  UserJobInfo
            WHERE UserId = " + userId.ToString());
    }

    [HttpPost("UserJobInfo")]
    public IActionResult PostUserJobInfo(UserJobInfo userJobInfoForInsert)
    {
        string sql = @"
        INSERT INTO UserJobInfo (
            UserId,
            Department,
            JobTitle
        ) VALUES (" + userJobInfoForInsert.UserId
                    + ", '" + userJobInfoForInsert.Department
                    + "', '" + userJobInfoForInsert.JobTitle
                    + "')";

        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userJobInfoForInsert);
        }
        throw new Exception("Adding User Job Info failed on save");
    }

    [HttpPut("UserJobInfo")]
    public IActionResult PutUserJobInfo(UserJobInfo userJobInfoForUpdate)
    {
        string sql = "UPDATE UserJobInfo SET Department='" 
                     + userJobInfoForUpdate.Department
                     + "', JobTitle='"
                     + userJobInfoForUpdate.JobTitle
                     + "' WHERE UserId=" + userJobInfoForUpdate.UserId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userJobInfoForUpdate);
        }
        throw new Exception("Updating User Job Info failed on save");
    }

    [HttpDelete("UserJobInfo/{userId:int}")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        string sql = @"
        DELETE FROM UserJobInfo 
            WHERE UserId = " + userId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        } 

        throw new Exception("Failed to Delete User");
    }
}