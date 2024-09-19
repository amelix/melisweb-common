using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase;

public static class ExtensionsCode
{
    public static string Indentation = "\t";

    #region C# Extensions

    public static string GetEntityClass(this Table table, string classNamespace, IEnumerable<string>? UsingList = null)
    {
        var result = new StringBuilder();
        if (UsingList != null)
        {
            foreach (var item in UsingList)
            {
                result.AppendLine($"using {item};");
            }
        }
        //result.AppendLine($"using System;");
        //result.AppendLine($"using System.Collections.Generic;");
        result.AppendLine();
        result.AppendLine($"namespace {classNamespace};");
        result.AppendLine();
        result.AppendLine($"public class {table.Name.ToPascalCase().ToSingular()} : BaseEntity");
        result.AppendLine("{");
        result.AppendLine($"{Indentation}#region Properties");
        result.AppendLine();
        result.AppendLine(table.GetProperties(Indentation));
        //result.AppendLine();
        result.AppendLine($"{Indentation}#endregion");
        result.AppendLine("}");
        return result.ToString();
    }

    public static string GetDataClass(this Table table, string classNamespace, IEnumerable<string>? UsingList = null)
    {
        var result = new StringBuilder();
        if (UsingList != null)
        {
            foreach (var item in UsingList)
            {
                result.AppendLine($"using {item};");
            }
        }
        //result.AppendLine($"using System;");
        //result.AppendLine($"using System.Collections.Generic;");
        result.AppendLine();
        result.AppendLine($"namespace {classNamespace};");
        result.AppendLine();
        result.AppendLine($"public class {table.Name.ToPascalCase().ToSingular()} : BaseData");
        result.AppendLine("{");
        
        result.AppendLine("}");

        return result.ToString();
    }

    #region Private Methods

    private static string GetProperties(this Table table, string indentation = "")
    {
        var result = new StringBuilder();
        foreach (var column in table.Columns)
        {
            result.AppendLine($"{indentation}public {column.GetCodeDataType()} {column.Name.ToPascalCase()} {{ get; set; }}");
        }

        return result.ToString();
    }

    #endregion

    #endregion
}
