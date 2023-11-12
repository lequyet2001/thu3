using System.Data;
using System.Data.SqlClient;
using Dapper;
using thu3.Model;
using Thu6.model;

public class DataAccess
{
    private readonly string _connectionString;

    public DataAccess(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("db");
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public T ExecuteScalar<T>(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.ExecuteScalar<T>(query, parameters);
        }
    }

    public int Execute(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.Execute(query, parameters);
        }
    }

    public IEnumerable<T> Query<T>(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.Query<T>(query, parameters);
        }
    }
    public List<T> QueryList<T>(string query, object parameters = null)
    {
        using (var connection = CreateConnection())
        {
            return connection.Query<T>(query, parameters).ToList();
        }
    }

    public IEnumerable<SignupModel> GetUsers()
    {
        var query = "SELECT * FROM users";
        return Query<SignupModel>(query);
    }
    public int AddUser(Users user)
    {
        var query = "INSERT INTO users ( id,password, email,active) VALUES (@id, @password, @email,0)";
        return Execute(query, user);
    }
    // Các phương thức thực hiện các tác vụ thêm, sửa, xóa dữ liệu khác...

    // Ví dụ phương thức lấy danh sách người dùng


}
