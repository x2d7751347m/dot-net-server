using System.Data;
using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace DotNetAPI.Data;

class DataContextDapper(IConfiguration config)
{
    public IEnumerable<T> LoadData<T>(string sql)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Query<T>(sql);
    }

    public T LoadDataSingle<T>(string sql)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.QuerySingle<T>(sql);
    }

    public bool ExecuteSql(string sql)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Execute(sql) > 0;
    }

    public int ExecuteSqlWithRowCount(string sql)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Execute(sql);
    }
}