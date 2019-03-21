using System.Threading.Tasks;

namespace Bifoql.Tests.Extensions
{
    internal static class Helpers
    {
        public static string TryGetString(this IBifoqlObject o)
        {
            return ((IBifoqlString)o).Value.Result;
        }

        public static double TryGetNumber(this IBifoqlObject o)
        {
            return ((IBifoqlNumber)o).Value.Result;
        }

        public static IBifoqlObject TryGetValue(this IBifoqlObject o, string key)
        {
            return ((IBifoqlMapInternal)o)[key]().Result;
        }

        public static string TryGetValueAsString(this IBifoqlObject o, string key)
        {
            return o.TryGetValue(key).TryGetString();
        }

        public static object TryGetValueAsNumber(this IBifoqlObject o, string key)
        {
            return o.TryGetValue(key).TryGetNumber();
        }
    }
}