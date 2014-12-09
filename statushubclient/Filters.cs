using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace statushubclient
{
    public class Filter
    {
        public int id { get; set; }
        public string name { get; set; }
        public string updated_at { get; set; }
        public bool up { get; set; }
    }
}
