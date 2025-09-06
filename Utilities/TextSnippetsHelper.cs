using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ctxmgr.Utilities
{
    public class TextSnippetsHelper
    {
        public static string ProccessCharsFunc(string rule)
        {
            // 匹配 chars('x', n) 的模式
            var regex = new Regex(@"chars\('(.{1})',\s*(\d+)\)");

            // 使用 Replace，把匹配的部分替换为重复字符
            string result = regex.Replace(rule, match =>
            {
                char c = match.Groups[1].Value[0];
                int count = int.Parse(match.Groups[2].Value);
                return new string(c, count);
            });

            return result;
        }
        public static string ProccessDateTimeFunc(string rule,string dateTime)
        {
            // 匹配 date('format') 的模式
            var regex = new Regex(@"dateTime\(\)");
            // 使用 Replace，把匹配的部分替换为当前日期的格式化字符串
            string result = regex.Replace(rule, match =>
            {
                return dateTime;
            });
            return result;
        }
        public static string ProcessCharsAndDateTime(string rule,string dateTimeFormat)
        {
            var currentDateTime = DateTime.Now.ToString(dateTimeFormat);
            var withCharsProcessed = ProccessCharsFunc(rule);
            var fullyProcessed = ProccessDateTimeFunc(withCharsProcessed, currentDateTime);
            return fullyProcessed;
        }
    }
}
