using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmazonApi.Core
{
    public class AmazonException : Exception
    {
        public IEnumerable<string> Messages { get; set; }
        public override string Message { get { return Messages.FirstOrDefault(); } }
    }
}
