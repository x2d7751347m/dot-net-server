using DotNetAPI.Data;
using DotNetAPI.Models;

namespace DotNetAPI.Data;

public class UserRepository(IConfiguration config) : IUserRepository
{
    DataContextEf _entityFramework = new(config);

    public bool SaveChanges()
    {
        return _entityFramework.SaveChanges() > 0;
    }

    // public bool AddEntity<T>(T entityToAdd)
    public void AddEntity<T>(T entityToAdd)
    {
        if (entityToAdd != null)
        {
            _entityFramework.Add(entityToAdd);
            // return true;
        }
        // return false;
    }

    // public bool AddEntity<T>(T entityToAdd)
    public void RemoveEntity<T>(T entityToAdd)
    {
        if (entityToAdd != null)
        {
            _entityFramework.Remove(entityToAdd);
            // return true;
        }
        // return false;
    }

    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _entityFramework.Users.ToList<User>();
        return users;
    }

    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework.Users
            .FirstOrDefault(u => u.UserId == userId);

        if (user != null)
        {
            return user;
        }
            
        throw new Exception("Failed to Get User");
    }

    public UserSalary GetSingleUserSalary(int userId)
    {
        UserSalary? userSalary = _entityFramework.UserSalary
            .FirstOrDefault(u => u.UserId == userId);

        if (userSalary != null)
        {
            return userSalary;
        }
            
        throw new Exception("Failed to Get User");
    }

    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = _entityFramework.UserJobInfo
            .FirstOrDefault(u => u.UserId == userId);

        if (userJobInfo != null)
        {
            return userJobInfo;
        }
            
        throw new Exception("Failed to Get User");
    }
}