using System;

namespace Bifoql.Playpen.Model
{
    public class Address : IBifoqlLookupSync
    {
        private readonly int _id;

        public Address(int id)
        {
            _id = id;
        }

        private static string[] streetNames = new string[]
        {
            "Main", "Maple", "Elk", "Oak", "River", "Windy", "Lakeshore", "Broad", "Rodeo",
            "Pleasant", "Great Tree", "Hill", "Mountain", "Happy", "Scenic", "Industrial", "Homey", "Diamond"
        };

        private static string[] streetTypes = new string[]
        {
            "St", "Rd", "Blvd", "Dr", "Cir", "Hwy", "Ct", "Ln"
        };

        public bool TryGetValue(string key, out Func<object> result)
        {
            var random = new Random(_id);

            switch (key)
            {
                case "street": result = () => RandomPicker.Pick(random, 1, 1000) + " " + RandomPicker.Pick(random, streetNames) + " " + RandomPicker.Pick(random, streetTypes); break;
                case "zipCode": result = () => RandomPicker.Pick(random, 10000, 99999); break;
                default: result = null; break;
            }
            return result != null;
        }
    }
}