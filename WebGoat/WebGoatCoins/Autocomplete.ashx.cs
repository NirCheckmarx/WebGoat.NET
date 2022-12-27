using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.WebUtility;
using System.Text.RegularExpressions;
using System.Data;
using OWASP.WebGoat.NET.App_Code;
using OWASP.WebGoat.NET.App_Code.DB;

namespace OWASP.WebGoat.NET.WebGoatCoins
{
    /// <summary>
    /// Summary description for Autocomplete
    /// </summary>
    public class Autocomplete : IHttpHandler
    {
    
    
        private IDbProvider du = Settings.CurrentDbProvider;

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");

            string query = context.Request["query"];
            
            DataSet ds = du.GetCustomerEmails(query);
            string json = Encoder.ToJSONSAutocompleteString(query, ds.Tables[0]);

            if (json != null && json.Length > 0)
            {
                context.Response.ContentType = "text/plain";
                string safeJson = SanitizeJson(json);
                context.Response.Write(safeJson);
            }
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("");
            
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private static string SanitizeJson(string UnsafeJsonString)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";
            const string stripFormatting = @"<[^>]*(>|$)";
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);
            var safeJsonString = UnsafeJsonString;
            safeJsonString = System.Net.WebUtility.HtmlDecode(safeJsonString);
            safeJsonString = tagWhiteSpaceRegex.Replace(safeJsonString, "><");
            safeJsonString = lineBreakRegex.Replace(safeJsonString, Environment.NewLine);
            safeJsonString = stripFormattingRegex.Replace(safeJsonString, string.Empty);
            return safeJsonString;

        }
    }
}