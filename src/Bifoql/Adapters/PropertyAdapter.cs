using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Bifoql.Types;

namespace Bifoql.Adapters
{
    internal class PropertyAdapter
    {
        private static ConcurrentDictionary<Type, PropertyAdapterInstanceBuilder> _builders 
            = new ConcurrentDictionary<Type, PropertyAdapterInstanceBuilder>();

        public static IAsyncObject Create<T>(T o)
        {
            return Create(o, typeof(T));
        }
        
        public static IAsyncObject Create(object o, Type type)
        {
            PropertyAdapterInstanceBuilder builder;
            if (!_builders.TryGetValue(type, out builder))
            {
                builder = new PropertyAdapterInstanceBuilder(type);
                _builders[type] = builder;
            }

            return builder.Create(o);
        }

        private class PropertyAdapterInstanceBuilder
        {
            // For each type, we'll load a set of getters from the type.
            public PropertyAdapterInstanceBuilder(Type type)
            {
                // Get all the public getters on the object and map them to functions.
                var getters = new Dictionary<string, Func<object, Task<IAsyncObject>>>();

                foreach (var prop in type.GetProperties().Where(p => p.GetGetMethod() != null))
                {
                    var getMethod = prop.GetGetMethod();
                    getters[prop.Name] = FuncConverter.Convert(
                        o => getMethod.Invoke(o, NO_ARGUMENTS), 
                        getMethod.ReturnType);
                }

                _getters = getters;
            }

            private readonly IReadOnlyDictionary<string, Func<object, Task<IAsyncObject>>> _getters;
            private static readonly object[] NO_ARGUMENTS = new object[0];

            public IEnumerable<string> Keys => _getters.Keys;

            public IAsyncObject Create(object @object)
            {
                var getters = _getters.ToDictionary(
                    p => p.Key,
                    p => (Func<Task<IAsyncObject>>)(() => p.Value(@object)));

                return new AsyncMap(getters);
            }
       }
    }
}