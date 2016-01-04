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

namespace EventService
{
    /// <summary>
    /// RADIO API
    /// </summary>

    public class RadioController : ApiController
    {
        public RadioController()
        {
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.UseDataContractJsonSerializer = true;
        }

        [System.Web.Http.AcceptVerbs("GET")]        
        public HttpResponseMessage Users()
        {
            IHttpRouteData rd = Request.GetRouteData();
            List<KeyValuePair<string, string>> keys = Request.GetQueryNameValuePairs().ToList();
            
            string format = HttpTools.GetFormat(keys, "format");
            if (format == null)
                format = "json";

            Users users = CoreData.Radio.GetUsers();
            HttpResponseMessage msg = null;
            if (format == "json")
                msg = Request.CreateResponse(HttpStatusCode.OK, users, "application/json");
            else
                msg = Request.CreateResponse(HttpStatusCode.OK, users, "application/xml");

            // naformatovaane jako xml
            //HttpResponseMessage msg = Request.CreateResponse(HttpStatusCode.OK, cars, "application/xml");          
            return msg;           
        }

        [System.Web.Http.AcceptVerbs("GET")]        
        public HttpResponseMessage Stations()
        {
            IHttpRouteData rd = Request.GetRouteData();
            List<KeyValuePair<string, string>> keys = Request.GetQueryNameValuePairs().ToList();

            string format = HttpTools.GetFormat(keys, "format");
            if (format == null)
                format = "json";

            Stations users = CoreData.Radio.GetStations();
            HttpResponseMessage msg = null;
            if (format == "json")
                msg = Request.CreateResponse(HttpStatusCode.OK, users, "application/json");
            else
                msg = Request.CreateResponse(HttpStatusCode.OK, users, "application/xml");

            // naformatovaane jako xml
            //HttpResponseMessage msg = Request.CreateResponse(HttpStatusCode.OK, cars, "application/xml");          
            return msg;
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage Machines()
        {            
            List<KeyValuePair<string, string>> keys = Request.GetQueryNameValuePairs().ToList();

            string format = HttpTools.GetFormat(keys, "format");
            if (format == null)
                format = "json";

            Machines m = CoreData.Radio.GetMachines();
            HttpResponseMessage msg = null;
            if (format == "json")            
                msg = Request.CreateResponse(HttpStatusCode.OK, m, "application/json");            
            else
                msg = Request.CreateResponse(HttpStatusCode.OK, m, "application/xml");

            // naformatovaane jako xml
            //HttpResponseMessage msg = Request.CreateResponse(HttpStatusCode.OK, cars, "application/xml");          
            return msg;
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public HttpResponseMessage Options(string app)
        {
            List<KeyValuePair<string, string>> keys = Request.GetQueryNameValuePairs().ToList();

            string format = HttpTools.GetFormat(keys, "format");
            if (format == null)
                format = "json";


            Options o = CoreData.Radio.GetOptions(app);
            HttpResponseMessage msg = null;
            if (format == "json")
            {
                /* 
                MemoryStream stream1 = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Options));
                ser.WriteObject(stream1, o);
                */
                string json = new JavaScriptSerializer(new TypeResolver()).Serialize(o);
                //object obj = new JavaScriptSerializer(new TypeResolver()).Deserialize<Options>(json);

                msg = Request.CreateResponse(HttpStatusCode.OK,json, "application/json");                                
                //msg = Request.CreateResponse(HttpStatusCode.OK, o, "application/json");                                
            }
            else
            {
                msg = Request.CreateResponse(HttpStatusCode.OK, o, "application/xml");
            }

            return msg;
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public string Login(string user, string pass)
        {
            string token = CoreData.Radio.Login(user, pass);
            return token;
            
        }
    }

}