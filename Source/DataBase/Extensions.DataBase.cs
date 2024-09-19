using System.Text;

namespace MelisWeb.Common.DataBase;

public static class ExtensionsDataBase
{
    public static string Indentation = "\t";

    #region Table Extensions

    public static string GetSaveStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var columns = string.Join(", ", table.WritableColumns.Select(c => c.Name));
        var values = string.Join(", ", table.WritableColumns.Select(c => $"@{c.Name}"));
        var set = string.Join(",\n      ", table.UpdatableColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));

        var procedureName = $"{table.Schema}.ERP_{table.Name}_SAVE";
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine(Indentation + string.Join($",\n{Indentation}", table.Columns.Where(c => !c.IsIdentity || c.IsPrimaryKey).Select(c => $"@{c.Name} {c.GetDataType()}")));

        script.AppendLine("AS");
        script.AppendLine("BEGIN");

        script.AppendLine($"{Indentation}IF EXISTS (SELECT 1 FROM {table.Schema}.{table.Name} WHERE {where})");
        script.AppendLine($"{Indentation}BEGIN");
        script.AppendLine(GetUpdateScript(table, Indentation + Indentation));
        script.AppendLine($"{Indentation}END");
        script.AppendLine($"{Indentation}ELSE");
        script.AppendLine($"{Indentation}BEGIN");
        script.AppendLine(GetInsertScript(table, Indentation + Indentation));
        script.AppendLine($"{Indentation}END");
        script.AppendLine("END");

        return script.ToString();
    }

    public static string GetDeleteStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var procedureName = $"{table.Schema}.ERP_{table.Name}_DELETE";
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine(Indentation + string.Join($",\n{Indentation}", table.PrimaryKeyColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetDeleteScript(table, Indentation));
        script.AppendLine("END");
        return script.ToString();
    }

    public static string GetGetByKeyStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var procedureName = $"{table.Schema}.ERP_{table.Name}_GET_BY_KEYS";
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine(Indentation + string.Join($",\n{Indentation}", table.PrimaryKeyColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetByKeyScript(table, Indentation));
        script.AppendLine("END");
        return script.ToString();
    }

    public static string GetGetAllStoredProcedureScript(this Table table, Column[]? requiredColumns = null)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var procedureName = $"{table.Schema}.ERP_{table.Name}_GET_ALL";
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        if (requiredColumns != null)
            script.AppendLine(Indentation + string.Join($",\n{Indentation}", requiredColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetAllScript(table, Indentation, requiredColumns));
        script.AppendLine("END");
        return script.ToString();
    }

    #region Private Methods
    private static string GetDeleteScript(this Table table, string indentation = "")
    {
        if (table == null) return string.Empty;

        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        return $@"{indentation}DELETE FROM {table.Name} 
{indentation}WHERE {where}";
    }

    private static string GetInsertScript(this Table table, string indentation = "")
    {
        if (table == null) return string.Empty;

        var columns = string.Join(", ", table.WritableColumns.Select(c => c.Name));
        var values = string.Join(", ", table.WritableColumns.Select(c => $"@{c.Name}"));
        return $@"{indentation}INSERT INTO {table.Name} 
{indentation}    ({columns}) 
{indentation}VALUES 
{indentation}    ({values})";
    }

    private static string GetUpdateScript(this Table table, string indentation = "")
    {
        if (table == null) return string.Empty;

        var set = string.Join($",\n{indentation}      ", table.UpdatableColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        return $@"{indentation}UPDATE {table.Schema}.{table.Name} 
{indentation}  SET {set} 
{indentation}WHERE {where}";
    }

    private static string GetAllScript(this Table table, string indentation = "", Column[]? requiredColumns = null)
    {
        if (table == null) return string.Empty;

        var columns = string.Join($",\n{indentation}   ", table.Columns.Select(c => c.Name));
        var where = "";
        if (requiredColumns != null)
        {
            where = $"\n{indentation}WHERE ";
            where += string.Join($"\n{indentation}  AND ", requiredColumns.Select(c => $"{c.Name} = @{c.Name}"));
        }

        return $@"{indentation}SELECT {columns} 
{indentation}FROM {table.Name}{where}";
    }

    private static string GetByKeyScript(this Table table, string indentation = "")
    {
        if (table == null) return string.Empty;

        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var columns = string.Join($",\n{indentation}   ", table.Columns.Select(c => c.Name));
        return $@"{indentation}SELECT {columns}
{indentation}FROM {table.Name}
{indentation}WHERE {where}";
    }

    private static void GetStoredProcedureHeader(StringBuilder script, string databaseName, string procedureName)
    {
        script.AppendLine($"USE {databaseName}");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine($"IF OBJECT_ID  ('{procedureName}', 'P') IS NOT NULL");
        script.AppendLine($"{Indentation}SET NOEXEC ON;");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine($"PRINT 'First creation of {procedureName} procedure';");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine($"CREATE PROCEDURE {procedureName}");
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine($"{Indentation}PRINT 'Fake body of {procedureName} procedure'");
        script.AppendLine("END");
        script.AppendLine("GO");
        script.AppendLine();
        script.AppendLine("SET NOEXEC OFF;");
        script.AppendLine("GO");
        script.AppendLine();
    }

    #endregion

    #endregion
}
