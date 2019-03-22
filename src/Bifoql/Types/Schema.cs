namespace Bifoql.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Schema
    {
        private readonly BifoqlType _rootType;
        private readonly Dictionary<string, NamedType> _namedTypes = new Dictionary<string, NamedType>();

        public Schema(BifoqlType rootType)
        {
            _rootType = rootType;
        }
        public Schema WithNamedType(string name, BifoqlType type, string documentation = null)
        {
            // Make this a dictionary to prevent duplicates.
            _namedTypes.Add(name, new NamedType(name, type, documentation));
            return this;
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