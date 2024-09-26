using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        var entityNamePlural = table.Name.ToPascalCase();
        var entityNameSingular = table.Name.ToPascalCase().ToSingular();
        if (className == null)
        {
            className = entityNameSingular;
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
        {
            var requiredColumns = table.Columns.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            if (parameters.Count == 0)
            {
                result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{entityNameSingular}>> GetAll{entityNamePlural}(params ParamStruct[] paramStructs)");
            }
            else
            {
                result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{entityNameSingular}>> GetAll{entityNamePlural}({string.Join(", ", parameters)}, params ParamStruct[] paramStructs)");
            }

            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.GetAllCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in requiredColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToPascalCase()});");
            }
            result.AppendLine($"{Indentation3}foreach (var paramStruct in paramStructs)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}parameters.Add(paramStruct);");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine($"{Indentation3}return await ExecuteReaderAsync<Model.{entityNameSingular}>(");
            result.AppendLine($"{Indentation4}command: command,");
            result.AppendLine($"{Indentation4}parameters: () => parameters,");
            result.AppendLine($"{Indentation4}commandType: System.Data.CommandType.StoredProcedure);");
            result.AppendLine($"{Indentation2}}}");
        }
        #endregion
        result.AppendLine();
        #region GetByKey
        {
            var requiredColumns = table.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            if (parameters.Count == 0)
            {
                result.AppendLine($"{Indentation2}public async Task<Model.{entityNameSingular}> Get{entityNameSingular}ByKey(params ParamStruct[] paramStructs)");
            }
            else
            {
                result.AppendLine($"{Indentation2}public async Task<Model.{entityNameSingular}> Get{entityNameSingular}ByKey({string.Join(", ", parameters)}, params ParamStruct[] paramStructs)");
            }
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.GetByKeyCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in requiredColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToPascalCase()});");
            }
            result.AppendLine($"{Indentation3}foreach (var paramStruct in paramStructs)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}parameters.Add(paramStruct);");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine($"{Indentation3}var result = await ExecuteReaderAsync<Model.{entityNameSingular}>(");
            result.AppendLine($"{Indentation4}command: command,");
            result.AppendLine($"{Indentation4}parameters: () => parameters,");
            result.AppendLine($"{Indentation4}commandType: System.Data.CommandType.StoredProcedure);");
            result.AppendLine($"{Indentation3}return result.FirstOrDefault();");
            result.AppendLine($"{Indentation2}}}");
        }
        #endregion
        result.AppendLine();
        #region Save
        {
            var parameters = table.WritableColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<bool> Save{entityNameSingular}({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.SaveCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in table.WritableColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToPascalCase()});");
            }
            result.AppendLine($"{Indentation3}return await ExecuteAsync<Model.{entityNameSingular}>(");
            result.AppendLine($"{Indentation4}command: command,");
            result.AppendLine($"{Indentation4}parameters: () => parameters,");
            result.AppendLine($"{Indentation4}commandType: System.Data.CommandType.StoredProcedure);");
            result.AppendLine($"{Indentation2}}}");
        }
        #endregion
        result.AppendLine();
        #region Delete
        {
            var requiredColumns = table.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<bool> Delete{entityNameSingular}({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.DeleteCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in requiredColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToPascalCase()});");
            }
            result.AppendLine($"{Indentation3}return await ExecuteAsync<Model.{entityNameSingular}>(");
            result.AppendLine($"{Indentation4}command: command,");
            result.AppendLine($"{Indentation4}parameters: () => parameters,");
            result.AppendLine($"{Indentation4}commandType: System.Data.CommandType.StoredProcedure);");
            result.AppendLine($"{Indentation2}}}");
        }
        #endregion

        result.AppendLine($"{Indentation}}}");
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
        var entityNamePlural = table.Name.ToPascalCase();
        var entityNameSingular = table.Name.ToPascalCase().ToSingular();

        if (className == null)
        {
            className = entityNameSingular;
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
        #region GetAll
        {
            var requiredColumns = table.Columns.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{entityNameSingular}>> GetAll({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.GetAll{entityNamePlural}({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region GetByKey
        {
            var requiredColumns = table.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<Model.{entityNameSingular}> GetByKey({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.Get{entityNameSingular}ByKey({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region Save
        {
            var parameters = table.WritableColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<bool> Save({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.Save{entityNameSingular}({string.Join(", ", table.WritableColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region Delete
        {
            var requiredColumns = table.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<bool> Delete({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.Delete{entityNameSingular}({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        result.AppendLine($"{Indentation2}#endregion");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine("}");

        return result.ToString();
    }

    public static string GetControllerClass(this Table table,
        string classNamespace,
        string? className = null,
        string? classNameRepository = null,
        string? classNameProvider = null,
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
        if (classNameProvider == null)
        {
            classNameProvider = $"{className}Provider";
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
        if (inheritedClass == null)
        {
            result.AppendLine($"{Indentation}public partial class {className} : Controller");
        }
        else if (string.IsNullOrWhiteSpace(inheritedClass))
        {
            result.AppendLine($"{Indentation}public partial class {className}");
        }
        else
        {
            result.AppendLine($"{Indentation}public partial class {className} : {inheritedClass}");
        }
        result.AppendLine($"{Indentation}{{");
        result.AppendLine($"{Indentation2}#region Properties");
        result.AppendLine();
        result.AppendLine($"{Indentation2}internal {classNameRepository} repository {{ get; }}");
        result.AppendLine($"{Indentation2}internal {classNameProvider} provider {{ get; }}");
        result.AppendLine($"{Indentation2}internal ILogger<{className}> logger {{ get; }}");
        result.AppendLine($"{Indentation2}internal IResourceProvider resourceProvider {{ get; }}");
        result.AppendLine($"{Indentation2}internal IRoleProvider roleProvider {{ get; }}");
        result.AppendLine($"{Indentation2}internal IToastNotification toastNotification {{ get; }}");

        result.AppendLine();
        result.AppendLine($"{Indentation2}#endregion");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#region Constructor");
        result.AppendLine();
        result.AppendLine($"{Indentation2}public {className}(");
        result.AppendLine($"{Indentation3}ILogger<{className}> logger,");
        result.AppendLine($"{Indentation3}IRoleProvider roleProvider,");
        result.AppendLine($"{Indentation3}IResourceProvider resourceProvider,");
        result.AppendLine($"{Indentation3}IToastNotification toastNotification,");
        result.AppendLine($"{Indentation3}{classNameRepository} repository,");
        result.AppendLine($"{Indentation3}{classNameProvider} provider");
        result.AppendLine($"{Indentation3})");
        result.AppendLine($"{Indentation2}{{");
        result.AppendLine($"{Indentation3}this.logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        result.AppendLine($"{Indentation3}this.roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));");
        result.AppendLine($"{Indentation3}this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));");
        result.AppendLine($"{Indentation3}this.toastNotification = toastNotification ?? throw new ArgumentNullException(nameof(toastNotification));");
        result.AppendLine($"{Indentation3}this.repository = repository;");
        result.AppendLine($"{Indentation3}this.provider = provider;");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#endregion");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#region CRUD commands");
        result.AppendLine();
        #region GetAll
        {
            var requiredColumns = table.Columns.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> GetAll({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var model = await this.provider.GetAll({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ data = model }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region GetByKey
        {
            var requiredColumns = table.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> GetByKey({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var model = await this.provider.GetByKey({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ data = model }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region Save
        {
            var parameters = table.WritableColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpPost]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> Save({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var result = await this.provider.Save({string.Join(", ", table.WritableColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ success = result }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region Delete
        {
            var requiredColumns = table.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpPost]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> Delete({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var result = await this.provider.Delete({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ success = result }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
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
