using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Types
{
    using System.Linq;
    public class IndexedType : BifoqlType
    {
        public BifoqlType ResultType { get; }
        public IndexParameter[] Parameters { get; }

        public IndexedType(BifoqlType resultType, params IndexParameter[] parameters)
        {
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

        public override IEnumerable<NamedType> ReferencedNamedTypes => 
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