﻿using System;
using System.Text;

namespace OData2PocoLib.V4
{
    public partial class Poco 
   
    {
        public string GeneratePoco()
        {
            var list = GeneratePocoList(); //generate all classes from model
            var textTemplate = new StringBuilder();
            AddUsingNamespace(textTemplate);
            textTemplate.AppendLine("{"); //namespace
          
            foreach (var item in list)
            {
                textTemplate.AppendLine(CsClassToString(item));
            }
            textTemplate.AppendLine("}"); //namespace
            return textTemplate.ToString();
        }

        private void AddUsingNamespace(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Spatial;");
            sb.AppendLine("using System.Collections.Generic;");

            //add comments
            var comment = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated using  OData2Poco.
//Service Url: {0}
//MetaData Version: {1}
{2}
// </auto-generated>
//------------------------------------------------------------------------------
";
            //sb.AppendFormat(comment, _metaDataReader.ServiceUrl, _metaDataReader.MetaDataVersion,
            //    string.IsNullOrEmpty(_metaDataReader.ServiceVersion) ? String.Empty :
            //        "//Service version: " + _metaDataReader.ServiceVersion);

            //sb.AppendFormat("namespace {0}\r\n", _metaDataReader.SchemaNamespace);
        }

   
        private string CsClassToString(ClassTemplate ent)
        {
            var sbClass = new StringBuilder();
            sbClass.AppendFormat("    public class {0}\r\n", ent.Name);
            sbClass.AppendLine("    {");
            foreach (var p in ent.Properties)
            {
                string s = string.Format("\tpublic {0} {1} {{get;set;}}", p.PropType, p.PropName);
                sbClass.AppendLine(s);
            }
            return sbClass.ToString();
        }


    }
}
