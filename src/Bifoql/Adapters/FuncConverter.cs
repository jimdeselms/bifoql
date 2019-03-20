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
        /// it becomes a function from an object to a Task<IAsyncObject>, which is the lingua franca
        /// type used internally.
        /// </summary>
        /// <param name="func">The function (must be object -> object.)</param>
        /// <param name="funcType">The actual return type of the function.</param>
        public static Func<object, Task<IAsyncObject>> Convert(Func<object, object> func, Type funcType)
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
            return o => Task.FromResult(func(o).ToAsyncObject());
        }

        private static Func<object, Task<IAsyncObject>> ConvertFunc(Func<object, object> func, Type funcType)
        {
            if (funcType == typeof(Func<Task<IAsyncObject>>))
            {
                return o => CoalesceTask(CoalesceFunc<Task<IAsyncObject>>(func(o))());
            }

            if (funcType == typeof(Func<IAsyncObject>))
            {
                return o => ToTask(CoalesceFunc<IAsyncObject>(func(o))());
            }

            if (funcType == typeof(Func<Task<object>>))
            {
                return o => CoalesceTask(CoalesceFunc<Task<object>>(func(o))());
            }

            if (funcType == typeof(Func<object>))
            {
                return o => ToTask((CoalesceFunc<object>(func(o))()).ToAsyncObject());
            }

            throw new Exception("Func return type must be either Func<object>, Func<IAsyncObject>, Func<Task<object>> or Func<Task<IAsyncObject>>");
        }

        private static Func<object, Task<IAsyncObject>> ConvertLazy(Func<object, object> func, Type funcType)
        {
            if (funcType == typeof(Lazy<Task<IAsyncObject>>))
            {
                return o => CoalesceTask(
                    CoalesceLazy<Task<IAsyncObject>>(func(o)).Value);
            }

            if (funcType == typeof(Lazy<IAsyncObject>))
            {
                return o => ToTask(
                    CoalesceLazy<IAsyncObject>(func(o)).Value);
            }

            if (funcType == typeof(Lazy<Task<object>>))
            {
                return o => CoalesceTask(
                    CoalesceLazy<Task<object>>(func(o)).Value);
            }

            if (funcType == typeof(Lazy<object>))
            {
                return o => ToTask(
                    CoalesceLazy<object>(func(o)).Value.ToAsyncObject());
            }

            throw new Exception("Lazy return type must be either Lazy<object>, Lazy<IAsyncObject>, Lazy<Task<object>> or Lazy<Task<IAsyncObject>>");
        }
        private static Func<object, Task<IAsyncObject>> ConvertTask(Func<object, object> func, Type funcType)
        {
            if (funcType == typeof(Task<object>) || funcType == typeof(Task<IAsyncObject>))
            {
                return o => CoalesceTask(func(o));
            }

            throw new Exception("Task return type must be either Task<object> or Task<IAsyncObject>");
        }

        private static async Task<IAsyncObject> CoalesceTask(object task)
        {
            if (task == null)
            {
                return new AsyncError("task is null");
            }
            else if (task is Task<IAsyncObject>)
            {
                return await (Task<IAsyncObject>)task;
            }
            else if (task is Task<object>)
            {
                var obj = await (Task<object>)task;
                return obj.ToAsyncObject();
            }

            throw new Exception($"Don't know how to convert {task.GetType().FullName} to Task<IAsyncObject>");
        }

        private static Func<TP> CoalesceFunc<TP>(object func)
        {
            return func == null ? () => default(TP) : (Func<TP>)func;
        }

        private static Lazy<TP> CoalesceLazy<TP>(object lazy)
        {
            return lazy == null ? new Lazy<TP>(() => default(TP)) : (Lazy<TP>)lazy;
        }

        private static Task<IAsyncObject> ToTask(IAsyncObject o)
        {
            return Task.FromResult(o);
        }
    }
}
