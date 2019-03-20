using System.Threading.Tasks;

namespace Bifoql.Tests.Extensions
{
    public static class Helpers
    {
        public static string TryGetString(this IAsyncObject o)
        {
            return ((IAsyncString)o).Value.Result;
        }

        public static double TryGetNumber(this IAsyncObject o)
        {
            return ((IAsyncNumber)o).Value.Result;
        }

        public static IAsyncObject TryGetValue(this IAsyncObject o, string key)
        {
            return ((IAsyncMap)o)[key]().Result;
        }

        public static string TryGetValueAsString(this IAsyncObject o, string key)
        {
            return o.TryGetValue(key).TryGetString();
        }

        public static object TryGetValueAsNumber(this IAsyncObject o, string key)
        {
            return o.TryGetValue(key).TryGetNumber();
        }
    }
}