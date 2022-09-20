using System;
using System.Collections.Generic;
using System.Text;

namespace ZD365DT.DeploymentTool.Utils
{
    public class SqlScriptParser
    {
        public static string[] SplitSqlQueryOnGo(string queryString)
        {
            List<string> result = new List<string>();

            string[] queryLines = queryString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder scriptPart = new StringBuilder();
            foreach (string queryLine in queryLines)
            {
                if (queryLine.Trim().Equals("GO", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Add(scriptPart.ToString());
                    scriptPart = new StringBuilder();
                }
                else
                {
                    scriptPart.AppendLine(queryLine);
                }
            }
            if (scriptPart.ToString().Trim() != String.Empty)
            {
                result.Add(scriptPart.ToString());
            }
            return result.ToArray();
        }
    }
}
