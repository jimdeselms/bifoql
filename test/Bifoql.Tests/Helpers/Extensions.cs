using System.Threading.Tasks;

namespace Bifoql.Tests.Helpers
{
    internal static class Extensions
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
            return ((IBifoqlLookupInternal)o)[key]().Result;
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