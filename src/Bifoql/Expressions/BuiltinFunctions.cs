namespace Bifoql.Expressions
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using Bifoql.Adapters;
    using Bifoql.Extensions;

    internal class BuiltinFunctions
    {
        public static async Task<IAsyncObject> Abs(Location location, QueryContext context, IAsyncNumber number)
        {
            var value = await number.Value;
            return new AsyncNumber(Math.Abs(value));
        }

        public static async Task<IAsyncObject> Avg(Location location, QueryContext context, IAsyncArray list)
        {
            double average = 0.0;

            foreach (var item in list)
            {
                var num = (await item()) as IAsyncNumber;
                if (num == null) return new AsyncError(location, "To take an average, all itmes in a list must be a number");

                average += (await num.Value) / (double)list.Count;
            }

            return new AsyncNumber(average);
        }

        public static async Task<IAsyncObject> Ceil(Location location, QueryContext context, IAsyncNumber number)
        {
            var value = await number.Value;
            return new AsyncNumber(Math.Ceiling(value));
        }

        public static async Task<IAsyncObject> Distinct(Location location, QueryContext context, IAsyncArray list)
        {
            var distinctObjects = new List<IAsyncObject>();

            // This is TOTALLY the wrong way to do this, it's O(n^2), and it might just cause objects to be evaluted multiple times
            //
            // But just for now, it gets the job done.
            foreach (var item in list)
            {
                var currItem = await item();
                bool alreadyFound = false;

                foreach (var distinctObj in distinctObjects)
                {
                    if (await currItem.IsEqualTo(distinctObj))
                    {
                        alreadyFound = true;
                        break;
                    }
                }

                if (!alreadyFound)
                {
                    distinctObjects.Add(currItem);
                }
            }

            return new AsyncArray(distinctObjects.Select(o => (Func<Task<IAsyncObject>>)(() => Task.FromResult(o))).ToList());
        }

        public static async Task<IAsyncObject> Eval(Location location, QueryContext context, IAsyncExpression expr)
        {
            return await expr.Evaluate(context);
        }

        public static async Task<IAsyncObject> Flatten(Location location, QueryContext context, IAsyncArray list)
        {
            var flattenedList = new List<Func<Task<IAsyncObject>>>();

            foreach (var item in list)
            {
                var currItem = await item();
                var currList = currItem as IAsyncArray;
                if (currList != null)
                {
                    foreach (var subitem in currList)
                    {
                        flattenedList.Add(subitem);
                    }
                }
                else
                {
                    flattenedList.Add(() => Task.FromResult(currItem));
                }
            }

            return new AsyncArray(flattenedList);
        }

        public static async Task<IAsyncObject> Floor(Location location, QueryContext context, IAsyncNumber number)
        {
            var value = await number.Value;
            return new AsyncNumber(Math.Floor(value));
        }

        public static async Task<IAsyncObject> Join(Location location, QueryContext context, IAsyncString glue, IAsyncArray stringList)
        {
            var strings = new List<string>();
            foreach (var s in stringList)
            {
                var str = await s() as IAsyncString;
                if (str == null) return new AsyncError(location, "Each member of join's string list must be a string");

                strings.Add(await str.Value);
            }

            var glueStr = await glue.Value;
            return new AsyncString(string.Join(glueStr, strings));
        }

        public static Task<IAsyncObject> Keys(Location location, QueryContext context, IAsyncMap map)
        {
            var keys = new List<Func<Task<IAsyncObject>>>();

            foreach (var pair in map)
            {
                keys.Add(() => Task.FromResult((IAsyncObject)new AsyncString(pair.Key)));
            }

            return Task.FromResult<IAsyncObject>(new AsyncArray(keys));
        }

        public static async Task<IAsyncObject> Length(Location location, QueryContext context, IAsyncObject value)
        {
            var str = value as IAsyncString;
            if (str != null)
            {
                return new AsyncNumber((await str.Value).Length);
            }

            var arr = value as IAsyncArray;
            if (arr != null)
            {
                return new AsyncNumber(arr.Count);
            }

            return new AsyncError(location, "length accepts a number or string argument");
        }

        public static Task<IAsyncObject> MaxBy(Location location, QueryContext context, IAsyncArray list, IAsyncExpression keyExpr)
        {
            return MaxMin(location, context, list, keyExpr, max: true);
        }

        public static Task<IAsyncObject> Max(Location location, QueryContext context, IAsyncArray list)
        {
            return MaxMin(location, context, list, null, max: true);
        }

        public static Task<IAsyncObject> MinBy(Location location, QueryContext context, IAsyncArray list, IAsyncExpression keyExpr)
        {
            return MaxMin(location, context, list, keyExpr, max: false);
        }

        public static Task<IAsyncObject> Min(Location location, QueryContext context, IAsyncArray list)
        {
            return MaxMin(location, context, list, null, max: false);
        }

        private static async Task<IAsyncObject> MaxMin(Location location, QueryContext context, IAsyncArray list, IAsyncExpression keyQuery, bool max)
        {
            var pairs = new List<KeyValuePair<IAsyncObject, IAsyncObject>>();
            foreach (var value in list)
            {
                var val = await value();
                var newContext = context.ReplaceTarget(val);

                var key = keyQuery == null ? val : await keyQuery.Evaluate(newContext);

                pairs.Add(new KeyValuePair<IAsyncObject, IAsyncObject>(key, val));
            }

            var resultNum = max ? double.MinValue : double.MaxValue;
            string resultStr = null;
            bool first = true;
            bool isNum = false;
            IAsyncObject result = null;

            foreach (var val in pairs)
            {
                var curr = val.Key;
                if (first)
                {
                    first = false;
                    isNum = curr is IAsyncNumber;
                }

                if (isNum)
                {
                    var currNumObj = curr as IAsyncNumber;
                    if (currNumObj == null)
                    {
                        return new AsyncError(location, "To take max, all members of the list must be a number or string");
                    }
                    var currNum = await currNumObj.Value;

                    if ((max && currNum > resultNum) || (!max && currNum < resultNum))
                    {
                        resultNum = currNum;
                        result = val.Value;
                    }
                }
                else
                {
                    var currStrObj = curr as IAsyncString;
                    if (currStrObj == null)
                    {
                        return new AsyncError(location, "To take max, all members of the list must be a number or string");
                    }
                    var currStr = await currStrObj.Value;

                    if (resultStr == null || currStr.CompareTo(resultStr) == (max ? 1 : -1))
                    {
                        resultStr = currStr;
                        result = val.Value;
                    }
                }
            }

            return result;
        }

        public static Task<IAsyncObject> Reverse(Location location, QueryContext context, IAsyncArray array)
        {
            return Task.FromResult<IAsyncObject>(new AsyncArray(array.Reverse().ToList()));
        }

        public static Task<IAsyncObject> Sort(Location location, QueryContext context, IAsyncArray array)
        {
            return SortBy(location, context, array, null);
        }

        public static async Task<IAsyncObject> SortBy(Location location, QueryContext context, IAsyncArray array, IAsyncExpression keyExpr)
        {
            var pairs = new List<Tuple<IAsyncObject, IAsyncObject>>();
            var values = new List<IAsyncObject>();
            foreach (var obj in array)
            {
                var currObj = await obj();
                var keyObj = keyExpr == null
                    ? currObj
                    : await keyExpr.Evaluate(context.ReplaceTarget(currObj));

                pairs.Add(Tuple.Create(keyObj, currObj));
            }

            if (pairs.All(p => p.Item1 is IAsyncString))
            {
                var pairsByString = new List<Tuple<string, IAsyncObject>>();
                foreach (var item in pairs)
                {
                    pairsByString.Add(Tuple.Create(await ((IAsyncString)item.Item1).Value, item.Item2));
                }
                return pairsByString.OrderBy(p => p.Item1).Select(p => p.Item2).ToList().ToAsyncObject();
            }
            else if (pairs.All(p => p.Item1 is IAsyncNumber))
            {
                var pairsByNumber = new List<Tuple<double, IAsyncObject>>();
                foreach (var item in pairs)
                {
                    pairsByNumber.Add(Tuple.Create(await ((IAsyncNumber)item.Item1).Value, item.Item2));
                }
                return pairsByNumber.OrderBy(p => p.Item1).Select(p => p.Item2).ToList().ToAsyncObject();
            }
            else
            {
                return new AsyncError("sort must be sorted either by number or string");
            }
        }

        public static async Task<IAsyncObject> Sum(Location location, QueryContext context, IAsyncArray numbers)
        {
            double sum = 0.0;

            foreach (var item in numbers)
            {
                var num = (await item()) as IAsyncNumber;
                if (num == null) return new AsyncError(location, "To take an average, all itmes in a list must be a number");

                sum += await num.Value;
            }

            return new AsyncNumber(sum);
        }

        public static async Task<IAsyncObject> ToMap(Location location, QueryContext context, IAsyncArray list, IAsyncExpression keyExpr, IAsyncExpression valueExpr)
        {
            var dict = new Dictionary<string, IAsyncObject>();

            foreach (var item in list)
            {
                var target = await item();

                var currContext = context.ReplaceTarget(target);
                var key = await keyExpr.Evaluate(currContext) as IAsyncString;
                if (key == null) return new AsyncError(location, "Result of to_map's key expression must be a string");

                var value = await valueExpr.Evaluate(currContext);

                dict.Add(await key.Value, value);
            }

            return dict.ToAsyncObject();
        }

        public static async Task<IAsyncObject> ToNumber(Location location, QueryContext context, IAsyncString str)
        {
            double dbl;
            if (double.TryParse(await str.Value, out dbl))
            {
                return new AsyncNumber(dbl);
            }
            else
            {
                return new AsyncError(location, "Can't convert string into number");
            }
        }

        public static Task<IAsyncObject> Unzip(Location location, QueryContext context, IAsyncMap map)
        {
            var keys = new List<Func<Task<IAsyncObject>>>();
            var values = new List<Func<Task<IAsyncObject>>>();

            foreach (var pair in map)
            {
                keys.Add(() => Task.FromResult((IAsyncObject)new AsyncString(pair.Key)));
                values.Add(pair.Value);
            }

            var resultDict = new List<Func<Task<IAsyncObject>>>
            {
                () => Task.FromResult((IAsyncObject)new AsyncArray(keys)),
                () => Task.FromResult((IAsyncObject)new AsyncArray(values))
            };

            return Task.FromResult<IAsyncObject>(new AsyncArray(resultDict));
        }

        public static Task<IAsyncObject> Values(Location location, QueryContext context, IAsyncMap map)
        {
            var values = new List<Func<Task<IAsyncObject>>>();

            foreach (var pair in map)
            {
                values.Add(pair.Value);
            }

            return Task.FromResult<IAsyncObject>(new AsyncArray(values));
        }

        public static async Task<IAsyncObject> Zip(Location location, QueryContext context, IAsyncArray keyList, IAsyncArray valueList)
        {
            if (keyList.Count != valueList.Count) return new AsyncError(location, "lists passed to zip must be the same length");
            var resultDict = new Dictionary<string, Func<Task<IAsyncObject>>>();
            
            for (int i = 0; i < valueList.Count; i++)
            {
                var key = await (await keyList[i]() as IAsyncString).Value;
                if (key == null) return new AsyncError(location, "each element in zip's key list must be a string");

                resultDict[key] = valueList[i];
            }

            return new AsyncMap(resultDict);
        }
    }
}