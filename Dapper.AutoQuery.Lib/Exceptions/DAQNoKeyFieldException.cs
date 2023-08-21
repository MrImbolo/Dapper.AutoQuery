using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.AutoQuery.Lib.Exceptions
{
    public class DAQNoKeyFieldException : Exception
    {
        public DAQNoKeyFieldException() : base("Entity has no key configured")
        {
            
        }
    }
    public class DAQNotConfiguredException : Exception
    {
        public DAQNotConfiguredException() : base("Dapper.AutoQuery.Lib.Config was not configured")
        {
            
        }
    }
}
