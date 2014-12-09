using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace statushubclient
{
    public class Utilities
    {
        public static bool Match(string origninal, string searchstring) {
            if (System.Text.RegularExpressions.Regex.IsMatch(origninal.ToLower(), "(" + searchstring + ")") || origninal.Contains(searchstring))
                {
                    return true;
                }
            else
                {
                    return false;
                }
        }
    }
}

namespace RestSharp.Deserializers
{
    public class DynamicJsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string RootObject { get; set; }
        public string Data { get; set; }
        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<dynamic>(response.Content);
        }
    }
}
