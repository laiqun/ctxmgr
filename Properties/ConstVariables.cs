using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctxmgr.Properties
{
    public class ConstVariables
    {
        public static string INSERT_LINE_TEXT = "chars('-', 80)";
        public static string INSERT_DATE_TEXT = "yyyy-MM-dd HH:mm:ss:fff";
        public static string INSERT_LINE_DATE_TEXT = "chars('-', 29) dateTime() chars('-', 30)";
    }
}
