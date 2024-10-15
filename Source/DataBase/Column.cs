using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase;

public class Column
{
    public string ColumnName { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int MaxLength { get; set; }
    public int Precision { get; set; }
    public int Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public int Index { get; set; }
    public bool IsIdentity { get; set; }

    public string? AdditionalFilter { get; set; }

    public string GetDataType()
    {
        if (Type == "varchar" || Type == "char")
        {
            return $"{Type}({MaxLength})";
        }
        else if (Type == "nvarchar" || Type == "nchar")
        {
            return $"{Type}({MaxLength / 2})";
        }
        else if (Type == "decimal" || Type == "numeric")
        {
            return $"{Type}({Precision},{Scale})";
        }

        return Type;
    }

    public string GetCodeDataType()
    {
        var nullable = "";
        if (IsNullable)
        {
            nullable = "?";
        }
        if (Type == "varchar" || Type == "char")
        {
            return "string" + nullable;
        }
        else if (Type == "nvarchar" || Type == "nchar")
        {
            return "string" + nullable;
        }
        else if (Type == "decimal" || Type == "numeric")
        {
            return "decimal" + nullable;
        }
        else if (Type == "bigint")
        {
            return "long" + nullable;
        }
        else if (Type == "bit")
        {
            return "bool" + nullable;
        }
        else if (Type == "datetime")
        {
            return "DateTime" + nullable;
        }

        return Type + nullable;
    }

    public string GetCodeConvertDataType()
    {
        if (Type == "varchar" || Type == "char")
        {
            return "Convert.ToString";
        }
        else if (Type == "nvarchar" || Type == "nchar")
        {
            return "Convert.ToString";
        }
        else if (Type == "decimal" || Type == "numeric")
        {
            return "Convert.ToDecimal";
        }
        else if (Type == "int")
        {
            return "Convert.ToInt32";
        }
        else if (Type == "bigint")
        {
            return "Convert.ToInt64";
        }
        else if (Type == "bit")
        {
            return "Convert.ToBoolean";
        }
        else if (Type == "datetime")
        {
            return "Convert.ToDateTime";
        }

        return "Convert.To" + Type;
    }
}
