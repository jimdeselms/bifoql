using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    using System.Linq;
    internal class IndexedType : BifoqlType
    {
        public BifoqlType ResultType { get; }
        public IndexParameter[] Parameters { get; }

        public IndexedType(BifoqlType resultType, params IndexParameter[] parameters)
        {
            Guard.ArgumentNotNull(resultType, nameof(resultType));
            ResultType = resultType;
            Parameters = parameters;
        }

        public override object ToObject()
        {
            return new 
            { 
                indexes = Parameters.Select(p => p.ToObject()).ToList(),
                resultType = ResultType.ToObject()
            };
        }

        internal override IEnumerable<NamedType> ReferencedNamedTypes => 
            Parameters
                .SelectMany(p => p.Type.ReferencedNamedTypes)
                .Concat(ResultType.ReferencedNamedTypes);

        internal override string ToString(int indent)
        {
            var keys = Parameters.Select(p => p.ToString(indent));
            return $"({string.Join(", ", keys)}) => {ResultType.ToString(indent)}";
        }
    }
}