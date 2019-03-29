using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bifoql;

namespace Bifoql.Playpen.Model
{
    internal class TestModel : IBifoqlLookupSync
    {
        public bool TryGetValue(string key, out Func<object> result)
        {
            object obj = null;
            switch (key)
            {
                case "person": obj = new { byId = new PersonLookup(), byRange = new PersonIndex() }; break;
                default: obj = null; break;
            }

            if (obj != null)
            {
                result = () => (object)obj;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }

    internal class PersonIndex : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            int startAt = (int?)args.TryGetNumberParameter("startAt") ?? 0;
            int take = (int?)args.TryGetNumberParameter("take") ?? 20;

            var result = new List<RandomPerson>();
            for (int i = 0; i < take; i++)
            {
                var person = PersonRepository.Get(i);
                if (person == null)
                {
                    break;
                }
                result.Add(person);
            }
            return result;
        }
    }

    internal class PersonLookup : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            int? id = (int?)args.TryGetNumberParameter("id");
            if (id.HasValue && id.Value < RandomPerson.MAX_PERSON)
            {
                return PersonRepository.Get(id.Value);
            }
            else
            {
                return null;
            }
        }
    }
    internal class PersonRepository
    {
        private static RandomPerson[] _persons;

        static PersonRepository()
        {
            _persons = new RandomPerson[RandomPerson.MAX_PERSON];
            for (int i = 0; i < RandomPerson.MAX_PERSON; i++)
            {
                _persons[i] = new RandomPerson(i);
            }

            for (int i = 0; i < RandomPerson.MAX_PERSON/5; i++)
            {
                var spouse1 = RandomPicker.Pick(i, 0, RandomPerson.MAX_PERSON-1);
                var spouse2 = RandomPicker.Pick(i+1, 0, RandomPerson.MAX_PERSON-1);

                _persons[spouse1].spouse = _persons[spouse2];
                _persons[spouse2].spouse = _persons[spouse1];
            }
        }

        public static RandomPerson Get(int i)
        {
            if (i < _persons.Length)
            {
                return _persons[i];
            }
            else
            {
                return null;
            }
        }
    }
}