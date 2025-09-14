using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Helper
{
    public static class DbConnectionHelper
    {
        public static string? ConnectionString { get; set; }

        public static string? TargetConnectionString { get; set; }
        public static string? CompareToConnectionString { get; set; }

        public static string? TargetDatabase { get; set; } = null;
        public static string? SourceDatabase { get; set; } = null;

    }
}
