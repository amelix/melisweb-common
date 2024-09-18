using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase;

public class Table
{
    public string Name { get; set; }
    public List<Column> Columns { get; set; }

    public List<Column> PrimaryKeyColumns => Columns.Where(c => c.IsPrimaryKey).ToList();
    public List<Column> WritableColumns => Columns.Where(c => !c.IsIdentity).ToList();
    public List<Column> UpdatableColumns => WritableColumns.Where(c => !c.IsPrimaryKey).ToList();
}
