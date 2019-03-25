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
        public static async Task<IBifoqlObject> Abs(Location location, QueryContext context, IBifoqlNumber number)
        {
            var value = await number.Value;
            return new AsyncNumber(Math.Abs(value));
        }

        public static async Task<IBifoqlObject> Avg(Location location, QueryContext context, IBifoqlArrayInternal list)
        {
            double average = 0.0;

            foreach (var item in list)
            {
                var num = (await item()) as IBifoqlNumber;
                if (num == null) return new AsyncError(location, "To take an average, all itmes in a list must be a number");

                average += (await num.Value) / (double)list.Count;
            }

            return new AsyncNumber(average);
        }

        public static async Task<IBifoqlObject> Ceil(Location location, QueryContext context, IBifoqlNumber number)
        {
            var value = await number.Value;
            return new AsyncNumber(Math.Ceiling(value));
        }

        public static async Task<IBifoqlObject> Distinct(Location location, QueryContext context, IBifoqlArrayInternal list)
        {
            var distinctObjects = new List<IBifoqlObject>();

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

            return new AsyncArray(distinctObjects.Select(o => (Func<Task<IBifoqlObject>>)(() => Task.FromResult(o))).ToList());
        }

        public static async Task<IBifoqlObject> Eval(Location location, QueryContext context, IBifoqlExpression expr)
        {
            return await expr.Evaluate(context);
        }

        public static async Task<IBifoqlObject> Flatten(Location location, QueryContext context, IBifoqlArrayInternal list)
        {
            var flattenedList = new List<Func<Task<IBifoqlObject>>>();

            foreach (var item in list)
            {
                var currItem = await item();
                var currList = currItem as IBifoqlArrayInternal;
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

        public static async Task<IBifoqlObject> Floor(Location location, QueryContext context, IBifoqlNumber number)
        {
            var value = await number.Value;
            return new AsyncNumber(Math.Floor(value));
        }

        public static async Task<IBifoqlObject> Join(Location location, QueryContext context, IBifoqlString glue, IBifoqlArrayInternal stringList)
        {
            var strings = new List<string>();
            foreach (var s in stringList)
            {
                var str = await s() as IBifoqlString;
                if (str == null) return new AsyncError(location, "Each member of join's string list must be a string");

                strings.Add(await str.Value);
            }

            var glueStr = await glue.Value;
            return new AsyncString(string.Join(glueStr, strings));
        }

        public static Task<IBifoqlObject> Keys(Location location, QueryContext context, IBifoqlMapInternal map)
        {
            var keys = new List<Func<Task<IBifoqlObject>>>();

            foreach (var pair in map)
            {
                keys.Add(() => Task.FromResult((IBifoqlObject)new AsyncString(pair.Key)));
            }

            return Task.FromResult<IBifoqlObject>(new AsyncArray(keys));
        }

        public static async Task<IBifoqlObject> Length(Location location, QueryContext context, IBifoqlObject value)
        {
            var str = value as IBifoqlString;
            if (str != null)
            {
                return new AsyncNumber((await str.Value).Length);
            }

            var arr = value as IBifoqlArrayInternal;
            if (arr != null)
            {
                return new AsyncNumber(arr.Count);
            }

            return new AsyncError(location, "length accepts a number or string argument");
        }

        public static Task<IBifoqlObject> MaxBy(Location location, QueryContext context, IBifoqlArrayInternal list, IBifoqlExpression keyExpr)
        {
            return MaxMin(location, context, list, keyExpr, max: true);
        }

        public static Task<IBifoqlObject> Max(Location location, QueryContext context, IBifoqlArrayInternal list)
        {
            return MaxMin(location, context, list, null, max: true);
        }

        public static Task<IBifoqlObject> MinBy(Location location, QueryContext context, IBifoqlArrayInternal list, IBifoqlExpression keyExpr)
        {
            return MaxMin(location, context, list, keyExpr, max: false);
        }

        public static Task<IBifoqlObject> Min(Location location, QueryContext context, IBifoqlArrayInternal list)
        {
            return MaxMin(location, context, list, null, max: false);
        }

        private static async Task<IBifoqlObject> MaxMin(Location location, QueryContext context, IBifoqlArrayInternal list, IBifoqlExpression keyQuery, bool max)
        {
            var pairs = new List<KeyValuePair<IBifoqlObject, IBifoqlObject>>();
            foreach (var value in list)
            {
                var val = await value();
                var newContext = context.ReplaceTarget(val);

                var key = keyQuery == null ? val : await keyQuery.Evaluate(newContext);

                pairs.Add(new KeyValuePair<IBifoqlObject, IBifoqlObject>(key, val));
            }

            var resultNum = max ? double.MinValue : double.MaxValue;
            string resultStr = null;
            bool first = true;
            bool isNum = false;
            IBifoqlObject result = null;

            foreach (var val in pairs)
            {
                var curr = val.Key;
                if (first)
                {
                    first = false;
                    isNum = curr is IBifoqlNumber;
                }

                if (isNum)
                {
                    var currNumObj = curr as IBifoqlNumber;
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
                    var currStrObj = curr as IBifoqlString;
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

        public static Task<IBifoqlObject> Reverse(Location location, QueryContext context, IBifoqlArrayInternal array)
        {
            return Task.FromResult<IBifoqlObject>(new AsyncArray(array.Reverse().ToList()));
        }

        public static Task<IBifoqlObject> Sort(Location location, QueryContext context, IBifoqlArrayInternal array)
        {
            return SortBy(location, context, array, null);
        }

        public static async Task<IBifoqlObject> SortBy(Location location, QueryContext context, IBifoqlArrayInternal array, IBifoqlExpression keyExpr)
        {
            var pairs = new List<Tuple<IBifoqlObject, IBifoqlObject>>();
            var values = new List<IBifoqlObject>();
            foreach (var obj in array)
            {
                var currObj = await obj();
                var keyObj = keyExpr == null
                    ? currObj
                    : await keyExpr.Evaluate(context.ReplaceTarget(currObj));

                pairs.Add(Tuple.Create(keyObj, currObj));
            }

            if (pairs.All(p => p.Item1 is IBifoqlString))
            {
                var pairsByString = new List<Tuple<string, IBifoqlObject>>();
                foreach (var item in pairs)
                {
                    pairsByString.Add(Tuple.Create(await ((IBifoqlString)item.Item1).Value, item.Item2));
                }
                return pairsByString.OrderBy(p => p.Item1).Select(p => p.Item2).ToList().ToBifoqlObject();
            }
            else if (pairs.All(p => p.Item1 is IBifoqlNumber))
            {
                var pairsByNumber = new List<Tuple<double, IBifoqlObject>>();
                foreach (var item in pairs)
                {
                    pairsByNumber.Add(Tuple.Create(await ((IBifoqlNumber)item.Item1).Value, item.Item2));
                }
                return pairsByNumber.OrderBy(p => p.Item1).Select(p => p.Item2).ToList().ToBifoqlObject();
            }
            else
            {
                return new AsyncError("sort must be sorted either by number or string");
            }
        }

        public static async Task<IBifoqlObject> Sum(Location location, QueryContext context, IBifoqlArrayInternal numbers)
        {
            double sum = 0.0;

            foreach (var item in numbers)
            {
                var num = (await item()) as IBifoqlNumber;
                if (num == null) return new AsyncError(location, "To take an average, all itmes in a list must be a number");

                sum += await num.Value;
            }

            return new AsyncNumber(sum);
        }

        public static async Task<IBifoqlObject> ToMap(Location location, QueryContext context, IBifoqlArrayInternal list, IBifoqlExpression keyExpr, IBifoqlExpression valueExpr)
        {
            var dict = new Dictionary<string, IBifoqlObject>();

            foreach (var item in list)
            {
                var target = await item();

                var currContext = context.ReplaceTarget(target);
                var key = await keyExpr.Evaluate(currContext) as IBifoqlString;
                if (key == null) return new AsyncError(location, "Result of to_map's key expression must be a string");

                var value = await valueExpr.Evaluate(currContext);

                dict.Add(await key.Value, value);
            }

            return dict.ToBifoqlMap();
        }

        public static async Task<IBifoqlObject> ToNumber(Location location, QueryContext context, IBifoqlString str)
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

        public static Task<IBifoqlObject> Unzip(Location location, QueryContext context, IBifoqlMapInternal map)
        {
            var keys = new List<Func<Task<IBifoqlObject>>>();
            var values = new List<Func<Task<IBifoqlObject>>>();

            foreach (var pair in map)
            {
                keys.Add(() => Task.FromResult((IBifoqlObject)new AsyncString(pair.Key)));
                values.Add(pair.Value);
            }

            var resultDict = new List<Func<Task<IBifoqlObject>>>
            {
                () => Task.FromResult((IBifoqlObject)new AsyncArray(keys)),
                () => Task.FromResult((IBifoqlObject)new AsyncArray(values))
            };

            return Task.FromResult<IBifoqlObject>(new AsyncArray(resultDict));
        }

        public static Task<IBifoqlObject> Values(Location location, QueryContext context, IBifoqlMapInternal map)
        {
            var values = new List<Func<Task<IBifoqlObject>>>();

            foreach (var pair in map)
            {
                values.Add(pair.Value);
            }

            return Task.FromResult<IBifoqlObject>(new AsyncArray(values));
        }

        public static async Task<IBifoqlObject> Zip(Location location, QueryContext context, IBifoqlArrayInternal keyList, IBifoqlArrayInternal valueList)
        {
            if (keyList.Count != valueList.Count) return new AsyncError(location, "lists passed to zip must be the same length");
            var resultDict = new Dictionary<string, Func<Task<IBifoqlObject>>>();
            
            for (int i = 0; i < valueList.Count; i++)
            {
                var key = await (await keyList[i]() as IBifoqlString).Value;
                if (key == null) return new AsyncError(location, "each element in zip's key list must be a string");

                resultDict[key] = valueList[i];
            }

            return new AsyncLookup(resultDict);
        }
    }
}