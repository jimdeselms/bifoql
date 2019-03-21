using Bifoql.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bifoql.Adapters
{
    internal static class FuncConverter
    {
        /// <summary>
        /// Converts a function that takes one object and returns another, and fixes it so that
        /// it becomes a function from an object to a Task<IBifoqlObject>, which is the lingua franca
        /// type used internally.
        /// </summary>
        /// <param name="func">The function (must be object -> object.)</param>
        /// <param name="funcType">The actual return type of the function.</param>
        public static Func<object, Task<IBifoqlObject>> Convert(Func<object, object> func, Type funcType)
        {
            if (funcType.IsGenericType)
            {
                var genericTypeDef = funcType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(Task<>))
                {
                    return ConvertTask(func, funcType);
                }

                else if (genericTypeDef == typeof(Lazy<>))
                {
                    return ConvertLazy(func, funcType);
                }

                else if (genericTypeDef == typeof(Func<>))
                {
                    return ConvertFunc(func, funcType);
                }
            }

            // Anything else, treat it like a POCO. (We may want to restrict this more.)
            return o => Task.FromResult(func(o).ToBifoqlObject());
        }

        private static Func<object, Task<IBifoqlObject>> ConvertFunc(Func<object, object> func, Type funcType)
        {
            if (funcType == typeof(Func<Task<IBifoqlObject>>))
            {
                return o => CoalesceTask(CoalesceFunc<Task<IBifoqlObject>>(func(o))());
            }

            if (funcType == typeof(Func<IBifoqlObject>))
            {
                return o => ToTask(CoalesceFunc<IBifoqlObject>(func(o))());
            }

            if (funcType == typeof(Func<Task<object>>))
            {
                return o => CoalesceTask(CoalesceFunc<Task<object>>(func(o))());
            }

            if (funcType == typeof(Func<object>))
            {
                return o => ToTask((CoalesceFunc<object>(func(o))()).ToBifoqlObject());
            }

            throw new Exception("Func return type must be either Func<object>, Func<IBifoqlObject>, Func<Task<object>> or Func<Task<IBifoqlObject>>");
        }

        private static Func<object, Task<IBifoqlObject>> ConvertLazy(Func<object, object> func, Type funcType)
        {
            if (funcType == typeof(Lazy<Task<IBifoqlObject>>))
            {
                return o => CoalesceTask(
                    CoalesceLazy<Task<IBifoqlObject>>(func(o)).Value);
            }

            if (funcType == typeof(Lazy<IBifoqlObject>))
            {
                return o => ToTask(
                    CoalesceLazy<IBifoqlObject>(func(o)).Value);
            }

            if (funcType == typeof(Lazy<Task<object>>))
            {
                return o => CoalesceTask(
                    CoalesceLazy<Task<object>>(func(o)).Value);
            }

            if (funcType == typeof(Lazy<object>))
            {
                return o => ToTask(
                    CoalesceLazy<object>(func(o)).Value.ToBifoqlObject());
            }

            throw new Exception("Lazy return type must be either Lazy<object>, Lazy<IBifoqlObject>, Lazy<Task<object>> or Lazy<Task<IBifoqlObject>>");
        }
        private static Func<object, Task<IBifoqlObject>> ConvertTask(Func<object, object> func, Type funcType)
        {
            if (funcType == typeof(Task<object>) || funcType == typeof(Task<IBifoqlObject>))
            {
                return o => CoalesceTask(func(o));
            }

            throw new Exception("Task return type must be either Task<object> or Task<IBifoqlObject>");
        }

        private static async Task<IBifoqlObject> CoalesceTask(object task)
        {
            if (task == null)
            {
                return new AsyncError("task is null");
            }
            else if (task is Task<IBifoqlObject>)
            {
                return await (Task<IBifoqlObject>)task;
            }
            else if (task is Task<object>)
            {
                var obj = await (Task<object>)task;
                return obj.ToBifoqlObject();
            }

            throw new Exception($"Don't know how to convert {task.GetType().FullName} to Task<IBifoqlObject>");
        }

        private static Func<TP> CoalesceFunc<TP>(object func)
        {
            return func == null ? () => default(TP) : (Func<TP>)func;
        }

        private static Lazy<TP> CoalesceLazy<TP>(object lazy)
        {
            return lazy == null ? new Lazy<TP>(() => default(TP)) : (Lazy<TP>)lazy;
        }

        private static Task<IBifoqlObject> ToTask(IBifoqlObject o)
        {
            return Task.FromResult(o);
        }
    }
}
