using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase
{
    public class Property : Column
    {
        public bool Visible { get; set; } = true;

        public string? Label { get; set; }
    }
}
