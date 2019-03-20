namespace Bifoql.Expressions
{
    using System.Linq;

    internal static class Utilities
    {
        public static string Escape(string id, out bool isEscaped)
        {
            if (id.Length > 1)
            {
                var firstChar = id[0];
                var rest = id.Skip(1);
                if ((firstChar == '_' || char.IsLetter(firstChar))
                    && rest.All(c => c == '_' || char.IsLetterOrDigit(c)))
                {
                    isEscaped = false;
                    return id;                    
                }
            }

            isEscaped = true;
            return id.Replace("\"", "\\\"");
        }
    }
}