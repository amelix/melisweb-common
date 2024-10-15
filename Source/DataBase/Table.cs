﻿using System.Text.Json.Serialization;

namespace MelisWeb.Common.DataBase;

public class Table
{
    [JsonRequired]
    public string DatabaseName { get; set; }
    public string Schema { get; set; }
    public string Name { get; set; }
    public List<Column> Columns { get; set; }

    public string? AdditionalFilter { get; set; }

    public List<Table> Childs { get; set; }

    [JsonIgnore]
    public List<Column> PrimaryKeyColumns => Columns.Where(c => c.IsPrimaryKey).ToList();
    [JsonIgnore]
    public List<Column> WritableColumns => Columns.Where(c => !c.IsIdentity).ToList();
    [JsonIgnore]
    public Column? IdentityColumn => Columns.Where(c => c.IsIdentity).FirstOrDefault();
    [JsonIgnore]
    public List<Column> UpdatableColumns => WritableColumns.Where(c => !c.IsPrimaryKey).ToList();
}
