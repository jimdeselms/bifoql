using System;

namespace Bifoql.Playpen.Model
{
    public class RandomPerson : IBifoqlLookupSync
    {
        public const int MAX_PERSON = 10000;

        public string firstName { get; }
        public string lastName { get; }

        public RandomDate dob { get; }

        public RandomPerson spouse { get; set; }

        public RandomPerson(int seed)
        {
            firstName = RandomPicker.Pick(seed, firstNames);
            lastName = RandomPicker.Pick(seed, lastNames);
            dob = new RandomDate(seed);
        }

        private static string[] firstNames = new string[] 
        {
            "Steve", "Fred", "Bill", "Jim", "Todd", "Dan", "Caleb", "Brian", "Jack", "Dave", "Link",
            "Melissa", "Jill", "Susan", "Mary", "Angelica", "Becky", "Donna", "Mabel", "Zelda"
        };

        private static string[] lastNames = new string[]
        {
            "Davis", "Bronson", "Stewart", "McMurphy", "Donaldson", "Ramirez", "Gupta", "Alvarez", "Carlton",
            "Dennison", "Egertz", "Farquad", "Gilbert", "Herzog", "Ivarson", "Jankoqicz", "Klerz", "Lambert",
            "Masterson", "Noberto", "Olivar", "Poirot", "Quincy", "Rappaport", "Samuelson", "Thomas", "Undine",
            "Velasquez", "Williams", "Yutz", "Zimmerman"
        };

        public bool TryGetValue(string key, out Func<object> result)
        {
            switch (key)
            {
                case "firstName": result = () => firstName; break;
                case "lastName": result = () => lastName; break;
                case "dob": result = () => dob; break;
                case "isMarried": result = () => spouse != null; break;
                case "spouse": result = () => spouse; break;
                default: result = null; break;
            }
            return result != null;
        }
    }

    public class RandomDate
    {
        public int day { get; }
        public int month { get; }
        public int year { get; }

        public RandomDate(int seed)
        {
            day = RandomPicker.Pick(seed, 1, 28);
            month = RandomPicker.Pick(seed, 1, 12);
            year = RandomPicker.Pick(seed, 1925, 2019);
        }
    }
}