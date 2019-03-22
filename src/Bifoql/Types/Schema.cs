namespace Bifoql.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Schema
    {
        private readonly BifoqlType _rootType;
        private readonly Dictionary<string, BifoqlNamedType> _namedTypes = new Dictionary<string, BifoqlNamedType>();

        public Schema(BifoqlType rootType, params BifoqlNamedType[] namedTypes)
        {
            _rootType = rootType;

            foreach (var namedType in namedTypes)
            {
                _namedTypes.Add(namedType.Name, namedType);
            }
        }

        public string BuildDocumentation()
        {
            var builder = new StringBuilder();
            builder.Append(_rootType.GetDocumentation(0));

            bool first = true;
            foreach (var type in _namedTypes.Values.OrderBy(pair => pair.Name))
            {
                if (first)
                {
                    builder.AppendLine();
                    first = false;
                }
                builder.AppendLine();
                builder.AppendLine(type.GetDocumentation(0));
            }

            return builder.ToString();
        }
    }

}