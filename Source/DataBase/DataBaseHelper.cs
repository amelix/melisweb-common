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
    public List<Table> GetTables()
    {
        var tables = new List<Table>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT 
	DB_NAME(), OBJECT_SCHEMA_NAME(t.object_id), t.name
FROM
	sys.tables t
WHERE
	t.type = 'U'
	AND t.name NOT IN ('sysdiagrams')
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

    public string GetDeleteScript(Table table, string indentation = "")
    {
        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        return $@"{indentation}DELETE FROM {table.Name} 
{indentation}WHERE {where}";
    }

    public string GetInsertScript(Table table, string indentation = "")
    {
        var columns = string.Join(", ", table.WritableColumns.Select(c => c.Name));
        var values = string.Join(", ", table.WritableColumns.Select(c => $"@{c.Name}"));
        return $@"{indentation}INSERT INTO {table.Name} 
{indentation}    ({columns}) 
{indentation}VALUES 
{indentation}    ({values})";
    }

    public string GetUpdateScript(Table table, string indentation = "")
    {
        var set = string.Join($",\n{indentation}      ", table.UpdatableColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        return $@"{indentation}UPDATE {table.Schema}.{table.Name} 
{indentation}  SET {set} 
{indentation}WHERE {where}";
    }

    public string GetAllScript(Table table, string indentation = "")
    {
        var columns = string.Join(",\n{indentation}   ", table.Columns.Select(c => c.Name));
        return $@"{indentation}SELECT {columns} 
{indentation}FROM {table.Name}";
    }

    public string GetByKeyScript(Table table, string indentation = "")
    {
        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var columns = string.Join($",\n{indentation}   ", table.Columns.Select(c => c.Name));
        return $@"{indentation}SELECT {columns}
{indentation}FROM {table.Name}
{indentation}WHERE {where}";
    }

    public string GetGetByKeyStoredProcedureScript(Table table)
    {
        var script = new StringBuilder();
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var procedureName = $"{table.Schema}.ERP_{table.Name}_GET_BY_KEYS";
        NewMethod(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine("\t" + string.Join(",\n\t", table.PrimaryKeyColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetByKeyScript(table, "\t"));
        script.AppendLine("END");
        return script.ToString();
    }

    public string GetDeleteStoredProcedureScript(Table table)
    {
        var script = new StringBuilder();
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var procedureName = $"{table.Schema}.ERP_{table.Name}_DELETE";
        NewMethod(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine("\t" + string.Join(",\n\t", table.PrimaryKeyColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetDeleteScript(table, "\t"));
        script.AppendLine("END");
        return script.ToString();
    }

    public string GetSaveStoredProcedureScript(Table table)
    {
        var script = new StringBuilder();
        var columns = string.Join(", ", table.WritableColumns.Select(c => c.Name));
        var values = string.Join(", ", table.WritableColumns.Select(c => $"@{c.Name}"));
        var set = string.Join(",\n      ", table.UpdatableColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));

        var procedureName = $"{table.Schema}.ERP_{table.Name}_SAVE";
        NewMethod(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine("\t" + string.Join(",\n\t", table.Columns.Where(c => !c.IsIdentity || c.IsPrimaryKey).Select(c => $"@{c.Name} {c.GetDataType()}")));

        script.AppendLine("AS");
        script.AppendLine("BEGIN");

        script.AppendLine($"\tIF EXISTS (SELECT 1 FROM {table.Schema}.{table.Name} WHERE {where})");
        script.AppendLine("\tBEGIN");
        script.AppendLine(GetUpdateScript(table, "\t\t"));
        script.AppendLine("\tEND");
        script.AppendLine("\tELSE");
        script.AppendLine("\tBEGIN");
        script.AppendLine(GetInsertScript(table, "\t\t"));
        script.AppendLine("\tEND");
        script.AppendLine("END");

        return script.ToString();
    }

    private static void NewMethod(StringBuilder script, string databaseName, string procedureName)
    {
        script.AppendLine($"USE {databaseName}");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine($"IF OBJECT_ID  ('{procedureName}', 'P') IS NOT NULL");
        script.AppendLine("\tSET NOEXEC ON;");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine($"PRINT 'First creation of {procedureName} procedure';");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine($"CREATE PROCEDURE {procedureName}");
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine($"\tPRINT 'Fake body of {procedureName} procedure'");
        script.AppendLine("END");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine("SET NOEXEC OFF;");
        script.AppendLine("GO");
        script.AppendLine();
    }
}
