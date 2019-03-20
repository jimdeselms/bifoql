namespace Bifoql
{
    using System;

    internal class QuerySegment
    {
        public string Text { get; }
        public string KeyLookup { get; } = null;
        public bool IsGetAll { get; } = false;

        public int? IndexLookup { get; } = null;

        public bool IsIdentity { get; } = false;

        public QuerySegment(String text)
        {
            Text = (text ?? "").TrimStart('.');

            if (Text == "")
            {
                IsIdentity = true;
                return;
            }

            if (Text.StartsWith("["))
            {
                string indexStr = Text.Substring(1, Text.Length-2);
                if (indexStr == "*" || indexStr == "")
                {
                    IsGetAll = true;
                }
                else
                {
                    IndexLookup = int.Parse(indexStr);
                }
            }
            else
            {
                KeyLookup = Text;
            }
        }
    }
}