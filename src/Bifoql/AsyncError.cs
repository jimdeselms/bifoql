using System.Threading.Tasks;
using Bifoql.Types;

namespace Bifoql
{
    internal struct Location
    {
        public int Line { get; }
        public int Column { get; }

        public Location(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public override bool Equals(object o)
        {
            var other = o as Location?;
            if (other == null) return false;
            return other.Value.Line == Line && other.Value.Column == Column;
        }

        public override int GetHashCode()
        {
            return Line + (Column<<16);
        }
    }

    internal class AsyncError : IBifoqlError
    {
        public string Message { get; }

        public AsyncError(string message) : this(null, message)
        {
        }

        internal AsyncError(Location? location, string message)
        {
            var loc = location ?? new Location(0, 0);

            if (loc.Line > 0)
            {
                Message = $"({loc.Line}, {loc.Column}) {message}";
            }
            else
            {
                Message = message;
            }
        }

        public Task<bool> IsEqualTo(IBifoqlObject other)
        {
            if (this == other) 
            {
                return Task.FromResult(true);
            }
            else
            {
                var o = other as AsyncError;

                return Task.FromResult(o != null && Message == o.Message);
            }
        }

        public override bool Equals(object obj)
        {
            return (obj as AsyncError)?.Message == this.Message;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }
    }
}