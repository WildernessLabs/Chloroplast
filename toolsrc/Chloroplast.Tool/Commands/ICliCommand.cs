using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Tool.Commands
{
    public interface ICliCommand
    {
        public string Name { get; }
        public Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config);
    }

    public class ConfigCommand : ICliCommand
    {
        public string Name => "Config";

        public Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config) => throw new NotImplementedException ();
    }
}
