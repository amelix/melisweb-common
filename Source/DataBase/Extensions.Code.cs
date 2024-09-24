using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase;

public static class ExtensionsCode
{
    public static string Indentation = "    ";
    private static string Indentation2 = Indentation + Indentation;
    private static string Indentation3 = Indentation2 + Indentation;
    private static string Indentation4 = Indentation3 + Indentation;
    private static string Indentation5 = Indentation4 + Indentation;
    private static string Indentation6 = Indentation5 + Indentation;
    private static string Indentation7 = Indentation6 + Indentation;
    private static string Indentation8 = Indentation7 + Indentation;

    #region C# Extensions

    public static string GetModelClass(this Table table,
        string classNamespace,
        string? className = null,
        string? inheritedClass = null,
        IEnumerable<string>? usingList = null)
    {
        var result = new StringBuilder();
        if (className == null)
        {
            className = table.Name.ToPascalCase().ToSingular();
        }
        if (usingList != null)
        {
            foreach (var item in usingList)
            {
                result.AppendLine($"using {item};");
            }
        }
        //result.AppendLine($"using System;");
        //result.AppendLine($"using System.Collections.Generic;");
        result.AppendLine();
        result.AppendLine($"namespace {classNamespace}");
        result.AppendLine("{");
        if (inheritedClass != null)
        {
            result.AppendLine($"{Indentation}public partial class {className} : {inheritedClass}");
        }
        else
        {
            result.AppendLine($"{Indentation}public partial class {className}");
        }

        result.AppendLine($"{Indentation}{{");
        result.AppendLine($"{Indentation + Indentation}#region Properties");
        result.AppendLine();
        result.Append(table.GetProperties(Indentation + Indentation));
        result.AppendLine($"{Indentation + Indentation}#region CRUD commands");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}public static string SaveCommandName => \"{table.GetSaveCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string DeleteCommandName => \"{table.GetDeleteCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string GetByKeyCommandName => \"{table.GetByKeyCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string GetAllCommandName => \"{table.GetGetAllCommandName()}\";");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}#endregion");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}#endregion");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine("}");
        return result.ToString();
    }

    public static string GetRepositoryClass(this Table table,
        string classNamespace,
        string? className = null,
        string? inheritedClass = null,
        IEnumerable<string>? usingList = null)
    {
        var result = new StringBuilder();
        var entityName = table.Name.ToPascalCase().ToSingular();
        if (className == null)
        {
            className = entityName;
        }
        if (usingList != null)
        {
            foreach (var item in usingList)
            {
                result.AppendLine($"using {item};");
            }
        }

        result.AppendLine();
        result.AppendLine($"namespace {classNamespace}");
        result.AppendLine("{");
        if (inheritedClass != null)
        {
            result.AppendLine($"{Indentation}public partial class {className} : {inheritedClass}");
        }
        else
        {
            result.AppendLine($"{Indentation}public partial class {className}");
        }
        result.AppendLine($"{Indentation}{{");

        #region GetAll
        var requiredColumns = table.Columns.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
        var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
        if (parameters.Count == 0)
        {
            result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{entityName}>> GetAll(params ParamStruct[] paramStructs)");
        }
        else
        {
            result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{entityName}>> GetAll({string.Join(", ", parameters)}, params ParamStruct[] paramStructs)");
        }

        result.AppendLine($"{Indentation2}{{");
        result.AppendLine($"{Indentation3}var start = Model.{entityName}.GetAllCommandName;");
        result.AppendLine($"{Indentation3}var command = Model.{entityName}.GetAllCommandName;");
        result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
        foreach (var column in requiredColumns)
        {
            result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToPascalCase()});");
        }
        result.AppendLine($"{Indentation3}foreach (var paramStruct in paramStructs)");
        result.AppendLine($"{Indentation3}{{");
        result.AppendLine($"{Indentation4}parameters.Add(paramStruct);");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation3}return await ExecuteReaderAsync<Model.{entityName}>(");
        result.AppendLine($"{Indentation4}command: command,");
        result.AppendLine($"{Indentation4}parameters: () => parameters,");
        result.AppendLine($"{Indentation4}commandType: System.Data.CommandType.StoredProcedure);");
        result.AppendLine($"{Indentation2}}}");

        result.AppendLine($"{Indentation}}}");
        #endregion

        result.AppendLine("}");

        return result.ToString();
    }

    public static string GetProviderClass(this Table table,
        string classNamespace,
        string? className = null,
        string? classNameRepository = null,
        string? inheritedClass = null,
        IEnumerable<string>? usingList = null)
    {
        var result = new StringBuilder();
        if (className == null)
        {
            className = table.Name.ToPascalCase().ToSingular();
        }
        if (classNameRepository == null)
        {
            classNameRepository = $"{className}Repository";
        }
        if (usingList != null)
        {
            foreach (var item in usingList)
            {
                result.AppendLine($"using {item};");
            }
        }

        result.AppendLine();
        result.AppendLine($"namespace {classNamespace}");
        result.AppendLine("{");
        if (inheritedClass != null)
        {
            result.AppendLine($"{Indentation}public partial class {className} : {inheritedClass}");
        }
        else
        {
            result.AppendLine($"{Indentation}public partial class {className}");
        }
        result.AppendLine($"{Indentation}{{");
        result.AppendLine($"{Indentation + Indentation}#region Properties");
        result.AppendLine();
        result.AppendLine($"{Indentation2}internal ILogger<{className}> logger {{ get; }}");
        result.AppendLine($"{Indentation2}internal {classNameRepository} repository {{ get; }}");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}#endregion");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}#region Constructor");
        result.AppendLine();
        result.AppendLine($"{Indentation2}public {className}(ILogger<{className}> logger, {classNameRepository} repository)");
        result.AppendLine($"{Indentation2}{{");
        result.AppendLine($"{Indentation3}this.logger = logger;");
        result.AppendLine($"{Indentation3}this.repository = repository;");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#endregion");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#region CRUD commands");
        result.AppendLine();
        var requiredColumns = table.Columns.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
        var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
        result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{table.Name.ToPascalCase().ToSingular()}>> GetAll({string.Join(", ", parameters)})");
        result.AppendLine($"{Indentation2}{{");
        result.AppendLine($"{Indentation3}return await repository.GetAll({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#endregion");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine("}");

        return result.ToString();
    }

    #region Private Methods

    private static string GetProperties(this Table table, string indentation = "")
    {
        var result = new StringBuilder();
        foreach (var column in table.Columns)
        {
            result.AppendLine($"{indentation}[Column(\"{column.Name}\")]");
            result.AppendLine($"{indentation}public {column.GetCodeDataType()} {column.Name.ToPascalCase()} {{ get; set; }}");
            result.AppendLine();
        }

        return result.ToString();
    }

    #endregion

    #endregion
}
