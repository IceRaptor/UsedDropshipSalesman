
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsedDropshipSalesman
{

    public static class ModStats
    {
    }

    public class ModConfig
    {

        // If true, many logs will be printed
        public bool Debug = false;
        // If true, all logs will be printed
        public bool Trace = false;

        public void LogConfig()
        {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG:{this.Debug} Trace:{this.Trace}");


            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            Mod.Log.Debug?.Write(" == Initializing Configuration");


            Mod.Log.Debug?.Write(" == Configuration Initialized");
        }

    }
}
