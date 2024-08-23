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

    public IEnumerable<T> LoadData<T, TU>(string sql, TU parameters)
    {
        using IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Query<T>(sql, parameters);
    }

    public T LoadDataSingle<T>(string sql)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.QuerySingle<T>(sql);
    }

    public T LoadDataSingle<T, TU>(string sql, TU parameters)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.QuerySingle<T>(sql, parameters);
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

    public bool ExecuteSqlWithParameters(string sql, DynamicParameters parameters)
    {
        using MySqlConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Execute(sql, parameters) > 0;
    }

    public bool ExecuteSqlWithParameters<T, TU>(string sql, TU parameters)
    {
        using MySqlConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Execute(sql, parameters) > 0;
    }
    
    public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters parameters)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.Query<T>(sql, parameters);
    }

    public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters parameters)
    {
        IDbConnection dbConnection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        return dbConnection.QuerySingle<T>(sql, parameters);
    }
}