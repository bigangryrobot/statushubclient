using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace statushubclient
{
    public class ApiCall
    {
        public static JArray CallGetStatusHubElements(string requesturi)
        {
            // connect to the api
            var client = new RestClient("https://statushub.io/api/");
            var request = new RestRequest(requesturi + "?api_key=", Method.GET);
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);

            // data returned as dynamic object
            JObject json = JObject.Parse(response.Content);
            JArray things = (JArray)json["services"];
            return things;
        }
        public static string CallPostStatusHub(string requesturi, string servicename, string servicestatus, string eventtitle, string eventbody, string incidenttype)
        {

            // connect to the api
            var client = new RestClient("https://statushub.io/api/");
            var request = new RestRequest(requesturi + "?api_key=", Method.POST);
            request.RequestFormat = DataFormat.Json;

            // custom json body param
            string requestparam = "{\"incident\": { \"title\": \"" + eventtitle.Trim() + "\", \"update\" :{\"body\": \"" + eventbody.Trim() + "\", \"incident_type\":\"" + incidenttype.Trim() + "\"}}, \"service_status\": {\"status_name\": \"" + servicestatus.Trim() + "\"} }";
            request.AddParameter("Application/Json", requestparam, ParameterType.RequestBody);

            // get and prepare the response
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            Console.WriteLine(content);
            return content;
        }
        public static string CallIncidentUpdate(string requesturi, string servicestatus, string eventbody, string incidenttype)
        {
            // connect to the api
            var client = new RestClient("https://statushub.io/api/");
            var request = new RestRequest(requesturi + "?api_key=", Method.POST);
            request.RequestFormat = DataFormat.Json;
            
            // custom json body param
            string requestparam = "{\"incident_update\": { \"body\": \"" + eventbody.Trim() + "\", \"incident_type\":\"" + incidenttype.Trim() + "\"}, \"service_status\": {\"status_name\": \"" + servicestatus.Trim() + "\"} }";
            request.AddParameter("Application/Json", requestparam, ParameterType.RequestBody);

            // get and prepare the response
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            Console.WriteLine(content);
            return content;
        }
        public static string GetServiceId(string servicetobefound)
        {
            // define default service id so we can catch empty search
            string id = "0";

            // get elements and search for match
            JArray services = CallGetStatusHubElements("status_pages/somecompany");
            foreach (JToken service in services)
            {
                if ((string)service["name"] == servicetobefound.Trim())
                {
                    id = ((string)service["id"]);
                }
            }
            return id;
        }
        public static Tuple<int, int> GetOpenIssueIdThatMatchStringBest(string emailsubject)
        {
            // get the elements
            JArray serviceids = CallGetStatusHubElements("status_pages/somecompany");
            int currentbestmatch = 30000; // this is a false value so that we can look for the lowest levenstein distance value returned in the below function
            int matchid = 0;
            string matchtitle = "";
            string matchx = "";
            int matchserviceid = 0;
            foreach (JToken serviceid in serviceids)
            {
                // search for invidents that a service id has open
                var client = new RestClient("https://statushub.io/api/");
                var request = new RestRequest("status_pages/somecompany/services/" + ((string)serviceid["id"]) + "/incidents" + "?api_key=", Method.GET);
                request.RequestFormat = DataFormat.Json;

                // get and prepare the response
                IRestResponse response = client.Execute(request);
                JArray incidents = JArray.Parse(response.Content);
                foreach (JToken incident in incidents)
                {
                    // using linq look for the most recent event that contians resolved as they seem to be listed in order
                    JToken match = incident["incident_updates"].FirstOrDefault(j => j["incident_type"].ToString().Equals("resolved"));
                    if (match == null)
                    {

                        // get fuzzy match level aka edit distance between these strings
                        int matchlevel = LevenshteinDistance.Compute(emailsubject.ToString().ToLower(), ((string)incident["title"]).ToLower().Trim());
                        if (matchlevel < currentbestmatch)
                        {
                            // when we do have a best match set the return variables
                            matchid = (int)incident["id"];
                            matchserviceid = (int)serviceid["id"];
                            matchx = (string)incident["title"];
                            matchtitle = (string)incident["title"];
                            currentbestmatch = matchlevel;
                        }
                    }
                }
            }
            // its a tuple because im to lazy to create a real object...
            return Tuple.Create(matchid, matchserviceid);
        }
        public static void ClearTheBoard()
        {
            // get the service ids
            JArray serviceids = CallGetStatusHubElements("status_pages/somecompany");
            foreach (JToken serviceid in serviceids)
            {
                // find the open incidents for this service id
                var client = new RestClient("https://statushub.io/api/");
                var request = new RestRequest("status_pages/somecompany/services/" + ((string)serviceid["id"]) + "/incidents" + "?api_key=", Method.GET);
                request.RequestFormat = DataFormat.Json;

                // get and prepare the response
                IRestResponse response = client.Execute(request);
                JArray incidents = JArray.Parse(response.Content);
                foreach (JToken incident in incidents)
                {
                    // linq search for incident type of resolved as we dont want to have an endless loop party again, it was far to messy
                    JToken match = incident["incident_updates"].FirstOrDefault(j => j["incident_type"].ToString().Equals("resolved"));
                    if (match == null)
                    {
                        // if we have a match of a service not in resolution state then set it to resolved
                        string postresponse = CallIncidentUpdate("status_pages/somecompany/services/" + (int)serviceid["id"] + "/incidents/" + (int)incident["id"] + "/incident_updates", "up", "resolved", "resolved");
                    }
                }
            }
        }
        public static List<string> GetServiceNameList()
        {
            // generate list of services
            List<string> list = new List<string>();
            JArray services = CallGetStatusHubElements("status_pages/somecompany");
            foreach (JToken service in services)
            {
                list.Add((string)service["name"]);
            }
            return list;
        }

        //public static string CallDeleteStatusHub(string requesturi)
        //{
        //    var client = new RestClient("https://statushub.io/api/");
        //    var request = new RestRequest(requesturi + "?api_key=", Method.DELETE);
        //    request.RequestFormat = DataFormat.Json;
        //    IRestResponse response = client.Execute(request);
        //    var content = response.Content;

        //    return content;
        //}
        //public static string CallGetStatusHub(string requesturi)
        //{
        //    var client = new RestClient("https://statushub.io/api/");
        //    var request = new RestRequest(requesturi + "?api_key=", Method.GET);
        //    request.RequestFormat = DataFormat.Json;
        //    IRestResponse response = client.Execute(request);
        //    var content = response.Content;
        //    Console.Wr    iteLine(content);
        //    return content;
        //}
        //public static string CallSetStatusStatusHub(string requesturi, string servicestatus)
        //{
        //    var client = new RestClient("https://statushub.io/api/");
        //    var request = new RestRequest(requesturi + "?api_key=", Method.POST);
        //    request.RequestFormat = DataFormat.Json;

        //    string requestparam = "{\"service_status\": {\"status_name\": \"" + servicestatus.Trim() + "\"} }";
        //    Console.WriteLine(requestparam);
        //    request.AddParameter("Application/Json", requestparam, ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);
        //    var content = response.Content;
        //    Console.WriteLine(content);
        //    return content;
        //}
    }
}
