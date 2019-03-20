namespace Bifoql.Lex
{    
    public class Token
    {
        public string Kind { get; }
        public string Text { get; }

        public int LineStart { get; }
        public int ColStart { get; }
        public int LineEnd { get; }
        public int ColEnd { get; }

        public static readonly Token Null = new Token("NULL", "NULL", 0, 0, 0, 0);

        internal Token(string kind, string text, int lineStart, int colStart, int lineEnd, int colEnd)
        {
            Kind = kind;
            Text = text;

            LineStart = lineStart;
            ColStart = colStart;
            LineEnd = lineEnd;
            ColEnd = colEnd;
        }

        public override string ToString()
        {
            if (Kind == Text)
            {
                return Kind;
            }
            else
            {
                return $"{Kind} ({Text})";
            }
        }
    }
}