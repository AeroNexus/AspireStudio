using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Core
{
    /// <summary>
    /// Yeah this isn't great. I couldn't quickly figure out how to inject
    /// the transport name into the places i needed it. Previously it was
    /// read from an environment variable. 
    /// </summary>
    public static class Config
    {
        public static string TransportName { get; set; }
    }
}
