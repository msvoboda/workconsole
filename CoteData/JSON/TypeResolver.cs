using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ServerData.JSON
{
    public class TypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id)
        {
            if (id == "string")
            {
                return new StringValue().GetType();
            }
            else if (id == "int")
            {
                return new IntValue().GetType();
            }
            else if (id == "bool")
            {
                return new BoolValue().GetType();
            }
            else if (id == "float")
            {
                return new FloatValue().GetType();
            }
            else if (id == "time")
            {
                return new TimeSpan().GetType();
            }
            else if (id == "date")
            {
                return new DateTime().GetType();
            }

            return new object().GetType();
        }

        public override string ResolveTypeId(Type type)
        {
            if (type.Name == "StringValue")
                return "string";
            else if (type.Name == "IntValue")
                return "int";
            else if (type.Name == "BoolValue")
                return "bool";
            else if (type.Name == "FloatValue")
                return "float";
            else if (type.Name == "TimeValue")
                return "time";
            else if (type.Name == "DateValue")
                return "date";

            return "anyType";
        }
    }
}
