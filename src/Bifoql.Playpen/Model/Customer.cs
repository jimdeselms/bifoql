using System;
using System.Collections.Generic;

namespace Bifoql.Playpen.Model
{
    public class Customer : IBifoqlLookupSync
    {
        public const int CUSTOMER_COUNT = 1000;

        public int id { get; }

        public Customer(int seed)
        {
            id = seed;
        }

        public static Customer Get(int id)
        {
            if (id < CUSTOMER_COUNT)
            {
                return new Customer(id);
            }
            else
            {
                return null;
            }
        }

        private static string[] firstNames = new string[] 
        {
            "Steve", "Fred", "Bill", "Jim", "Todd", "Dan", "Caleb", "Brian", "Jack", "Dave", "Link",
            "Melissa", "Jill", "Susan", "Mary", "Angelica", "Becky", "Donna", "Mabel", "Zelda",
            "Micah", "Penelope", "Emile", "Lucille", "Linda", "Darla", "Carolina", "Bucky", "Scarlet",
            "Charlotte", "Travis", "Chris", "Tina", "Emily", "Sandra", "Leslie", "Natalie", "Aaron",
            "Erin", "Hercule",
        };

        private static string[] lastNames = new string[]
        {
            "Smith", "Samson", "Salazar", "Sanderson",
            "Davis", "Bronson", "Stewart", "McMurphy", "Donaldson", "Ramirez", "Gupta", "Alvarez", "Carlton",
            "Dennison", "Egertz", "Farquad", "Gilbert", "Herzog", "Ivarson", "Jankoqicz", "Klerz", "Lambert",
            "Masterson", "Noberto", "Olivar", "Poirot", "Quincy", "Rappaport", "Samuelson", "Thomas", "Undine",
            "Velasquez", "Williams", "Yutz", "Zimmerman", "Hurviz", "Yin", "Peterson", "Katz"
        };

        public bool TryGetValue(string key, out Func<object> result)
        {
            var random = new Random(id);
            switch (key)
            {
                case "id": result = () => id; break;
                case "name": result = () => {
                    var firstName = RandomPicker.Pick(random, firstNames);
                    if (firstName == "Melissa") { System.Diagnostics.Debugger.Break(); }
                    return firstName + " " + RandomPicker.Pick(random, lastNames); }; break;
                case "dob": result = () => new RandomDate(random); break;
                case "address": result = () => new Address(random.Next()); break;
                case "phone": result = () => RandomPicker.Pick(random, 100, 999) + "-" + RandomPicker.Pick(random, 100, 999) + "-" + RandomPicker.Pick(random, 1000, 9999); break;
                case "friends": result = () => PickFriends(random); break;
                default: result = null; break;
            }
            return result != null;
        }

        private object PickFriends(Random r)
        {
            var count = r.Next(0,6);
            var friends = new List<object>();
            for (int i = 0; i < count; i++)
            {
                friends.Add(new Customer(r.Next(CUSTOMER_COUNT)));
            }
            return friends;
        }
    }

    public class RandomDate
    {
        public int day { get; }
        public int month { get; }
        public int year { get; }

        public RandomDate(Random random)
        {
            day = RandomPicker.Pick(random, 1, 28);
            month = RandomPicker.Pick(random, 1, 12);
            year = RandomPicker.Pick(random, 1925, 2019);
        }
    }
}