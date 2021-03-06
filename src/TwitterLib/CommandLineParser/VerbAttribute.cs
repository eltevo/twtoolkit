﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterLib.CommandLineParser
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VerbAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
