using System;
using System.Collections.Generic;
using System.Linq;

namespace Bifoql.Playpen.Model
{
    public class HeroIndex : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            var ep = args.TryGetStringParameter("episode");
            ep = ep ?? "NEWHOPE";
            Entity result;
            if (StarWars.Heroes.TryGetValue(ep, out result))
            {
                return result;
            }

            return null;
        }
    }

    public class HumanIndex : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            var id = args.TryGetStringParameter("id");
            if (id != null)
            {
                return StarWars.AllEntities.FirstOrDefault(e => e.id == id && e is Human);
            }

            return null;
        }
    }

    public class DroidIndex : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            var id = args.TryGetStringParameter("id");
            if (id != null)
            {
                return StarWars.AllEntities.FirstOrDefault(e => e.id == id && e is Droid);
            }

            return null;
        }
    }

    public class StarshipIndex : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            var id = args.TryGetStringParameter("id");
            if (id != null)
            {
                return StarWars.AllEntities.FirstOrDefault(e => e.id == id && e is Starship);
            }

            return null;
        }
    }

    public class SearchIndex : IBifoqlIndexSync
    {
        public object Lookup(IIndexArgumentList args)
        {
            var text = args.TryGetStringParameter("text");
            if (text != null)
            {
                return StarWars.AllEntities.Where(e => e.name.Contains(text));
            }

            return null;
        }
    }
    
    public static class StarWars
    {
        public static readonly Human LUKE;
        public static readonly Human LEIA;
        public static readonly Human DARTH;
        public static readonly Human HAN;
        public static readonly Human TARKIN;

        public static readonly Droid R2D2;
        public static readonly Droid C3P0;

        public static readonly Starship IMPERIAL_SHUTTLE;
        public static readonly Starship TIE_ADVANCED_X1;
        public static readonly Starship X_WING;
        public static readonly Starship MILLENIUM_FALCON;
        
        public static readonly IReadOnlyList<Entity> AllEntities;
        public static readonly IReadOnlyDictionary<string, Entity> Heroes;
        static StarWars()
        {
            var all = new string[] { "NEWHOME", "EMPIRE", "JEDI" };

            LUKE = new Human { id = "1000", name = "Luke Skywalker", height=1.72, appearsIn=all };
            LEIA = new Human { id= "1003", name = "Leia Organa", height=1.5, appearsIn=all };
            DARTH = new Human { id = "1001", name = "Darth Vader", height=2.02, appearsIn=all };
            HAN = new Human { id = "1002", name = "Han Solo", height=1.8, appearsIn=all };
            TARKIN = new Human { id = "1004", name = "Wilhuff Tarkin", height=1.8, appearsIn= new string[] { "NEWHOPE" } };

            C3P0 = new Droid { id="2000", name = "C-3P0", primaryFunction="Protocol", appearsIn=all };
            R2D2 = new Droid { id="2001", name = "R2-D2", primaryFunction="Astromech", appearsIn=all };

            MILLENIUM_FALCON = new Starship { id = "3000", name = "Millenium Falcom", length= 34.37 };
            X_WING = new Starship { id = "3001", name = "X-Wing", length= 12.5 };
            TIE_ADVANCED_X1 = new Starship { id = "3002", name = "TIE Advanced x1", length= 9.2 };
            IMPERIAL_SHUTTLE = new Starship { id = "3003", name = "Imperial Shuttle", length=20 };

            LUKE.friends = new object[] { HAN, LEIA, C3P0, R2D2 };
            LEIA.friends = new object[] { HAN, LUKE, C3P0, R2D2 };
            HAN.friends = new object[] { LUKE, LEIA, R2D2 };
            DARTH.friends = new object[] { TARKIN };
            TARKIN.friends = new object[] { DARTH };

            C3P0.friends= new object[] { LUKE, HAN, LEIA, R2D2 };
            R2D2.friends= new object[] { LUKE, HAN, LEIA };

            AllEntities = new List<Entity>()
            {
                LUKE, LEIA, DARTH, HAN, TARKIN, C3P0, R2D2, MILLENIUM_FALCON, X_WING, TIE_ADVANCED_X1, IMPERIAL_SHUTTLE
            };

            Heroes = new Dictionary<string, Entity> {
                ["NEWHOPE"] = R2D2,
                ["EMPIRE"] = LUKE,
                ["JEDI"] = R2D2
            };
        }
    }

    public class Entity
    {
        public string id { get; set; }
        public string name { get; set; }
         public IReadOnlyList<object> friends { get; set; }
    }

    public class Human : Entity
    {
        public double height { get; set; }
        public IReadOnlyList<string> appearsIn { get; set; }
    }

    public class Droid : Entity
    {
        public string primaryFunction { get; set; }
        public IReadOnlyList<string> appearsIn { get; set; }
    }

    public class Starship : Entity
    {
        public double length { get; set; }
    }
}