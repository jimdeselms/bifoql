### Bifoql ###

Bifoql is the BIg Freaking Object Query Language.

Similar to JSON, Bifoql objects can be strings, arrays, or hash tables of objects; it can also contain indexes which allow you to quickly search for specific objects.

The big difference is that when you iterate over the members of an array or look things up in a dictionary, you're doing it asynchronously.

Additionally, the nodes of the object can be lazy-loaded.

Then, you can apply queries to one of these objects, and only the nodes of the object needed to satisfy the query will be loaded.

### Documentation ###

See the _docs folder.

### Why? ###

Bifoql can be used to make web services more flexible. Often, people design their web services using an over-gereralized API where the service returns more information than the client needs, slowing down the service and wasting computation resources.

Imagine that you have an API that returns product information, and that information includes everything from inventory to pricing information. And imagine that building that response, requires calls to multiple other services. Sometimes, if performance is critical, it's a common practice to build a "front-end for back-end" service that consumes the other data, and reformats into something more useful for another service.

If you use Bifoql in your service, you can allow the client to request the entire result which might be quite large and expensive to generate, but then, the client can also parse a query string, which will map the object into something smaller that will be easily consumable by the client.

Then, on the server side, you can build your response in such a way that only the bits of information that are actually requetsed will be returned; it's a win-win for the client and the server.

### Todo ###
* Query validaton
    * If an object has a schema, then we should be able to statically determine if the incoming query is compliant
    * Need to think about how union types are going to work
    * My original thinking on schema validation wasn't great; the thinking was that the schema validation would happen on the result, not the request.
* Enum types
* Add interfaces to make certain kinds of objects more performant for certain kinds of requests:
    * Support true enumerable so that if you if you take a slice, you won't have to get the whole thing
    * "ICountable" meaning that you can just get the count of something
    * "ISortable" meaning that you can pre-sort the object instead of doing it in a dumb way (you could do the sort through a query for example.)

### bugs ###
* Need a better message when you can't find a key on an object
* Lookup up a key that doesn't exist should be an error.
