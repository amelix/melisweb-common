﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelisWeb.Common.DataBase
{
    public class Property
    {
        public string Name { get; set; }

        public bool Visible { get; set; } = true;
        
        public bool VisibleInCreate { get; set; } = true;

        public bool Required{ get; set; } = false;

        public string? DefaultValue { get; set; }

        public string? Label { get; set; }

        public Column Database { get; set; }
    }
}
