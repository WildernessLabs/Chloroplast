using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Tool.Commands
{
    public class WatchCommand : ICliCommand
    {
        public WatchCommand ()
        {
        }

        public string Name => "Watch";

        public Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            throw new NotImplementedException ();
        }
    }
}
