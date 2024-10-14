using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase
{
    public class Model
    {
        public string DatabaseName { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }

        public bool Ignore { get; set; } = false;

        public List<Property> Properties { get; set; } = new List<Property>();

        [JsonIgnore]
        public Table Table { get; set; }
        [JsonIgnore]
        public List<Property> PrimaryKeyColumns => Properties.Where(c => c.IsPrimaryKey).ToList();
        [JsonIgnore]
        public List<Property> WritableColumns => Properties.Where(c => !c.IsIdentity).ToList();
        [JsonIgnore]
        public Property? IdentityColumn => Properties.Where(c => c.IsIdentity).FirstOrDefault();
        [JsonIgnore]
        public List<Property> UpdatableColumns => WritableColumns.Where(c => !c.IsPrimaryKey).ToList();
    }
}
