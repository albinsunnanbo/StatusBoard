using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatusBoard.Core
{
    public static class Utilities
    {
        public static IEnumerable<StatusCheck> GetAllStatusChecksInAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(type => !type.IsAbstract)
                .Where(type => typeof(StatusCheck).IsAssignableFrom(type));
            return types.Select(type => Activator.CreateInstance(type)).Cast<StatusCheck>();
        }
    }
}
