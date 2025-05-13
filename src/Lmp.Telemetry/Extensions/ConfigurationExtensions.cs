using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Lmp.Telemetry.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T BindWithDisplayName<T>(this IConfigurationSection section) where T : new()
        {
            var instance = new T();
            section.Bind(instance);
            return instance;
        }
    }
}
