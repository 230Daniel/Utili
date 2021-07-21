﻿using System.Collections.Generic;

namespace UtiliBackend.Models
{
    public class CoreConfigurationModel
    {
        public string Prefix { get; set; }
        public bool CommandsEnabled { get; set; }
        public List<string> NonCommandChannels { get; set; }
    }
}
