namespace Bifoql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal abstract class TypedFunctionCallExpr : Expr
    {
        protected string Name { get; }
        protected IReadOnlyList<Expr> Arguments { get; }
        protected TypedFunctionCallExpr(Location location, string name, IReadOnlyList<Expr> arguments) : base(location)
        {
            Name = name;
            Arguments = arguments;
        }

        protected async Task<IAsyncObject> GetArgument<T>(string name, int i, QueryContext context) where T : IAsyncObject
        {
            if (i >= Arguments.Count)
            {
                return new AsyncError(Location, $"too few arguments passed to {name}");
            }
            var val = await Arguments[i].Apply(context);
            if (val is IAsyncError) return val;
            if (!(val is T))
            {
                return new AsyncError(Arguments[i].Location, $"argument {name}: expected {typeof(T).Name}, got {val.GetType().Name} instead.");
            }

            return val;
        }

        public override string ToString()
        {
            var args = "(" + string.Join(",", Arguments.Select(a => a.ToString())) + ")";
            return Name + args;
        }

        public override bool NeedsAsync(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            // Special case. If this is "eval", then we can't simplify this
            return Name == "eval" || Arguments.Any(a => a.NeedsAsync(variables));
        }
    }

    internal class TypedFunctionCallExpr<T1> : TypedFunctionCallExpr where T1 : IAsyncObject
    {
        private Func<Location, QueryContext, T1, Task<IAsyncObject>> _func;

        public TypedFunctionCallExpr(
            Location location, 
            string name, 
            IReadOnlyList<Expr> arguments,
            Func<Location, QueryContext, T1, Task<IAsyncObject>> func) : base(location, name, arguments)
        {
            _func = func;
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            var a1 = await GetArgument<T1>("arg1", 0, context);
            if (a1 is IAsyncError) return a1;

            return await _func(Location, context, (T1)a1);
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            return new TypedFunctionCallExpr<T1>(Location, Name, Arguments, _func);
        }
    }


    internal class TypedFunctionCallExpr<T1, T2> : TypedFunctionCallExpr 
        where T1 : IAsyncObject
        where T2 : IAsyncObject
    {
        private Func<Location, QueryContext, T1, T2, Task<IAsyncObject>> _func;

        public TypedFunctionCallExpr(
            Location location, 
            string name, 
            IReadOnlyList<Expr> arguments,
            Func<Location, QueryContext, T1, T2, Task<IAsyncObject>> func) : base(location, name, arguments)
        {
            _func = func;
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            var a1 = await GetArgument<T1>("arg1", 0, context);
            if (a1 is IAsyncError) return a1;

            var a2 = await GetArgument<T2>("arg2", 1, context);
            if (a2 is IAsyncError) return a2;

            return await _func(Location, context, (T1)a1, (T2)a2);
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            return new TypedFunctionCallExpr<T1, T2>(Location, Name, Arguments, _func);
        }
    }

    internal class TypedFunctionCallExpr<T1, T2, T3> : TypedFunctionCallExpr 
        where T1 : IAsyncObject
        where T2 : IAsyncObject
        where T3 : IAsyncObject
    {
        private Func<Location, QueryContext, T1, T2, T3, Task<IAsyncObject>> _func;

        public TypedFunctionCallExpr(
            Location location, 
            string name, 
            IReadOnlyList<Expr> arguments,
            Func<Location, QueryContext, T1, T2, T3, Task<IAsyncObject>> func) : base(location, name, arguments)
        {
            _func = func;
        }

        protected override async Task<IAsyncObject> DoApply(QueryContext context)
        {
            var a1 = await GetArgument<T1>("arg1", 0, context);
            if (a1 is IAsyncError) return a1;

            var a2 = await GetArgument<T2>("arg2", 1, context);
            if (a2 is IAsyncError) return a2;

            var a3 = await GetArgument<T2>("arg3", 2, context);
            if (a3 is IAsyncError) return a3;

            return await _func(Location, context, (T1)a1, (T2)a2, (T3)a3);
        }

        protected override Expr SimplifyChildren(IReadOnlyDictionary<string, IAsyncObject> variables)
        {
            return new TypedFunctionCallExpr<T1, T2, T3>(Location, Name, Arguments, _func);
        }
    }
}