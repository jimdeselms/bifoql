namespace Bifoql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bifoql.Expressions;
    using Bifoql.Extensions;

    /// You can create custom functions that take up to three parametrs.
    public abstract class CustomFunction
    {
        internal abstract Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments);

        public static CustomFunction Create<T1>(Func<T1, Task<IAsyncObject>> func)
            where T1 : IAsyncObject
        {
            return new CustomFunction<T1>(func);
        }
        public static CustomFunction Create<T1, T2>(Func<T1, T2, Task<IAsyncObject>> func)
            where T1 : IAsyncObject
            where T2 : IAsyncObject
        {
            return new CustomFunction<T1, T2>(func);
        }
        public static CustomFunction Create<T1, T2, T3>(Func<T1, T2, T3, Task<IAsyncObject>> func)
            where T1 : IAsyncObject
            where T2 : IAsyncObject
            where T3 : IAsyncObject
        {
            return new CustomFunction<T1, T2, T3>(func);
        }
    }

    internal class CustomFunction<T1> : CustomFunction 
        where T1 : IAsyncObject
    {
        private Func<Location, QueryContext, T1, Task<IAsyncObject>> _func;

        public CustomFunction(Func<T1, Task<IAsyncObject>> func)
        {
            _func = (l, c, a1) => func(a1);
        }

        internal override Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments)
        {
            return new TypedFunctionCallExpr<T1>(location, name, arguments, _func);
        }
    }

    internal class CustomFunction<T1, T2> : CustomFunction 
        where T1 : IAsyncObject 
        where T2 : IAsyncObject
    {
        private Func<Location, QueryContext, T1, T2, Task<IAsyncObject>> _func;

        public CustomFunction(Func<T1, T2, Task<IAsyncObject>> func)
        {
            _func = (l, c, a1, a2) => func(a1, a2);
        }

        internal override Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments)
        {
            return new TypedFunctionCallExpr<T1, T2>(location, name, arguments, _func);
        }
    }

    internal class CustomFunction<T1, T2, T3> : CustomFunction 
        where T1 : IAsyncObject 
        where T2 : IAsyncObject
        where T3 : IAsyncObject
    {
        private Func<Location, QueryContext, T1, T2, T3, Task<IAsyncObject>> _func;

        public CustomFunction(Func<T1, T2, T3, Task<IAsyncObject>> func)
        {
            _func = (l, c, a1, a2, a3) => func(a1, a2, a3);
        }

        internal override Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments)
        {
            return new TypedFunctionCallExpr<T1, T2, T3>(location, name, arguments, _func);
        }
    }
}