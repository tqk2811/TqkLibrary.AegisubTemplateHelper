using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Aegisub.Exceptions
{
    public class AegisubProcessException : Exception
    {
        public required int ExitCode { get; set; }
        public required string StdOut { get; set; }
        public required string StdErr { get; set; }

        public override string Message
        {
            get
            {
                return JsonConvert.SerializeObject(new
                {
                    ExitCode,
                    StdOut,
                    StdErr
                });
            }
        }
    }
}
