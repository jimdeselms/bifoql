using System;

namespace Bifoql.Playpen.Model
{
    public static class RandomPicker
    {
        public static T Pick<T>(Random random, T[] values)
        {
            var idx = random.Next(values.Length-1);
            return values[idx];
        }

        public static int Pick(Random random, int min, int max)
        {
            return random.Next(min, max+1);
        }
    }
}