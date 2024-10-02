using System.Text;

namespace MelisWeb.Common.DataBase;

public static class ExtensionsDataBase
{

    public static string Indentation = "    ";
    private static string Indentation2 = Indentation + Indentation;
    private static string Indentation3 = Indentation2 + Indentation;
    private static string Indentation4 = Indentation3 + Indentation;
    private static string Indentation5 = Indentation4 + Indentation;
    private static string Indentation6 = Indentation5 + Indentation;
    private static string Indentation7 = Indentation6 + Indentation;
    private static string Indentation8 = Indentation7 + Indentation;

    #region Table Extensions

    public static string GetSaveCommandName(this Table table)
    {
        return table.GetCommandName("save");
    }

    public static string GetSaveStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var columns = string.Join(", ", table.WritableColumns.Select(c => c.Name));
        var values = string.Join(", ", table.WritableColumns.Select(c => $"@{c.Name}"));
        var set = string.Join(",\n      ", table.UpdatableColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));

        var procedureName = table.GetSaveCommandName();
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

    public static string GetDeleteCommandName(this Table table)
    {
        return table.GetCommandName("delete");
    }

    public static string GetDeleteStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var where = string.Join(" AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var procedureName = table.GetDeleteCommandName();
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine(Indentation + string.Join($",\n{Indentation}", table.PrimaryKeyColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetDeleteScript(table, Indentation));
        script.AppendLine("END");
        return script.ToString();
    }

    public static string GetByKeyCommandName(this Table table)
    {
        return table.GetCommandName("get_by_keys");
    }
    public static string GetByIdentityCommandName(this Table table)
    {
        return table.GetCommandName("get_by_identity");
    }

    public static string GetGetByIdentityStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;
        if (table.IdentityColumn == null) return string.Empty;

        var script = new StringBuilder();
        var procedureName = table.GetByIdentityCommandName();
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine(Indentation + $"@{table.IdentityColumn.Name} {table.IdentityColumn.GetDataType()}");
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetByIdentityScript(table, Indentation));
        script.AppendLine("END");
        return script.ToString();
    }
    public static string GetGetByKeyStoredProcedureScript(this Table table)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var procedureName = table.GetByKeyCommandName();
        GetStoredProcedureHeader(script, table.DatabaseName, procedureName);
        script.AppendLine($"ALTER PROCEDURE {procedureName}");
        script.AppendLine(Indentation + string.Join($",\n{Indentation}", table.PrimaryKeyColumns.Select(c => $"@{c.Name} {c.GetDataType()}")));
        script.AppendLine("AS");
        script.AppendLine("BEGIN");
        script.AppendLine(GetByKeyScript(table, Indentation));
        script.AppendLine("END");
        return script.ToString();
    }

    public static string GetGetAllCommandName(this Table table)
    {
        return table.GetCommandName("get_all");
    }

    public static string GetGetAllStoredProcedureScript(this Table table, Column[]? requiredColumns = null)
    {
        if (table == null) return string.Empty;

        var script = new StringBuilder();
        var procedureName = table.GetGetAllCommandName();
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
        var result = new StringBuilder();
        var recVersion = table.UpdatableColumns.Where(c => c.Name.EndsWith("_RECVERSION")).FirstOrDefault();
        if (recVersion != null)
        {
            result.AppendLine($"{indentation}SET @{recVersion.Name} = COALESCE(@{recVersion.Name}, 0) + 1");
            result.AppendLine();
        }
        var set = string.Join($",\n{indentation}      ", table.UpdatableColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        result.AppendLine($@"{indentation}UPDATE {table.Schema}.{table.Name} 
{indentation}  SET {set} 
{indentation}WHERE {where}");
        return result.ToString();
    }

    private static string GetAllScript(this Table table, string indentation = "", Column[]? requiredColumns = null)
    {
        if (table == null) return string.Empty;

        var columns = string.Join($",\n{indentation}   ", table.Columns.Select(c => $"{c.Name} [{c.Name.ToPascalCase()}]"));
        var where = "";
        if (requiredColumns != null && requiredColumns.Length > 0)
        {
            where = $"\n{indentation}WHERE ";
            where += string.Join($"\n{indentation}  AND ", requiredColumns.Select(c => $"{c.Name} = @{c.Name}"));
        }

        return $@"{indentation}SELECT {columns} 
{indentation}FROM {table.Name}{where}";
    }

    private static string GetByIdentityScript(this Table table, string indentation = "")
    {
        if (table == null) return string.Empty;
        if (table.IdentityColumn == null) return string.Empty;

        var where = $"{table.IdentityColumn.Name} = @{table.IdentityColumn.Name}";
        var columns = string.Join($",\n{indentation}   ", table.Columns.Select(c => $"{c.Name} [{c.Name.ToPascalCase()}]"));
        return $@"{indentation}SELECT {columns}
{indentation}FROM {table.Name}
{indentation}WHERE {where}";
    }

    private static string GetByKeyScript(this Table table, string indentation = "")
    {
        if (table == null) return string.Empty;

        var where = string.Join($"\n{indentation}  AND ", table.PrimaryKeyColumns.Select(c => $"{c.Name} = @{c.Name}"));
        var columns = string.Join($",\n{indentation}   ", table.Columns.Select(c => $"{c.Name} [{c.Name.ToPascalCase()}]"));
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

    private static string GetCommandName(this Table table, string action)
    {
        return $"{table.Schema}.ERP_{table.Name}_{action.ToUpper()}";
    }

    #endregion

    #endregion
}
