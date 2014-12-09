using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices;
using NetOffice.OutlookApi;
using Microsoft.Exchange.WebServices.Data;
using System.Net;
using RestSharp;
using RestSharp.Deserializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace statushubclient
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    // exchange service connector set for the lowest level you want to support
                    ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                    service.Credentials = new NetworkCredential("statusupdates", "", "somecompany.local"); // ugg i know a username and password in plain text, well thats ok just go ahead and send some emails as stausupdates and see if i care
                    service.AutodiscoverUrl("statusupdates@somecompany.com");
                    PropertySet itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties);
                    itempropertyset.RequestedBodyType = BodyType.Text;

                    // pull 10 at a time cuz well i dont know, i just felt like 10 was a good number for now
                    ItemView itemview = new ItemView(10);
                    itemview.PropertySet = itempropertyset;
                    FindItemsResults<Item> items = service.FindItems(WellKnownFolderName.Inbox, itemview);
                    if (items.TotalCount != 0)
                    {
                        foreach (EmailMessage item in items)
                        {
                            // load exchange email message
                            item.Load();
                            item.Load(itempropertyset);

                            // clean the body of html and other elements that cause issues
                            string eventbody = Regex.Replace(Regex.Replace(item.Body.ToString().Replace(System.Environment.NewLine, " "), @"<[^>]+>|&nbsp;", "").Trim(), @"\s{2,}", " "); // replace newline with spaces, remove all HTML and then trim double spaces 
                            
                            // clean the title for better presentation
                            string cleanedtitle = (Regex.Replace(Regex.Replace(Regex.Replace(item.Subject.ToString().Replace(System.Environment.NewLine, " "), @"<[^>]+>|&nbsp;|\[(.*?)\]|\*+|([0-9]{1,3}[\.]){3}[0-9]{1,3}|\b(\w+)\s+\1\b|\]|\[|\(|\)|-", " ").Trim(), @"\s{2,}", " "), @"\b(\w+)\s+\1\b", "")).Replace(",", " ").ToLower(); // remove all HTML, remove duplicate text, remove brackets and other unneeded chars

                            // check for the clear the board sequence
                            if (Utilities.Match(cleanedtitle, "clear the board please"))
                            {
                                ApiCall.ClearTheBoard();
                            }

                            // check for ipmon emails that fit the listed match rules
                            if (Utilities.Match(item.From.Name.ToString(), "ipmon@somecompany.com"))
                            {
                                // parse the match rules json file
                                JObject json = JObject.Parse(File.ReadAllText((@"MatchRules.json")));
                                JArray matchrules = (JArray)json["matchrules"];
                                foreach (JToken matchrule in matchrules)
                                {
                                    if (matchrule != null)
                                    {
                                        if (Utilities.Match(cleanedtitle, (string)matchrule["matchstring"]))
                                        {
                                            string serviceid = ApiCall.GetServiceId((string)matchrule["servicestring"]);
                                            if (serviceid != null)
                                            {
                                                // if we found a service id and the event is a down event
                                                if (Utilities.Match(cleanedtitle, (string)matchrule["matchdown"]))
                                                {
                                                    // prepare and call the post response
                                                    string title = (string)matchrule["overridetitle"] != null ? cleanedtitle : (string)matchrule["overridetitle"];
                                                    string body = (string)matchrule["overridebody"] != null ? cleanedtitle.Replace("down", "experiencing issues") + ", we are evaluating this further and will update as soon as we know more. Please contact itsupport@somecompany.com if you you have issues or information reguarding this event." : (string)matchrule["overridebody"];
                                                    string postresponse = ApiCall.CallPostStatusHub("status_pages/somecompany/services/" + serviceid + "/incidents", (string)matchrule["servicestring"], (string)matchrule["downstatustring"], title, body, (string)matchrule["downincidenttypestring"]);
                                                }

                                                // if we found a service id and the event is a up event
                                                if (Utilities.Match(cleanedtitle, (string)matchrule["matchup"]) || Utilities.Match(cleanedtitle, "has recovered"))
                                                {
                                                    // prepare and call the post response
                                                    var eventidtoupdate = ApiCall.GetOpenIssueIdThatMatchStringBest(cleanedtitle);
                                                    string title = (string)matchrule["overridetitle"] != null ? cleanedtitle : (string)matchrule["overridetitle"];
                                                    string body = (string)matchrule["overridebody"] != null ? cleanedtitle.Replace("down", "working normally") + ", we have resolved this event. Please contact itsupport@somecompany.com if you you have further issues reguarding this." : (string)matchrule["overridebody"];
                                                    ApiCall.CallIncidentUpdate("status_pages/somecompany/services/" + eventidtoupdate.Item2 + "/incidents/" + eventidtoupdate.Item1 + "/incident_updates", (string)matchrule["upstatustring"], body, (string)matchrule["upincidenttypestring"]);
                                                }
                                            }
                                        }
                                    }
                                }

                                // delete the email
                                item.Delete(DeleteMode.MoveToDeletedItems);
                            }
                            else
                            {
                                // for emails from humans we do fuzy matching as humans are not robots and text can be... interesting
                                int currentbestmatch = 30000; // this is a false value so that we can look for the lowest levenstein distance value returned in the below function
                                string match = null;
                                List<string> serviceslist = ApiCall.GetServiceNameList();
                                foreach (string x in serviceslist)
                                {
                                    string normalizedx = x.ToLower().Trim();
                                    string normalisedsubject = cleanedtitle.ToLower();

                                    // check to see if it is a match
                                    if (normalisedsubject.Contains(normalizedx))
                                    {
                                        // get fuzzy match level
                                        int matchlevel = LevenshteinDistance.Compute(normalizedx, normalisedsubject);
                                        if (matchlevel < currentbestmatch)
                                        {
                                            match = x;
                                            currentbestmatch = matchlevel;
                                        }
                                    }
                                }
                                if (match != null)
                                {
                                    // lookup the service id from the string provided
                                    string serviceid = ApiCall.GetServiceId(match);
                                    
                                    // particular functions for particular states because im a particular person with particularly bad coding pricipals
                                    if (Utilities.Match(cleanedtitle, "down"))
                                    {
                                        string postresponse = ApiCall.CallPostStatusHub("status_pages/somecompany/services/" + serviceid + "/incidents", match, "down", item.Subject, eventbody, "investigating");
                                    }
                                    if (Utilities.Match(cleanedtitle, "up"))
                                    {
                                        string postresponse = ApiCall.CallPostStatusHub("status_pages/somecompany/services/" + serviceid + "/incidents", match, "up", item.Subject, eventbody, "resolved");
                                    }
                                    if (Utilities.Match(cleanedtitle, "degraded"))
                                    {
                                        string postresponse = ApiCall.CallPostStatusHub("status_pages/somecompany/services/" + serviceid + "/incidents", match, "degraded-performance", item.Subject, eventbody, "investigating");
                                    }
                                }

                                // delete the email
                                item.Delete(DeleteMode.MoveToDeletedItems);
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(100 * 1);
                }
                catch { } // dont judge the pokemon...
            }
        }
    }
}