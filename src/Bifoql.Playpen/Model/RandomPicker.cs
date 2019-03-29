using System;

namespace Bifoql.Playpen.Model
{
    public static class RandomPicker
    {
        public static T Pick<T>(int seed, T[] values)
        {
            var random = new Random(seed);
            var idx = random.Next(values.Length-1);
            return values[idx];
        }

        public static int Pick(int seed, int min, int max)
        {
            var random = new Random(seed);
            return random.Next(min, max+1);
        }
    }
}