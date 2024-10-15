using System;
using System.Data.Common;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

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

    public static string GetModelClass(this Model model,
        string classNamespace,
        string? className = null,
        string? inheritedClass = null,
        IEnumerable<string>? usingList = null)
    {
        var result = new StringBuilder();
        if (className == null)
        {
            className = model.Name.ToPascalCase().ToSingular();
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
        result.Append(model.GetProperties(Indentation + Indentation));
        result.AppendLine($"{Indentation + Indentation}#region CRUD commands");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}public static string SaveCommandName => \"{model.Table.GetSaveCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string DeleteCommandName => \"{model.Table.GetDeleteCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string GetByKeyCommandName => \"{model.Table.GetByKeyCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string GetByIdentityCommandName => \"{model.Table.GetByIdentityCommandName()}\";");
        result.AppendLine($"{Indentation + Indentation}public static string GetAllCommandName => \"{model.Table.GetGetAllCommandName()}\";");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}#endregion");
        result.AppendLine();
        result.AppendLine($"{Indentation + Indentation}#endregion");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine("}");
        return result.ToString();
    }

    public static string GetRepositoryClass(this Model table,
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
            var requiredColumns = table.Properties.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
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
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToCamelCase()});");
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
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
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
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToCamelCase()});");
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
        #region GetByIdentity
        {
            var requiredColumns = new Column[] { table.IdentityColumn };
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            if (parameters.Count == 0)
            {
                result.AppendLine($"{Indentation2}public async Task<Model.{entityNameSingular}> Get{entityNameSingular}ByIdentity(params ParamStruct[] paramStructs)");
            }
            else
            {
                result.AppendLine($"{Indentation2}public async Task<Model.{entityNameSingular}> Get{entityNameSingular}ByIdentity({string.Join(", ", parameters)}, params ParamStruct[] paramStructs)");
            }
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.GetByIdentityCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in requiredColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToCamelCase()});");
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
            var parameters = table.WritableColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<bool> Save{entityNameSingular}({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.SaveCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in table.WritableColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToCamelCase()});");
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
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<bool> Delete{entityNameSingular}({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var command = Model.{entityNameSingular}.DeleteCommandName;");
            result.AppendLine($"{Indentation3}var parameters = new DynamicParameters();");
            foreach (var column in requiredColumns)
            {
                result.AppendLine($"{Indentation3}parameters.Add(\"{column.Name}\", {column.Name.ToCamelCase()});");
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

    public static string GetProviderClass(this Model table,
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
            var requiredColumns = table.Properties.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<IEnumerable<Model.{entityNameSingular}>> GetAll({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.GetAll{entityNamePlural}({string.Join(", ", requiredColumns.Select(c => c.Name.ToCamelCase()))});");
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
        #region GetByIdentity
        {
            var requiredColumns = new Column[] { table.IdentityColumn };
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}public async Task<Model.{entityNameSingular}> GetByIdentity({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.Get{entityNameSingular}ByIdentity({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
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

            result.AppendLine($"{Indentation2}/// <summary>");
            result.AppendLine($"{Indentation2}/// Saves the specified {entityNameSingular} model.");
            result.AppendLine($"{Indentation2}/// </summary>");
            result.AppendLine($"{Indentation2}/// <param name=\"model\">The {entityNameSingular} model to save.</param>");
            result.AppendLine($"{Indentation2}/// <returns>A task that represents the asynchronous save operation. The task result contains a boolean indicating success or failure.</returns>");
            result.AppendLine($"{Indentation2}public async Task<bool> Save(Model.{entityNameSingular} model)");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}return await repository.Save{entityNameSingular}(model.{string.Join(", model.", table.WritableColumns.Select(c => c.Name.ToPascalCase()))});");
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

    public static string GetControllerClass(this Model model,
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
            className = model.Name.ToPascalCase().ToSingular();
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
        result.AppendLine($"{Indentation}[Area(\"Erp\")]");
        result.AppendLine($"{Indentation}[ServiceFilter(typeof(Authorization.RAMSAuthorizeValidator))]");
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
        result.AppendLine($"{Indentation2}internal string userName => this.User.Identity.Name;");
        result.AppendLine($"{Indentation2}internal string companyId => this.User.Identity.GetCompanyId();");
        result.AppendLine($"{Indentation2}internal string erpCode => User.Identity.GetEntityId();");
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
        result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage, RmsCommon.Enums.RMSOperation.ErpView)]");
        result.AppendLine($"{Indentation2}public async Task<IActionResult> IndexAsync()");
        result.AppendLine($"{Indentation2}{{");
        result.AppendLine($"{Indentation3}ViewData[RmsCommon.Costants.ViewDataCostants.CanManage] = await this.roleProvider.CanPerformOperationAny(");
        result.AppendLine($"{Indentation4}this.userName,");
        result.AppendLine($"{Indentation4}this.companyId,");
        result.AppendLine($"{Indentation4}RmsCommon.Enums.RMSOperation.All,");
        result.AppendLine($"{Indentation4}RmsCommon.Enums.RMSOperation.ErpManage);");
        result.AppendLine();


        result.AppendLine($"{Indentation3}return View();");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}#region CRUD commands");
        result.AppendLine();
        #region GetAll
        {
            var requiredColumns = model.Properties.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> GetAll({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var model = await this.provider.GetAll({string.Join(", ", requiredColumns.Select(c => c.Name.ToCamelCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ data = model }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            result.AppendLine($"{Indentation2}public async Task<IActionResult> LoadData()");
            result.AppendLine($"{Indentation2}{{");
            if (requiredColumns.Length == 0)
            {
                result.AppendLine($"{Indentation3}return await this.GetAll();");
            }
            else if (requiredColumns.Length == 1)
            {
                if (requiredColumns[0].Name.EndsWith("_COMPANY"))
                {
                    result.AppendLine($"{Indentation3}return await this.GetAll(this.companyId);");
                }
                else
                {
                    result.AppendLine($"{Indentation3}return await this.GetAll(this.erpCode);");
                }
            }
            else
            {
                if (requiredColumns[0].Name.EndsWith("_COMPANY"))
                {
                    result.AppendLine($"{Indentation3}return await this.GetAll(this.companyId, this.erpCode);");
                }
                else
                {
                    result.AppendLine($"{Indentation3}return await this.GetAll(this.erpCode, this.companyId);");
                }

            }
            //result.AppendLine($"{Indentation3}// return await this.GetAll({string.Join(", ", requiredColumns.Select(c => c.Name.ToCamelCase()))});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region GetByKey
        {
            var requiredColumns = model.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> GetByKey({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var model = await this.provider.GetByKey({string.Join(", ", requiredColumns.Select(c => c.Name.ToCamelCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ data = model }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region GetByIdentity
        {
            var requiredColumns = new Column[] { model.IdentityColumn };
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> GetByIdentity({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var model = await this.provider.GetByIdentity({string.Join(", ", requiredColumns.Select(c => c.Name.ToCamelCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ data = model }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            result.AppendLine($"{Indentation2}public async Task<IActionResult> AddEditData(int? code)");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}if (code.HasValue)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}var model = await this.provider.GetByIdentity(code.Value);");
            result.AppendLine($"{Indentation4}if (model != null)");
            result.AppendLine($"{Indentation4}{{");
            result.AppendLine($"{Indentation5}ViewData[RmsCommon.Costants.ViewDataCostants.NewRecord] = false;");
            result.AppendLine($"{Indentation5}return PartialView(\"DataPartial\", model);");
            result.AppendLine($"{Indentation4}}}");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine();
            var requiredColumns2 = model.Properties.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            result.AppendLine($"{Indentation3}var emptyModel = new Model.{model.Name.ToPascalCase().ToSingular()}");
            result.AppendLine($"{Indentation3}{{");
            foreach (var column in model.Properties)
            {

                if (column.Name.EndsWith("_COMPANY"))
                {
                    result.AppendLine($"{Indentation4}{column.Name.ToPascalCase()} = this.companyId,");
                }
                else if (column.Name.EndsWith("_ERP_CODE"))
                {
                    result.AppendLine($"{Indentation4}{column.Name.ToPascalCase()} = this.erpCode,");
                }
            }
            result.AppendLine($"{Indentation3}}};");
            result.AppendLine($"{Indentation3}ViewData[RmsCommon.Costants.ViewDataCostants.NewRecord] = true;");
            result.AppendLine($"{Indentation3}return PartialView(\"DataPartial\", emptyModel);");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region Save \ SaveData
        {
            var requiredColumns = model.PrimaryKeyColumns.ToArray();
            var parameters = model.WritableColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToCamelCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpPost]");
            result.AppendLine($"{Indentation2}public async Task<JsonResult> Save({string.Join(", ", parameters)})");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}var result = await this.provider.Save({string.Join(", ", model.WritableColumns.Select(c => c.Name.ToCamelCase()))});");
            result.AppendLine($"{Indentation3}return Json(new {{ success = result }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation2}[HttpPost]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            result.AppendLine($"{Indentation2}public async Task<IActionResult> SaveData(Model.{model.Name.ToPascalCase().ToSingular()} model)");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}if (ModelState.IsValid)");
            result.AppendLine($"{Indentation3}{{");
            if (model.IdentityColumn != null)
            {
                result.AppendLine($"{Indentation4}if (model.{model.IdentityColumn.Name.ToPascalCase()} == 0)");
                result.AppendLine($"{Indentation4}{{");
                result.AppendLine($"{Indentation5}var modelOld = await this.provider.GetByKey(model.{string.Join(", model.", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
                result.AppendLine($"{Indentation5}if (modelOld != null)");
                result.AppendLine($"{Indentation5}{{");
                result.AppendLine($"{Indentation6}this.toastNotification.AddErrorToastMessage(\"Data already exists.\");");
                result.AppendLine($"{Indentation6}return StatusCode(500, \"Data already exists.\");");
                result.AppendLine($"{Indentation5}}}");
                result.AppendLine();
                foreach (var column in model.Properties)
                {
                    if (!column.VisibleInCreate && !string.IsNullOrWhiteSpace(column.DefaultValue))
                    {
                        result.AppendLine($"{Indentation5}model.{column.Name.ToPascalCase()} = {column.DefaultValue};");
                    }
                }
                result.AppendLine($"{Indentation4}}}");
                result.AppendLine();
            }

            foreach (var column in model.Properties)
            {

                if (column.Name.EndsWith("_COMPANY"))
                {
                    result.AppendLine($"{Indentation4}model.{column.Name.ToPascalCase()} = this.companyId;");
                }
                else if (column.Name.EndsWith("_ERP_CODE"))
                {
                    result.AppendLine($"{Indentation4}model.{column.Name.ToPascalCase()} = this.erpCode;");
                }
            }
            result.AppendLine();
            result.AppendLine($"{Indentation4}if(await this.provider.Save({string.Join($", \r\n{Indentation5}", model.WritableColumns.Select(c => $"model.{c.Name.ToPascalCase()}"))}))");
            result.AppendLine($"{Indentation4}{{");
            result.AppendLine($"{Indentation5}this.toastNotification.AddSuccessToastMessage(\"Data saved successfully.\");");
            result.AppendLine($"{Indentation5}return Ok(new {{ status = \"success\", action = \"refresh\" }});");
            result.AppendLine($"{Indentation4}}}");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine($"{Indentation3}this.toastNotification.AddErrorToastMessage(\"Data not saved.\");");
            result.AppendLine($"{Indentation3}return StatusCode(500, \"Data not saved.\");");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        #region Delete
        {
            var requiredColumns = model.PrimaryKeyColumns.ToArray();
            var parameters = requiredColumns.Select(c => $"{c.GetCodeDataType()} {c.Name.ToPascalCase()}").ToList();
            result.AppendLine($"{Indentation2}[HttpDelete]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            if (model.IdentityColumn == null)
            {
                result.AppendLine($"{Indentation2}public async Task<IActionResult> Delete({string.Join(", ", parameters)})");
                result.AppendLine($"{Indentation2}{{");
                result.AppendLine($"{Indentation3}var result = await this.provider.Delete({string.Join(", ", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            }
            else
            {
                result.AppendLine($"{Indentation2}public async Task<IActionResult> Delete({model.IdentityColumn.GetCodeDataType()} code)");
                result.AppendLine($"{Indentation2}{{");
                result.AppendLine($"{Indentation3}var model = await this.provider.GetByIdentity(code);");
                result.AppendLine($"{Indentation3}if (model == null)");
                result.AppendLine($"{Indentation3}{{");
                result.AppendLine($"{Indentation4}this.toastNotification.AddErrorToastMessage(\"Data not found.\");");
                result.AppendLine($"{Indentation4}return StatusCode(500, \"Data not saved.\");");
                result.AppendLine($"{Indentation3}}}");
                result.AppendLine($"{Indentation3}var result = await this.provider.Delete(model.{string.Join(", model.", requiredColumns.Select(c => c.Name.ToPascalCase()))});");
            }
            result.AppendLine($"{Indentation3}if (!result)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}this.toastNotification.AddErrorToastMessage(\"Data not deleted.\");");
            result.AppendLine($"{Indentation4}return StatusCode(500, \"Data not deleted.\");");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine($"{Indentation3}return Ok(new {{ status = \"success\", action = \"refresh\" }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
        }
        #endregion
        result.AppendLine($"{Indentation2}#endregion");
        #region Import \\ Export
        {
            result.AppendLine($"{Indentation2}#region Import \\ Export");
            result.AppendLine();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            result.AppendLine($"{Indentation2}public async Task<IActionResult> ExportData()");
            result.AppendLine($"{Indentation2}{{");
            var requiredColumns = model.Properties.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
            result.Append($"{Indentation3}var result = await this.provider.GetAll(");
            if (requiredColumns.Length == 0) { }
            else if (requiredColumns.Length == 1)
            {
                if (requiredColumns[0].Name.EndsWith("_COMPANY"))
                {
                    result.Append($"this.companyId");
                }
                else
                {
                    result.Append($"this.erpCode");
                }
            }
            else
            {
                if (requiredColumns[0].Name.EndsWith("_COMPANY"))
                {
                    result.Append($"this.companyId, this.erpCode");
                }
                else
                {
                    result.Append($"this.erpCode, this.companyId");
                }

            }
            result.AppendLine($");");
            result.AppendLine($"{Indentation3}var fileName = $\"{model.Name.ToPascalCase()}.{{DateTime.Now.ToString(\"yyyyMMdd.HHmmss\")}}.csv\";");
            result.AppendLine($"{Indentation3}this.toastNotification.AddSuccessToastMessage($\"Export file {{fileName}} created.\");");
            result.AppendLine();
            result.AppendLine($"{Indentation3}return File(result.ToCsvByteArray(), \"text/csv\", fileName);");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation2}[HttpGet]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            result.AppendLine($"{Indentation2}public async Task<IActionResult> ImportData()");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}ViewData[RmsCommon.Costants.ViewDataCostants.ControllerName] = this.ControllerContext.RouteData.Values[\"controller\"].ToString();;");
            result.AppendLine($"{Indentation3}return PartialView(\"../Common/FileUploadPartial\");");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation2}[HttpPost]");
            result.AppendLine($"{Indentation2}[Authorization.RAMSAuthorize(RmsCommon.Enums.RMSOperation.All, RmsCommon.Enums.RMSOperation.ErpManage)]");
            result.AppendLine($"{Indentation2}public async Task<IActionResult> ImportFile(IFormFile postedFiles)");
            result.AppendLine($"{Indentation2}{{");
            result.AppendLine($"{Indentation3}if (postedFiles == null || postedFiles.Length == 0)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}this.toastNotification.AddErrorToastMessage(\"File not selected.\");");
            result.AppendLine($"{Indentation4}return Ok(new {{status = \"Fail\"}});");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation3}var result = new List<string>();");
            result.AppendLine($"{Indentation3}using (var reader = new StreamReader(postedFiles.OpenReadStream()))");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}while (reader.Peek() >= 0)");
            result.AppendLine($"{Indentation4}{{");
            result.AppendLine($"{Indentation5}result.Add(reader.ReadLine());");
            result.AppendLine($"{Indentation4}}}");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation3}if (result.Count == 0)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}this.toastNotification.AddErrorToastMessage(\"File is empty.\");");
            result.AppendLine($"{Indentation4}return Ok(new {{status = \"Fail\"}});");
            result.AppendLine($"{Indentation3}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation3}var columns = new List<string>(result[0].Split(','));");
            result.AppendLine($"{Indentation3}var index = 0;");
            result.AppendLine($"{Indentation3}var imported = 0;");
            result.AppendLine($"{Indentation3}for(var i = 1; i < result.Count; i++)");
            result.AppendLine($"{Indentation3}{{");
            result.AppendLine($"{Indentation4}if (string.IsNullOrWhiteSpace(result[i])) continue;");
            result.AppendLine($"{Indentation4}var values = result[i].Split(',');");
            result.AppendLine($"{Indentation4}var model = new Model.{model.Name.ToPascalCase().ToSingular()}();");
            foreach (var column in model.Properties)
            {
                if (column.Name.EndsWith("_COMPANY"))
                {
                    result.AppendLine($"{Indentation4}model.{column.Name.ToPascalCase()} = this.companyId;");
                }
                else if (column.Name.EndsWith("_ERP_CODE"))
                {
                    result.AppendLine($"{Indentation4}model.{column.Name.ToPascalCase()} = this.erpCode;");
                }
                else
                {
                    result.AppendLine($"{Indentation4}index = columns.IndexOf(\"{column.Name.ToPascalCase()}\");");
                    result.AppendLine($"{Indentation4}if (index >= 0) model.{column.Name.ToPascalCase()} = {column.GetCodeConvertDataType()}(values[index].Trim('\"'));");
                }
            }
            result.AppendLine();
            result.AppendLine($"{Indentation4}if(await this.provider.Save(model)) imported++;");
            result.AppendLine($"{Indentation3}}}");
            //result.AppendLine($"{Indentation3}var result = await this.provider.Import(postedFiles);");
            //result.AppendLine($"{Indentation3}if (result)");
            //result.AppendLine($"{Indentation3}{{");
            //result.AppendLine($"{Indentation4}this.toastNotification.AddSuccessToastMessage(\"Data imported successfully.\");");
            //result.AppendLine($"{Indentation4}return RedirectToAction(\"Index\");");
            //result.AppendLine($"{Indentation3}}}");
            result.AppendLine($"{Indentation3}this.toastNotification.AddSuccessToastMessage(\"{{imported}} rows imported.\");");
            result.AppendLine($"{Indentation5}return Ok(new {{ status = \"success\", action = \"refresh\" }});");
            result.AppendLine($"{Indentation2}}}");
            result.AppendLine();
            result.AppendLine($"{Indentation2}#endregion");
        }
        #endregion
        result.AppendLine($"{Indentation}}}");
        result.AppendLine("}");

        return result.ToString();
    }

    public static string GetViewClass(this Model table,
        string classNamespace,
        string? className = null,
        string? classNameRepository = null,
        string? classNameProvider = null,
        string? classNameController = null,
        string? inheritedClass = null,
        IEnumerable<string>? usingList = null)
    {
        var result = new StringBuilder();
        var modelName = table.Name.ToPascalCase().ToSingular();
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
        if (classNameController == null)
        {
            classNameController = $"{className}Controller";
        }
        if (usingList != null)
        {
            foreach (var item in usingList)
            {
                result.AppendLine($"@using {item}");
            }
        }
        result.AppendLine();
        result.AppendLine($"@model Model.{modelName}");
        result.AppendLine();
        result.AppendLine($"@{{");
        result.AppendLine($"{Indentation}ViewData[\"Title\"] = \"{table.Name.ToPascalCase(" ")}\";");
        result.AppendLine($"}}");
        result.AppendLine($"<div class=\"row\">");
        result.AppendLine($"{Indentation}<div class=\"col-lg-6 col-md-6 col-sm-6 col-xs-6\">");
        result.AppendLine($"{Indentation2}<h1>{table.Name.ToPascalCase(" ")}</h1>");
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"{Indentation}<div class=\"col-lg-6 col-md-6 col-sm-6 col-xs-6 text-right\">");
        result.AppendLine($"{Indentation2}<button title=\"Help\" class=\"btn btn-link deletethis helpIcon\" onclick=\"openHandbook('General_{table.Name.ToPascalCase()}')\">");
        result.AppendLine($"{Indentation3}<span class=\"fa fa-question-circle fa-lg\"></span>");
        result.AppendLine($"{Indentation2}</button>");
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"</div>");

        result.AppendLine($"@section Scripts");
        result.AppendLine($"{{");
        result.AppendLine($"{Indentation}<script>");
        result.AppendLine($"{Indentation2}function EditData(id) {{");
        result.AppendLine($"{Indentation3}$('#placeholderPartialView').html('<i class=\"fa fa-spinner fa-spin fa-fw\"></i> Loading...');");
        result.AppendLine($"{Indentation3}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation3}$.ajax({{");
        result.AppendLine($"{Indentation4}type: \"GET\",");
        result.AppendLine($"{Indentation4}url: \"/Erp/{table.Name.ToPascalCase()}/AddEditData?\" + $.param({{ code: id }}),");
        result.AppendLine($"{Indentation4}success: function(response) {{");
        result.AppendLine($"{Indentation5}$('#placeholderPartialView').html(response);");
        result.AppendLine($"{Indentation5}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation4}}},");
        result.AppendLine($"{Indentation4}error: function(xhr, status, error) {{");
        result.AppendLine($"{Indentation5}$(\"#errormessage\").text('Error(' + xhr.status + '): ' + error);");
        result.AppendLine($"{Indentation5}$('#divPartialView').modal('hide');");
        //result.AppendLine($"{Indentation5}$('#placeholderPartialView').html('<i class=\"fas fa-exclamation-triangle\" aria-hidden=\"true\" style=\"color: red;\"></i> Error(' + xhr.status + '): ' + error);");
        //result.AppendLine($"{Indentation5}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation4}}}");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function ExportData() {{");
        result.AppendLine($"{Indentation3}$('#placeholderPartialView').html('<i class=\"fa fa-spinner fa-spin fa-fw\"></i> Exporting data for {table.Name.ToPascalCase()}...');");
        result.AppendLine($"{Indentation3}window.location.href = \"/Erp/{table.Name.ToPascalCase()}/ExportData\";");
        result.AppendLine($"{Indentation3}setTimeout(function() {{ location.reload(); }}, 2000);");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function ImportData() {{");
        result.AppendLine($"{Indentation3}$('#placeholderPartialView').html('<i class=\"fa fa-spinner fa-spin fa-fw\"></i> Loading...');");
        result.AppendLine($"{Indentation3}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation3}$.ajax({{");
        result.AppendLine($"{Indentation4}type: \"GET\",");
        result.AppendLine($"{Indentation4}url: \"/Erp/{table.Name.ToPascalCase()}/ImportData\",");
        result.AppendLine($"{Indentation4}success: function(response) {{");
        result.AppendLine($"{Indentation5}$('#placeholderPartialView').html(response);");
        result.AppendLine($"{Indentation5}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation4}}},");
        result.AppendLine($"{Indentation4}error: function(xhr, status, error) {{");
        result.AppendLine($"{Indentation5}$(\"#errormessage\").text('Error(' + xhr.status + '): ' + error);");
        result.AppendLine($"{Indentation5}$('#divPartialView').modal('hide');");
        //result.AppendLine($"{Indentation5}$('#placeholderPartialView').html('<i class=\"fas fa-exclamation-triangle\" aria-hidden=\"true\" style=\"color: red;\"></i> Error(' + xhr.status + '): ' + error);");
        //result.AppendLine($"{Indentation5}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation4}}}");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function DeleteData(id) {{");
        result.AppendLine($"{Indentation3}$('#placeholderPartialView').html('<i class=\"fa fa-spinner fa-spin fa-fw\"></i> Deleting...');");
        result.AppendLine($"{Indentation3}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation3}showDialog(\"Delete {modelName}\", \"Are you sure you want to delete {modelName}?\", \"Yes\", \"No\", function(result) {{");
        result.AppendLine($"{Indentation4}if (result) {{");
        result.AppendLine($"{Indentation5}$.ajax({{");
        result.AppendLine($"{Indentation6}type: \"DELETE\",");
        result.AppendLine($"{Indentation6}url: \"/Erp/{table.Name.ToPascalCase()}/Delete?\" + $.param({{ code: id }}),");
        result.AppendLine($"{Indentation6}success: function(data) {{");
        result.AppendLine($"{Indentation7}table.ajax.reload(null, false);");
        result.AppendLine($"{Indentation7}$('#divPartialView').modal('hide');");
        result.AppendLine($"{Indentation6}}},");
        result.AppendLine($"{Indentation6}error: function(xhr, status, error) {{");
        result.AppendLine($"{Indentation7}$(\"#errormessage\").text('Error(' + xhr.status + '): ' + error);");
        result.AppendLine($"{Indentation7}$('#divPartialView').modal('hide');");
        //result.AppendLine($"{Indentation7}$('#placeholderPartialView').html('<i class=\"fas fa-exclamation-triangle\" aria-hidden=\"true\" style=\"color: red;\"></i> Error(' + xhr.status + '): ' + error);");
        //result.AppendLine($"{Indentation7}$('#divPartialView').modal('show');");
        result.AppendLine($"{Indentation6}}}");
        result.AppendLine($"{Indentation5}}});");
        result.AppendLine($"{Indentation4}}}");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function resourceChange() {{");
        result.AppendLine($"{Indentation3}var selectedVal = $(\"#resource option:selected\").val();");
        result.AppendLine($"{Indentation3}$(\"#resourceGroup\").val(selectedVal);");
        result.AppendLine($"{Indentation3}$(\"#workcenter\").val(selectedVal);");
        result.AppendLine($"{Indentation3}$(\"#site\").val(selectedVal);");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function OnBegin() {{");
        result.AppendLine($"{Indentation3}$(\"#errormessage\").text(\"\");");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function OnFailure(response) {{");
        result.AppendLine($"{Indentation3}$(\"#errormessage\").text(response.responseText);");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function OnSuccess(response) {{");
        result.AppendLine($"{Indentation3}if (response.status == \"success\") {{");
        result.AppendLine($"{Indentation4}if (response.action == \"refresh\") {{");
        result.AppendLine($"{Indentation5}table.ajax.reload(null, false);");
        result.AppendLine($"{Indentation4}}}");
        result.AppendLine($"{Indentation4}$(\"#divPartialView\").modal('hide');");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function OnComplete() {{");
        result.AppendLine($"{Indentation3}$(\"#progress\").modal('hide');");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function actionButtons(actionType, dataRow) {{");
        result.AppendLine($"{Indentation3}if (actionType == 0) {{");
        result.AppendLine($"{Indentation4}return '<btn title=\"Edit\" class=\"btn btn-link btn-sm editthis\" onclick=\"EditData(' + dataRow.{table.IdentityColumn!.Name.ToCamelCase()} + ');\"><span class=\"fa fa-edit fa-lg\"></span></a>';");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation3}else if (actionType == 1) {{");
        result.AppendLine($"{Indentation4}return '<btn title=\"Delete\" class=\"btn btn-link btn-sm deletethis\" onclick=\"DeleteData(' + dataRow.{table.IdentityColumn!.Name.ToCamelCase()} + ');\"><span class=\"fa fa-trash fa-lg\"></span></a>';");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}var table;");
        result.AppendLine();
        result.AppendLine($"{Indentation2}$(document).ready(function() {{");
        result.AppendLine();
        result.AppendLine($"{Indentation3}table = $(\"#viewGrid\").DataTable({{");
        result.AppendLine($"{Indentation4}\"stateSave\": true,");
        result.AppendLine($"{Indentation4}\"processing\": true, // for show progress bar");
        result.AppendLine($"{Indentation4}\"filter\": true, // this is for disable filter (search box)");
        result.AppendLine($"{Indentation4}\"orderMulti\": false, // for disable multiple column at once");
        result.AppendLine($"{Indentation4}\"pageLength\": 10,");
        result.AppendLine($"{Indentation4}\"searchPanes\": {{ viewTotal: true }},");
        result.AppendLine($"{Indentation4}\"orderCellsTop\": true,");
        result.AppendLine($"{Indentation4}\"fixedHeader\": true,");
        result.AppendLine($"{Indentation4}\"ordering\": true,");
        result.AppendLine($"{Indentation4}\"ajax\": {{");
        result.AppendLine($"{Indentation5}\"url\": \"/Erp/{table.Name.ToPascalCase()}/LoadData\",");
        result.AppendLine($"{Indentation5}\"type\": \"GET\",");
        result.AppendLine($"{Indentation5}\"datatype\": \"json\"");
        result.AppendLine($"{Indentation4}}},");
        result.AppendLine($"{Indentation4}\"columnDefs\": [");
        result.AppendLine($"{Indentation5}{{ targets: \"no-sort\", orderable: false }},");
        result.AppendLine($"{Indentation5}{{");
        result.AppendLine($"{Indentation6}\"targets\": [0],");
        result.AppendLine($"{Indentation6}\"visible\": false,");
        result.AppendLine($"{Indentation6}\"searchable\": false");
        result.AppendLine($"{Indentation5}}}");
        result.AppendLine($"{Indentation4}],");
        result.AppendLine($"{Indentation4}\"columns\": [");
        foreach (var column in table.Properties)
        {
            if (column.Name == table.IdentityColumn!.Name)
            {
                result.AppendLine($"{Indentation5}{{ \"data\": \"{column.Name.ToCamelCase()}\", \"autoWidth\": true }},");
            }
            else
            {
                if (column.Name.EndsWith("_COMPANY") || column.Name.EndsWith("_ERP_CODE") || column.Name.EndsWith("_RECVERSION") || !column.Visible)
                { }
                else
                {
                    if (column.IsNullable)
                    {
                        result.AppendLine($"{Indentation5}{{ \"data\": \"{column.Name.ToCamelCase()}\", \"autoWidth\": true, \"defaultContent\": \"\" }},");
                    }
                    else
                    {
                        result.AppendLine($"{Indentation5}{{ \"data\": \"{column.Name.ToCamelCase()}\", \"autoWidth\": true }},");
                    }
                    //result.AppendLine($"{Indentation5}{{ \"data\": \"{column.Name.ToCamelCase()}\", \"autoWidth\": true }},");
                }
            }
        }
        result.AppendLine($"{Indentation2}@{{");
        result.AppendLine($"{Indentation3}if ((bool)ViewData[ViewDataCostants.CanManage])");
        result.AppendLine($"{Indentation3}{{");
        result.AppendLine($"{Indentation4}<text>");
        result.AppendLine($"{Indentation5}{{\"render\": function(data, type, full, meta) {{ return actionButtons(0, full); }} }},");
        result.AppendLine($"{Indentation5}{{\"render\": function(data, type, full, meta) {{ return actionButtons(1, full); }} }},");
        result.AppendLine($"{Indentation4}</text>");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine($"{Indentation4}],");
        result.AppendLine($"{Indentation4}dom: '<\"top\">rt<\"bottom\"lp><\"clear\">i'");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine();
        result.AppendLine($"{Indentation3}var state = table.state.loaded();");
        result.AppendLine($"{Indentation3}if (state) {{");
        result.AppendLine($"{Indentation4}table.columns().eq(0).each(function(colIdx) {{");
        result.AppendLine($"{Indentation5}var colSearch = state.columns[colIdx].search;");
        result.AppendLine();
        result.AppendLine($"{Indentation5}if (colSearch.search) {{");
        result.AppendLine($"{Indentation6}$('input', table.column(colIdx).header()).val(colSearch.search);");
        result.AppendLine($"{Indentation5}}}");
        result.AppendLine($"{Indentation4}}});");
        result.AppendLine($"{Indentation4}table.draw();");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation3}table.columns().every(function() {{");
        result.AppendLine($"{Indentation4}var that = this;");
        result.AppendLine($"{Indentation4}$('input', this.header()).on('keyup change', function() {{");
        result.AppendLine($"{Indentation5}if (that.search() !== this.value) {{");
        result.AppendLine($"{Indentation6}that.search(this.value)");
        result.AppendLine($"{Indentation7}.draw();");
        result.AppendLine($"{Indentation5}}}");
        result.AppendLine($"{Indentation4}}});");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine($"{Indentation2}}});");
        result.AppendLine($"{Indentation}</script>");
        result.AppendLine($"}}");
        result.AppendLine();
        result.AppendLine($"@if ((bool)ViewData[ViewDataCostants.CanManage])");
        result.AppendLine($"{{");
        result.AppendLine($"{Indentation}<div id=\"actionbar\">");
        result.AppendLine($"{Indentation2}<ul>");
        result.AppendLine($"{Indentation3}<li>");
        #region Add new record
        result.AppendLine($"{Indentation4}@* Add new {table.Name.ToPascalCase().ToSingular()} *@");
        result.AppendLine($"{Indentation4}<form method=\"get\"");
        result.AppendLine($"{Indentation5}  asp-area=\"Data\"");
        result.AppendLine($"{Indentation5}  asp-action=\"AddEditData\"");
        result.AppendLine($"{Indentation5}  asp-controller=\"{table.Name.ToPascalCase()}\"");
        result.AppendLine($"{Indentation5}  data-ajax=\"true\"");
        result.AppendLine($"{Indentation5}  data-ajax-method=\"GET\"");
        result.AppendLine($"{Indentation5}  data-ajax-mode=\"replace\"");
        result.AppendLine($"{Indentation5}  data-ajax-update=\"#placeholderPartialView\">");
        result.AppendLine($"{Indentation5}<button class=\"btn btn-sm btn-primary mb-2\" data-toggle=\"modal\" data-target=\"#divPartialView\" onclick=\"EditData()\"><span class=\"fas fa-plus-circle\"></span>&nbsp;New</button>");
        result.AppendLine($"{Indentation4}</form>");
        #endregion
        result.AppendLine($"{Indentation3}</li>");
        result.AppendLine($"{Indentation3}<li>");
        #region Export to Excel
        result.AppendLine($"{Indentation4}@* Export {table.Name.ToPascalCase().ToSingular()} to CSV *@");
        result.AppendLine($"{Indentation4}<form method=\"get\"");
        result.AppendLine($"{Indentation5}  asp-area=\"Data\"");
        result.AppendLine($"{Indentation5}  asp-action=\"AddExportData\"");
        result.AppendLine($"{Indentation5}  asp-controller=\"{table.Name.ToPascalCase()}\"");
        result.AppendLine($"{Indentation5}  data-ajax=\"true\"");
        result.AppendLine($"{Indentation5}  data-ajax-method=\"GET\"");
        result.AppendLine($"{Indentation5}  data-ajax-mode=\"replace\"");
        result.AppendLine($"{Indentation5}  data-ajax-update=\"#placeholderPartialView\">");
        result.AppendLine($"{Indentation5}<button class=\"btn btn-sm btn-primary mb-2\" data-toggle=\"modal\" data-target=\"#divPartialView\" onclick=\"ExportData()\"><span class=\"fas fa-download\"></span>&nbsp;Export</button>");
        result.AppendLine($"{Indentation4}</form>");
        #endregion
        result.AppendLine($"{Indentation3}</li>");
        result.AppendLine($"{Indentation3}<li>");
        #region Import to Excel
        result.AppendLine($"{Indentation4}@* Import {table.Name.ToPascalCase().ToSingular()} to CSV *@");
        result.AppendLine($"{Indentation4}<form method=\"get\"");
        result.AppendLine($"{Indentation5}  asp-area=\"Data\"");
        result.AppendLine($"{Indentation5}  asp-action=\"AddImportData\"");
        result.AppendLine($"{Indentation5}  asp-controller=\"{table.Name.ToPascalCase()}\"");
        result.AppendLine($"{Indentation5}  data-ajax=\"true\"");
        result.AppendLine($"{Indentation5}  data-ajax-method=\"GET\"");
        result.AppendLine($"{Indentation5}  data-ajax-mode=\"replace\"");
        result.AppendLine($"{Indentation5}  data-ajax-update=\"#placeholderPartialView\">");
        result.AppendLine($"{Indentation5}<button class=\"btn btn-sm btn-primary mb-2\" data-toggle=\"modal\" data-target=\"#divPartialView\" onclick=\"ImportData()\"><span class=\"fas fa-upload\"></span>&nbsp;Import</button>");
        result.AppendLine($"{Indentation4}</form>");
        #endregion
        result.AppendLine($"{Indentation3}</li>");
        result.AppendLine($"{Indentation2}</ul>");
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"}}");
        result.AppendLine();
        result.AppendLine($"<div class=\"modal fade\" id=\"divPartialView\">");
        result.AppendLine($"{Indentation}<div class=\"modal-dialog\">");
        result.AppendLine($"{Indentation2}<div class=\"modal-content\">");
        result.AppendLine($"{Indentation3}<div id=\"placeholderPartialView\"></div>");
        result.AppendLine($"{Indentation3}<div>");
        result.AppendLine($"{Indentation4}<label id=\"errormessage\" class=\"text-danger\"></label>");
        result.AppendLine($"{Indentation3}</div>");
        result.AppendLine($"{Indentation2}</div>");
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"</div>");
        result.AppendLine();
        result.AppendLine($"<table id=\"viewGrid\" class=\"table table-striped dt-responsive mb-2\" cellspacing=\"0\">");
        result.AppendLine($"{Indentation}<thead>");
        result.AppendLine($"{Indentation2}<tr>");
        foreach (var property in table.Properties)
        {
            if (property.Name == table.IdentityColumn!.Name)
            {
                result.AppendLine($"{Indentation3}<th>{property.Name.ToPascalCase()}</th>");
            }
            else if (property.Name.EndsWith("_COMPANY") || property.Name.EndsWith("_ERP_CODE") || property.Name.EndsWith("_RECVERSION") || !property.Visible)
            { }
            else
            {
                result.AppendLine($"{Indentation3}<th>{property.Label ?? property.Name.ToPascalCase(" ")[3..]}<br /><input type=\"text\" placeholder=\"{property.Label ?? property.Name.ToPascalCase(" ")[3..]}\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
            }
        }
        result.AppendLine($"{Indentation3}@if ((bool)ViewData[ViewDataCostants.CanManage])");
        result.AppendLine($"{Indentation3}{{");
        result.AppendLine($"{Indentation4}<th class=\"no-sort\"></th>");
        result.AppendLine($"{Indentation4}<th class=\"no-sort\"></th>");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation2}</tr>");
        result.AppendLine($"{Indentation}</thead>");
        result.AppendLine($"</table>");

        return result.ToString();
    }

    public static string GetPartialViewClass(this Model table,
        string classNamespace,
        string? className = null,
        string? classNameRepository = null,
        string? classNameProvider = null,
        string? classNameController = null,
        string? inheritedClass = null,
        IEnumerable<string>? usingList = null)
    {
        var result = new StringBuilder();
        var modelName = table.Name.ToPascalCase().ToSingular();
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
        if (classNameController == null)
        {
            classNameController = $"{className}Controller";
        }
        if (usingList != null)
        {
            foreach (var item in usingList)
            {
                result.AppendLine($"@using {item}");
            }
        }
        result.AppendLine();
        result.AppendLine($"@model Model.{modelName}");
        result.AppendLine();
        result.AppendLine($"<div class=\"modal-header\">");
        result.AppendLine($"{Indentation}@if ((bool)ViewData[ViewDataCostants.NewRecord])");
        result.AppendLine($"{Indentation}{{");
        result.AppendLine($"{Indentation2}<h4 class=\"modal-title\">Add {modelName}</h4>");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine($"{Indentation}else");
        result.AppendLine($"{Indentation}{{");
        result.AppendLine($"{Indentation2}<h4 class=\"modal-title\">Edit {modelName}</h4>");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine($"</div>");
        result.AppendLine();
        result.AppendLine($"<form method=\"post\"");
        result.AppendLine($"{Indentation} asp-action=\"SaveData\"");
        result.AppendLine($"{Indentation} asp-controller=\"{table.Name.ToPascalCase()}\"");
        result.AppendLine($"{Indentation} data-ajax=\"true\"");
        result.AppendLine($"{Indentation} data-ajax-method=\"POST\"");
        result.AppendLine($"{Indentation} data-ajax-begin=\"OnBegin\"");
        result.AppendLine($"{Indentation} data-ajax-failure=\"OnFailure\"");
        result.AppendLine($"{Indentation} data-ajax-success=\"OnSuccess\"");
        result.AppendLine($"{Indentation} data-ajax-complete=\"OnComplete\"");
        result.AppendLine($"{Indentation} class=\"needs-validation\"");
        result.AppendLine($"{Indentation} novalidate>");
        result.AppendLine($"{Indentation}<div class=\"modal-body\">");
        var requiredColumns = table.Properties.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE") || c.Name.EndsWith("_RECVERSION")).ToArray();
        foreach (var column in requiredColumns)
        {
            result.AppendLine($"{Indentation}@Html.HiddenFor(model => model.{column.Name.ToPascalCase()})");
        }
        foreach (var column in table.WritableColumns)
        {
            if (requiredColumns.Contains(column)) continue;
            if (!column.Visible)
            {
                result.AppendLine($"{Indentation}@Html.HiddenFor(model => model.{column.Name.ToPascalCase()})");
                continue;
            }

            if (!column.IsPrimaryKey)
            {
                if (column.VisibleInCreate)
                {
                    result.AppendLine($"{Indentation}<div class=\"form-group\" id=\"form{column.Name.ToPascalCase()}\">");
                    result.AppendLine($"{Indentation2}<label for=\"{column.Name.ToPascalCase()}\">{column.Label ?? column.Name.ToPascalCase(" ")[3..]}</label>");
                    result.Append($"{Indentation2}@Html.EditorFor(model => model.{column.Name.ToPascalCase()}, new {{ htmlAttributes = new {{ @class = \"form-control\", placeholder = \"{column.Label ?? column.Name.ToPascalCase(" ")[3..]}\"");
                    if (!column.IsNullable || column.Required)
                    {
                        result.Append(", required = \"required\"");
                        result.AppendLine(" } })");
                        result.AppendLine($"{Indentation2}<div class=\"invalid-feedback\">");
                        result.AppendLine($"{Indentation3}<i class=\"fas fa-exclamation-triangle\"></i>&nbsp;&nbsp;Please enter {column.Label ?? column.Name.ToPascalCase(" ")[3..]}");
                        result.AppendLine($"{Indentation2}</div>");
                    }
                    else
                    {
                        result.AppendLine(" } })");
                    }
                    result.AppendLine($"{Indentation}</div>");
                }
                else
                {
                    result.AppendLine($"{Indentation}@if ((bool)ViewData[ViewDataCostants.NewRecord])");
                    result.AppendLine($"{Indentation}{{");
                    result.AppendLine($"{Indentation2}@Html.HiddenFor(model => model.{column.Name.ToPascalCase()})");
                    result.AppendLine($"{Indentation}}}");
                    result.AppendLine($"{Indentation}else");
                    result.AppendLine($"{Indentation}{{");
                    result.AppendLine($"{Indentation2}<div class=\"form-group\" id=\"form{column.Name.ToPascalCase()}\">");
                    result.AppendLine($"{Indentation2}<label for=\"{column.Name.ToPascalCase()}\">{column.Label ?? column.Name.ToPascalCase(" ")[3..]}</label>");
                    result.Append($"{Indentation2}@Html.EditorFor(model => model.{column.Name.ToPascalCase()}, new {{ htmlAttributes = new {{ @class = \"form-control\", placeholder = \"{column.Label ?? column.Name.ToPascalCase(" ")[3..]}\"");
                    if (!column.IsNullable || column.Required)
                    {
                        result.Append(", required = \"required\"");
                        result.AppendLine(" } })");
                        result.AppendLine($"{Indentation2}<div class=\"invalid-feedback\">");
                        result.AppendLine($"{Indentation3}<i class=\"fas fa-exclamation-triangle\"></i>&nbsp;&nbsp;Please enter {column.Label ?? column.Name.ToPascalCase(" ")[3..]}");
                        result.AppendLine($"{Indentation2}</div>");
                    }
                    else
                    {
                        result.AppendLine(" } })");
                    }

                    result.AppendLine($"{Indentation2}</div>");
                    result.AppendLine($"{Indentation}}}");
                }
            }
            else
            {
                result.AppendLine($"{Indentation}<div class=\"form-group\" id=\"form{column.Name.ToPascalCase()}\">");
                result.AppendLine($"{Indentation2}<label for=\"{column.Name.ToPascalCase()}\">{column.Label ?? column.Name.ToPascalCase(" ")[3..]}</label>");
                result.AppendLine($"{Indentation2}@if ((bool)ViewData[ViewDataCostants.NewRecord])");
                result.AppendLine($"{Indentation2}{{");
                result.AppendLine($"{Indentation3}@Html.EditorFor(model => model.{column.Name.ToPascalCase()}, new {{ htmlAttributes = new {{ @class = \"form-control\", placeholder = \"{column.Label ?? column.Name.ToPascalCase(" ")[3..]}\", required = \"required\" }} }})");
                result.AppendLine($"{Indentation2}}}");
                result.AppendLine($"{Indentation2}else");
                result.AppendLine($"{Indentation2}{{");
                result.AppendLine($"{Indentation3}@Html.HiddenFor(model => model.{column.Name.ToPascalCase()})");
                result.AppendLine($"{Indentation3}@Html.EditorFor(model => model.{column.Name.ToPascalCase()}, new {{ htmlAttributes = new {{ @class = \"form-control\", placeholder = \"{column.Label ?? column.Name.ToPascalCase(" ")[3..]}\", required = \"required\", disabled = \"disabled\", @readonly = \"readonly\" }} }})");
                result.AppendLine($"{Indentation2}}}");
                result.AppendLine($"{Indentation2}<div class=\"invalid-feedback\">");
                result.AppendLine($"{Indentation3}<i class=\"fas fa-exclamation-triangle\"></i>&nbsp;&nbsp;Please enter {column.Label ?? column.Name.ToPascalCase(" ")[3..]}");
                result.AppendLine($"{Indentation2}</div>");
                result.AppendLine($"{Indentation}</div>");
            }
        }
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"{Indentation}<div class=\"modal-footer\">");
        result.AppendLine($"{Indentation2}<button type=\"submit\" class=\"btn btn-primary\" data-save=\"modal\"><span class=\"far fa-save\"></span>&nbsp;Save</button>");
        result.AppendLine($"{Indentation2}<button type=\"button\" class=\"btn btn-secondary\" data-dismiss=\"modal\"><span class=\"fas fa-times\"></span>&nbsp;Cancel</button>");
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"</form>");

        result.AppendLine($"<script>");
        result.AppendLine($"{Indentation}\"use strict\";");
        result.AppendLine();
        result.AppendLine($"{Indentation}(function() {{");
        result.AppendLine($"{Indentation2}var forms = document.querySelectorAll('.needs-validation');");
        result.AppendLine();
        result.AppendLine($"{Indentation2}// Loop over them and prevent submission");
        result.AppendLine($"{Indentation2}Array.prototype.slice.call(forms)");
        result.AppendLine($"{Indentation3}.forEach(function(form) {{");
        result.AppendLine($"{Indentation4}form.addEventListener('submit',");
        result.AppendLine($"{Indentation5}function(event) {{");
        result.AppendLine($"{Indentation6}if (!form.checkValidity()) {{");
        result.AppendLine($"{Indentation7}event.preventDefault();");
        result.AppendLine($"{Indentation7}event.stopPropagation();");
        result.AppendLine($"{Indentation6}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation6}form.classList.add('was-validated');");
        result.AppendLine($"{Indentation5}}},");
        result.AppendLine($"{Indentation5}false);");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine($"{Indentation}}})();");
        result.AppendLine($"</script>");

        return result.ToString();
    }

    #region Private Methods

    private static string GetProperties(this Model model, string indentation = "")
    {
        var result = new StringBuilder();
        foreach (var property in model.Properties)
        {
            result.AppendLine($"{indentation}[Column(\"{property.Name}\")]");
            result.AppendLine($"{indentation}public {property.GetCodeDataType()} {property.Name.ToPascalCase()} {{ get; set; }}");
            result.AppendLine();
        }

        return result.ToString();
    }

    #endregion

    #endregion
}
