using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bifoql;

namespace Bifoql.Playpen.Model
{
    
    internal class TestModel
    {
        public object customer => new {
                byId = new CustomerIndexById(), 
                byRange = new CustomerIndexByRange(),
                all = new CustomerIndexAll()
            };
        public object hero => new HeroIndex();
        public object droid => new DroidIndex();
        public object human => new HumanIndex();
        public object starship => new StarshipIndex();
        public object search => new SearchIndex();
    }

    internal class CustomerIndexAll : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            var result = new List<Customer>();
            for (int i = 0; i < Customer.CUSTOMER_COUNT; i++)
            {
                var customer = Customer.Get(i);
                if (customer == null)
                {
                    break;
                }
                result.Add(customer);
            }
            return result;
        }
    }

    internal class CustomerIndexByRange : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            int startAt = (int?)args.TryGetNumberParameter("startAt") ?? 0;
            int take = (int?)args.TryGetNumberParameter("take") ?? int.MaxValue;

            var result = new List<Customer>();
            for (int i = 0; i < take; i++)
            {
                var customer = Customer.Get(i + startAt);
                if (customer == null)
                {
                    break;
                }
                result.Add(customer);
            }
            return result;
        }
    }

    internal class CustomerIndexById : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            int? id = (int?)args.TryGetNumberParameter("id");
            if (id.HasValue)
            {
                return Customer.Get(id.Value);
            }
            else
            {
                return null;
            }
        }
    }
}