using System.Text.Json.Serialization;

namespace MelisWeb.Common.DataBase;

public class Table
{
    [JsonRequired]
    public string DatabaseName { get; set; }
    public string Schema { get; set; }
    public string Name { get; set; }
    public List<Column> Columns { get; set; }

    public List<Table> Childs { get; set; }

    public List<Column> PrimaryKeyColumns => Columns.Where(c => c.IsPrimaryKey).ToList();
    public List<Column> WritableColumns => Columns.Where(c => !c.IsIdentity).ToList();
    public List<Column> UpdatableColumns => WritableColumns.Where(c => !c.IsPrimaryKey).ToList();
}
