﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitterLib;
using TwitterLib.CommandLineParser;

namespace TwitterReader
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Type> verbs = new List<Type>() { 
                typeof(Sample),
                typeof(World),
                typeof(Chunk),
                typeof(Follower),
            };
            
            Verb v = null;

            try
            {
                PrintHeader();
                v = (Verb)ArgumentParser.Parse(args, verbs);
            }
            catch (ArgumentParserException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine();

                ArgumentParser.PrintUsage(verbs, Console.Out);
            }

            if (v != null)
            {
                v.Run();
            }
        }

        private static void PrintHeader()
        {
            Console.WriteLine(
@"Twitter stream dump utility
(c) 2012 László Dobos dobos@complex.elte.hu
Department of Physics of Complex Systems, Eötvös Loránd University

");
        }
    }
}
