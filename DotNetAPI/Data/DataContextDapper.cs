using System.Data;
using Dapper;
using MySqlConnector;

namespace DotNetAPI.Data;

internal class DataContextDapper(IConfiguration config)
{
    public IEnumerable<T> LoadData<T>(string sql)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Query<T>(sql);
    }

    public IEnumerable<T> LoadData<T, U>(string sql, U parameters)
    {
        using IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Query<T>(sql, parameters);
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

    public bool ExecuteSqlWithParameters(string sql, List<MySqlParameter> parameters)
    {
        using MySqlConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        using MySqlCommand commandWithParams = new MySqlCommand(sql, dbConnection);
        foreach (MySqlParameter parameter in parameters)
        {
            commandWithParams.Parameters.Add(parameter);
        }

        dbConnection.Open();
        int rowsAffected = commandWithParams.ExecuteNonQuery();
        return rowsAffected > 0;
    }
}