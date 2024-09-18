using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase;

public class Column
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int MaxLength { get; set; }
    public int Precision { get; set; }
    public int Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public int Index { get; set; }
    public bool IsIdentity { get; set; }

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
        else
        {
            return Type;
        }
    }
}
