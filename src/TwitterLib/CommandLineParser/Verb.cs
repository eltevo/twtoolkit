﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace TwitterLib.CommandLineParser
{
    public abstract class Verb
    {
        public abstract void Run();
    }
}
