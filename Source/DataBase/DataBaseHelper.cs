using System.Data.SqlClient;
using System.Text;

namespace MelisWeb.Common.DataBase;

public class DataBaseHelper
{
    private string _connectionString;
    public static DataBaseHelper Instance { get; } = new DataBaseHelper();

    public void Initialize(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Get the tables of a database
    /// </summary>
    /// <returns></returns>
    public List<Table> GetTables(List<string> requiredTables = null,
        List<string> tablesToIgnore = null)
    {
        var tables = new List<Table>();
        var where = "";
        if (requiredTables != null)
        {
            where += $"\n\tAND t.name IN ('{string.Join("','", requiredTables)}')";            
        }
        if (tablesToIgnore == null)
        {
            tablesToIgnore = new List<string>();
        }
        tablesToIgnore.Add("sysdiagrams");
        where += $"\n\tAND t.name NOT IN ('{string.Join("','", tablesToIgnore)}')";
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @$"
SELECT 
	DB_NAME(), OBJECT_SCHEMA_NAME(t.object_id), t.name
FROM
	sys.tables t
WHERE
	t.type = 'U'{where}
ORDER BY
	t.name";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(new Table
                        {
                            DatabaseName = reader.GetString(0),
                            Schema = reader.GetString(1),
                            Name = reader.GetString(2),
                            Columns = GetTableColumns(reader.GetString(2))
                        });
                    }
                }
            }
        }

        return tables;
    }

    /// <summary>
    /// Get the columns of a table
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public List<Column> GetTableColumns(string tableName)
    {
        var columns = new List<Column>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
		c.name Name
		,t.Name DataType
		,c.max_length MaxLength
		,c.precision Precision
		,c.scale Scale
		,c.is_nullable IsNullable
		,ISNULL(i.is_primary_key, 0) PrimaryKey
		,c.column_id [Index]
		,c.is_identity IsIdentity
	FROM 
		sys.columns c
	INNER JOIN 
		sys.types t ON c.user_type_id = t.user_type_id
	LEFT OUTER JOIN 
		sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
	LEFT OUTER JOIN 
		sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND i.is_primary_key != 0
	WHERE
		c.object_id = OBJECT_ID(@table_name)
	ORDER BY c.column_id";
                command.Parameters.AddWithValue("@table_name", tableName);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new Column
                        {
                            ColumnName = reader.GetString(0),
                            Name = reader.GetString(0),
                            Type = reader.GetString(1),
                            MaxLength = reader.GetInt16(2),
                            Precision = reader.GetByte(3),
                            Scale = reader.GetByte(4),
                            IsNullable = reader.GetBoolean(5),
                            IsPrimaryKey = reader.GetBoolean(6),
                            Index = reader.GetInt32(7),
                            IsIdentity = reader.GetBoolean(8)
                        });
                    }
                }
            }
        }

        return columns;
    }


}
