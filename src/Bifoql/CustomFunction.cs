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

        public static CustomFunction Create<T1>(Func<T1, object> func)
        {
            return new CustomFunction<T1>(func);
        }
        public static CustomFunction Create<T1, T2>(Func<T1, T2, object> func)
        {
            return new CustomFunction<T1, T2>(func);
        }
        public static CustomFunction Create<T1, T2, T3>(Func<T1, T2, T3, object> func)
        {
            return new CustomFunction<T1, T2, T3>(func);
        }
    }

    internal class CustomFunction<T1> : CustomFunction 
    {
        private Func<Location, QueryContext, IBifoqlObject, Task<IBifoqlObject>> _func;

        public CustomFunction(Func<T1, object> func)
        {
            _func = async (l, c, a1) => {
                var obj1 = await a1.ToSimpleObject();

                if (!(obj1 is T1))
                {
                    return new AsyncError($"Could not cast {obj1?.GetType().FullName} into type {typeof(T1).FullName}");
                }

                return func((T1)obj1).ToBifoqlObject();
            };
        }

        internal override Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments)
        {
            return new TypedFunctionCallExpr<IBifoqlObject>(location, name, arguments, _func);
        }
    }

    internal class CustomFunction<T1, T2> : CustomFunction 
    {
        private Func<Location, QueryContext, IBifoqlObject, IBifoqlObject, Task<IBifoqlObject>> _func;

        public CustomFunction(Func<T1, T2, object> func)
        {
            _func = async (l, c, a1, a2) => {
                var obj1 = await a1.ToSimpleObject();
                var obj2 = await a2.ToSimpleObject();

                if (!(obj1 is T1))
                {
                    return new AsyncError($"Could not cast {obj1?.GetType().FullName} into type {typeof(T1).FullName}");
                }

                if (!(obj2 is T2))
                {
                    return new AsyncError($"Could not cast {obj2?.GetType().FullName} into type {typeof(T2).FullName}");
                }

                return func((T1)obj1, (T2)obj2).ToBifoqlObject();
            };
        }

        internal override Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments)
        {
            return new TypedFunctionCallExpr<IBifoqlObject, IBifoqlObject>(location, name, arguments, _func);
        }
    }

    internal class CustomFunction<T1, T2, T3> : CustomFunction 
    {
        private Func<Location, QueryContext, IBifoqlObject, IBifoqlObject, IBifoqlObject, Task<IBifoqlObject>> _func;

        public CustomFunction(Func<T1, T2, T3, object> func)
        {
            _func = async (l, c, a1, a2, a3) => {
                var obj1 = await a1.ToSimpleObject();
                var obj2 = await a2.ToSimpleObject();
                var obj3 = await a3.ToSimpleObject();

                if (!(obj1 is T1))
                {
                    return new AsyncError($"Could not cast {obj1?.GetType().FullName} into type {typeof(T1).FullName}");
                }

                if (!(obj2 is T2))
                {
                    return new AsyncError($"Could not cast {obj2?.GetType().FullName} into type {typeof(T2).FullName}");
                }

                if (!(obj3 is T3))
                {
                    return new AsyncError($"Could not cast {obj3?.GetType().FullName} into type {typeof(T3).FullName}");
                }

                return func((T1)obj1, (T2)obj2, (T3)obj3).ToBifoqlObject();
            };        
        }

        internal override Expr ToExpr(Location location, string name, IReadOnlyList<Expr> arguments)
        {
            return new TypedFunctionCallExpr<IBifoqlObject, IBifoqlObject, IBifoqlObject>(location, name, arguments, _func);
        }
    }
}