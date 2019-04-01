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
        /// it becomes a function from an object to a Func<Task<IBifoqlObject>>, which is the lingua franca
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
            }

            // Anything else, treat it like a POCO. (We may want to restrict this more.)
            return o => Task.FromResult(func(o).ToBifoqlObject());
        }
        private static Func<object, Task<IBifoqlObject>> ConvertTask(Func<object, object> func, Type funcType)
        {
            // Ideally, we shouldn't have to restrict this to returning "Task<object>", but 
            // there isn't a simple way to convert a Task<T> into a Task<object>
//            if (funcType == typeof(Task<object>) || funcType == typeof(Task<IBifoqlObject>))
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
            else
            {
                return await ConvertToObjectTask(task);
            }

            throw new Exception($"Don't know how to convert {task.GetType().FullName} to Task<IBifoqlObject>");
        }

        private static async Task<IBifoqlObject> ConvertToObjectTask(object task)
        {
            if (task is Task<object>)
            {
                var result = await ((Task<object>)task);
                return result.ToBifoqlObject();
            }

            // Garsh this is hacky, but it won't block the thread, and it allows us to convert any Task<T> to a Task<IBifoqlObject>
            // I don't know what the performance characteristics of this are, but I have a feeling they're not that bad.
            // TODO - I believe there's a better way using dymamic, but I'm not going to worry about it for now.
            await ((Task)task);

            var resultProperty = task.GetType().GetProperty("Result").GetGetMethod();
            return resultProperty.Invoke(task, new object[0]).ToBifoqlObject();
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
