using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Xml;
using System.Web.Http.Routing;
using System.Web.Mvc;
using ServerData;
using System.Xml.Serialization;
using ServerData.Tools;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using ServerData.JSON;
using System.Text;
using Log;
using System.ComponentModel.Composition;

namespace ServerData
{
    public class HttpController : ApiController
    {

        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage Get()
        {
            StringBuilder output = new StringBuilder();

            List<KeyValuePair<string, string>> keys = Request.GetQueryNameValuePairs().ToList();
            foreach (KeyValuePair<string, string> kv in keys)
            {
                output.AppendLine("HTTP Receive:"+string.Format("{0}={1}", kv.Key, kv.Value));
            }

            CoreData.Log.Info(output.ToString());

            HttpResponseMessage msg = new HttpResponseMessage();
            msg = Request.CreateResponse(HttpStatusCode.OK, output.ToString(),"text/*");
            return msg;
        }
    }
}
