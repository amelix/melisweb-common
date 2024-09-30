﻿using System.Text;

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

    public static string GetViewClass(this Table table,
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
        result.AppendLine($"{Indentation}ViewData[\"Title\"] = \"{table.Name.ToPascalCase()}\";");
        result.AppendLine($"}}");
        result.AppendLine($"<div class=\"row\">");
        result.AppendLine($"{Indentation}<div class=\"col-lg-6 col-md-6 col-sm-6 col-xs-6\">");
        result.AppendLine($"{Indentation2}<h1>{table.Name.ToPascalCase()}</h1>");
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
        result.AppendLine($"{Indentation3}$.ajax({{");
        result.AppendLine($"{Indentation4}type: \"GET\",");
        result.AppendLine($"{Indentation4}url: \"/Erp/{table.Name.ToPascalCase()}/AddEditData?\" + $.param({{ code: id }}),");
        result.AppendLine($"{Indentation4}success: function(response) {{");
        result.AppendLine($"{Indentation5}$('#placeholderAddEditData').html(response);");
        result.AppendLine($"{Indentation5}$('#addEditData').modal('show');");
        result.AppendLine($"{Indentation4}}},");
        result.AppendLine($"{Indentation4}error: function(xhr, status, error) {{}}");
        result.AppendLine($"{Indentation3}}});");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function DeleteData(id) {{");
        result.AppendLine($"{Indentation3}showDialog(\"Delete {modelName}\", \"Are you sure you want to delete {modelName}?\", \"Yes\", \"No\", function(result) {{");
        result.AppendLine($"{Indentation4}if (result) {{");
        result.AppendLine($"{Indentation5}$.ajax({{");
        result.AppendLine($"{Indentation6}type: \"DELETE\",");
        result.AppendLine($"{Indentation6}url: \"/Erp/{table.Name.ToPascalCase()}/Delete?\" + $.param({{ code: id }}),");
        result.AppendLine($"{Indentation6}success: function(data) {{");
        result.AppendLine($"{Indentation7}table.ajax.reload(null, false);");
        result.AppendLine($"{Indentation6}}},");
        result.AppendLine($"{Indentation6}error: function(xhr, status, error) {{}}");
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
        result.AppendLine($"{Indentation4}$(\"#addEditData\").modal('hide');");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function OnComplete() {{");
        result.AppendLine($"{Indentation3}$(\"#progress\").modal('hide');");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}function actionButtons(actionType, dataRow) {{");
        result.AppendLine($"{Indentation3}if (actionType == 0) {{");
        result.AppendLine($"{Indentation4}return '<btn title=\"Edit\" class=\"btn btn-link btn-sm editthis\" onclick=\"EditData(\'' + dataRow.worWorkorder + '\')\"><span class=\"fa fa-edit fa-lg\"></span></a>';");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation3}else if (actionType == 1) {{");
        result.AppendLine($"{Indentation4}return '<btn title=\"Delete\" class=\"btn btn-link btn-sm deletethis\" onclick=\"DeleteData(\'' + dataRow.worWorkorder + '\')\"><span class=\"fa fa-trash fa-lg\"></span></a>';");
        result.AppendLine($"{Indentation3}}}");
        result.AppendLine($"{Indentation2}}}");
        result.AppendLine();
        result.AppendLine($"{Indentation2}var table;");
        result.AppendLine();
        result.AppendLine($"{Indentation2}$(document).ready(function() {{");
        result.AppendLine();
        result.AppendLine($"{Indentation3}table = $(\"#viewGrid\").DataTable({{");
        result.AppendLine($"{Indentation3}    \"stateSave\": true,");
        result.AppendLine($"{Indentation3}    \"processing\": true, // for show progress bar");
        result.AppendLine($"{Indentation3}    \"filter\": true, // this is for disable filter (search box)");
        result.AppendLine($"{Indentation3}    \"orderMulti\": false, // for disable multiple column at once");
        result.AppendLine($"{Indentation3}    \"pageLength\": 10,");
        result.AppendLine($"{Indentation3}    \"searchPanes\": {{");
        result.AppendLine($"{Indentation3}        viewTotal: true");
        result.AppendLine($"{Indentation3}    }},");
        result.AppendLine($"{Indentation3}    \"orderCellsTop\": true,");
        result.AppendLine($"{Indentation3}    \"fixedHeader\": true,");
        result.AppendLine($"{Indentation3}    \"ordering\": true,");
        result.AppendLine($"{Indentation3}    \"ajax\": {{");
        result.AppendLine($"{Indentation3}        \"url\": \"/Erp/{table.Name.ToPascalCase()}/LoadData\",");
        result.AppendLine($"{Indentation3}        \"type\": \"GET\",");
        result.AppendLine($"{Indentation3}        \"datatype\": \"json\"");
        result.AppendLine($"{Indentation3}    }},");
        result.AppendLine();
        result.AppendLine($"{Indentation4}\"columnDefs\":");
        result.AppendLine($"{Indentation4}    [");
        result.AppendLine($"{Indentation4}        {{ targets: \"no-sort\", orderable: false }},");
        result.AppendLine($"{Indentation4}        {{");
        result.AppendLine($"{Indentation4}            // worRecid");
        result.AppendLine($"{Indentation4}            \"targets\": [0],");
        result.AppendLine($"{Indentation4}            \"visible\": false,");
        result.AppendLine($"{Indentation4}            \"searchable\": false");
        result.AppendLine($"{Indentation4}        }}");
        result.AppendLine($"{Indentation4}    ],");
        result.AppendLine();
        result.AppendLine($"{Indentation4}\"columns\": [");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worRecid\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worWorkorder\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}// {{ \"data\": \"worCompany\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}// {{ \"data\": \"worErpCode\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worItem\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worRemainQty\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worSchedQty\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worStandQty\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}{{ \"data\": \"worStatus\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}// {{ \"data\": \"worProdpool\", \"autoWidth\": true }},");
        result.AppendLine($"{Indentation5}// {{ \"data\": \"worStockavailable\", \"autoWidth\": true }},");
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
        result.AppendLine($"{Indentation}<form method=\"get\"");
        result.AppendLine($"{Indentation2}  asp-area=\"Data\"");
        result.AppendLine($"{Indentation2}  asp-action=\"AddEditData\"");
        result.AppendLine($"{Indentation2}  asp-controller=\"{table.Name.ToPascalCase()}\"");
        result.AppendLine($"{Indentation2}  data-ajax=\"true\"");
        result.AppendLine($"{Indentation2}  data-ajax-method=\"GET\"");
        result.AppendLine($"{Indentation2}  data-ajax-mode=\"replace\"");
        result.AppendLine($"{Indentation2}  data-ajax-update=\"#placeholderAddEditData\">");
        result.AppendLine($"{Indentation2}<button class=\"btn btn-sm btn-primary mb-2\" data-toggle=\"modal\" data-target=\"#addEditData\" onclick=\"EditData()\">New</button>");
        result.AppendLine($"{Indentation}</form>");
        result.AppendLine($"}}");
        result.AppendLine();
        result.AppendLine($"<div class=\"modal fade\" id=\"addEditData\">");
        result.AppendLine($"    <div class=\"modal-dialog\">");
        result.AppendLine($"        <div class=\"modal-content\">");
        result.AppendLine($"            <div id=\"placeholderAddEditData\">");
        result.AppendLine($"            </div>");
        result.AppendLine($"        </div>");
        result.AppendLine($"    </div>");
        result.AppendLine($"</div>");
        result.AppendLine();
        result.AppendLine($"<table id=\"viewGrid\" class=\"table table-striped dt-responsive mb-2\" cellspacing=\"0\">");
        result.AppendLine($"    <thead>");
        result.AppendLine($"        <tr>");
        result.AppendLine($"            <th>worRecid</th>");
        result.AppendLine($"            <th>Workorder <input type=\"text\" placeholder=\"Workorder\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
        result.AppendLine($"            @* <th>Company<input type=\"text\" placeholder=\"Company\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th> *@");
        result.AppendLine($"            @* <th>ERP<input type=\"text\" placeholder=\"ERP\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th> *@");
        result.AppendLine($"            <th>Item<input type=\"text\" placeholder=\"Item\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
        result.AppendLine($"            <th>Remain Qty<input type=\"text\" placeholder=\"Remain Qty\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
        result.AppendLine($"            <th>Sched Qty<input type=\"text\" placeholder=\"Sched Qty\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
        result.AppendLine($"            <th>Stand Qty<input type=\"text\" placeholder=\"Stand Qty\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
        result.AppendLine($"            <th>Status<input type=\"text\" placeholder=\"Status\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th>");
        result.AppendLine($"            @* <th>Prodpool<input type=\"text\" placeholder=\"Prodpool\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th> *@");
        result.AppendLine($"            @* <th>Stock Available<input type=\"text\" placeholder=\"Stock Available\" class=\"fieldSearch\" onclick=\"event.stopPropagation();\" /></th> *@");
        result.AppendLine($"            @if ((bool)ViewData[ViewDataCostants.CanManage])");
        result.AppendLine($"            {{");
        result.AppendLine($"                <th class=\"no-sort\"></th>");
        result.AppendLine($"                <th class=\"no-sort\"></th>");
        result.AppendLine($"            }}");
        result.AppendLine($"        </tr>");
        result.AppendLine($"    </thead>");
        result.AppendLine($"</table>");

        return result.ToString();
    }

    public static string GetPartialViewClass(this Table table,
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
        result.AppendLine($"{Indentation}@if (Model == null)");
        result.AppendLine($"{Indentation}{{");

        result.AppendLine($"{Indentation2}<h4 class=\"modal-title\">");
        result.AppendLine($"{Indentation3}Add {modelName}");
        result.AppendLine($"{Indentation2}</h4>");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine($"{Indentation}else");
        result.AppendLine($"{Indentation}{{");

        result.AppendLine($"{Indentation2}<h4 class=\"modal-title\">");
        result.AppendLine($"{Indentation3}Edit {modelName}");
        result.AppendLine($"{Indentation2}</h4>");
        result.AppendLine($"{Indentation}}}");
        result.AppendLine($"</div>");
        result.AppendLine();
        result.AppendLine($"<form method=\"post\"");
        result.AppendLine($"{Indentation} asp-action=\"SaveData\"");
        result.AppendLine($"{Indentation} asp-controller=\"Workorders\"");
        result.AppendLine($"{Indentation} data-ajax=\"true\"");
        result.AppendLine($"{Indentation} data-ajax-method=\"POST\"");
        result.AppendLine($"{Indentation} data-ajax-begin=\"OnBegin\"");
        result.AppendLine($"{Indentation} data-ajax-failure=\"OnFailure\"");
        result.AppendLine($"{Indentation} data-ajax-success=\"OnSuccess\"");
        result.AppendLine($"{Indentation} data-ajax-complete=\"OnComplete\"");
        result.AppendLine($"{Indentation} class=\"needs-validation\"");
        result.AppendLine($"{Indentation} novalidate>");
        var requiredColumns = table.Columns.Where(c => c.Name.EndsWith("_COMPANY") || c.Name.EndsWith("_ERP_CODE")).ToArray();
        foreach (var column in requiredColumns)
        {
            result.AppendLine($"{Indentation}@Html.HiddenFor(model => model.{column.Name.ToPascalCase()})");
        }
        foreach (var column in table.WritableColumns)
        {
            result.AppendLine($"{Indentation}<div class=\"form-group\" id=\"form{column.Name.ToPascalCase()}\">");
            result.AppendLine($"{Indentation2}<label for=\"{column.Name.ToPascalCase()}\">{column.Name.ToPascalCase()[3..]}</label>");
            if (column.IsNullable)
            {
                result.AppendLine($"{Indentation2}@Html.EditorFor(model => model.{column.Name.ToPascalCase()}, new {{ htmlAttributes = new {{ @class = \"form-control\", placeholder = \"{column.Name.ToPascalCase()[3..]}\" }} }})");
            }
            else
            {
                result.AppendLine($"{Indentation2}@Html.EditorFor(model => model.{column.Name.ToPascalCase()}, new {{ htmlAttributes = new {{ @class = \"form-control\", placeholder = \"{column.Name.ToPascalCase()[3..]}\", required = \"required\" }} }})");
                result.AppendLine($"{Indentation2}<div class=\"invalid-feedback\">");
                result.AppendLine($"{Indentation3}Please enter {column.Name.ToPascalCase()[3..]}");
                result.AppendLine($"{Indentation2}</div>");
            }
            result.AppendLine($"{Indentation}</div>");
        }
        //    @Html.HiddenFor(model => model.WorRecid)
        //    @Html.HiddenFor(model => model.WorCompany)
        //    @Html.HiddenFor(model => model.WorErpCode)
        result.AppendLine($"{Indentation}<div class=\"modal-body\">");
        //        <div class=\"form-group\" id=\"formWorWorkorder\">
        //            <label for=\"WorWorkorder\">Workorder</label>
        //            @Html.EditorFor(model => model.WorWorkorder, new { htmlAttributes = new { @class = \"form-control\", placeholder = \"Workorder\", required = \"required\" } })
        //            <div class=\"invalid-feedback\">
        //                Please enter Workorder
        //            </div>
        //        </div>
        //        <div class=\"form-group\" id=\"formWorItem\">
        //            <label for=\"WorItem\">Item</label>
        //            @Html.EditorFor(model => model.WorItem, new { htmlAttributes = new { @class = \"form-control\", placeholder = \"Item\" } })
        //        </div>
        result.AppendLine($"{Indentation}</div>");
        result.AppendLine($"{Indentation}<div class=\"modal-footer\">");
        result.AppendLine($"{Indentation2}<button type=\"submit\" class=\"btn btn-primary\" data-save=\"modal\">Save</button>");
        result.AppendLine($"{Indentation2}<button type=\"button\" class=\"btn btn-secondary\" data-dismiss=\"modal\">Cancel</button>");
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
