### Bifoql ###

Bifoql is a library for ASynchronous Object Queries

Similar to JSON, Bifoql objects can be strings, arrays, or hash tables of objects.

The big difference is that when you iterate over the members of an array or look things up in a dictionary, you're doing it asynchronously.

Additionally, the nodes of the object can be lazy-loaded.

Then, you can apply queries to one of these objects, and only the nodes of the object needed to satisfy the query will be loaded.

### Why? ###

Bifoql can be used to make web services more flexible. Often, people design their web services using an over-gereralized API where the service returns more information than the client needs, slowing down the service and wasting computation resources.

Imagine that you have an API that returns product information, and that information includes everything from inventory to pricing information. And imagine that building that response, requires calls to multiple other services. Sometimes, if performance is critical, it's a common practice to build a "front-end for back-end" service that consumes the other data, and reformats into something more useful for another service.

If you use Bifoql in your service, you can allow the client to request the entire result which might be quite large and expensive to generate, but then, the client can also parse a query string, which will map the object into something smaller that will be easily consumable by the client.

Then, on the server side, you can build your response in such a way that only the bits of information that are actually requetsed will be returned; it's a win-win for the client and the server.

### Todo ###
* Bubble up errors
    * When evaluating an expression, if you find an error in the arguments, then just return that error.
    * I think this is done, but I need some testing around it. I'm not sure that it works in every case.
* Schema validation
    * Now that we've got the basic idea of schema, we should be able to do validation on the schema... that basically means that I should be able to do a request, and if the response for that request doesn't match the schema, then it's a fail.
* Schema propagation.
    * If I have a service with a formally defined schema, but I return a subset of that object, I should get the appropriate schema, not just the inferred one. For example, if I have an array of a type, when I get an element from the array, I should just return the element type, rather than trying to infer it.
    * If you can propagate down the "real" schema, then you could also build the inferred schema and compare them. THat's how validation would work.
    * Maybe type inference itself is a bad idea?!
* Formal index definition
    * (This is basically satisfied by adding schema definition to indexes; I'm not going to mess with this.)
    * I should able to return the precise list of known indexes on an object; this can be returned when showing the schema, or when the user tries to call a schema that doesn't exist.
    * Each filter has:
        * The set of parameters
        * The type of each parameter
* Timeouts, and timeout estimation
* Fix distinct so that it's not terrible
    * It's currently O(n^2)
    * If objects had a "GetHashCode" method, then you could use that to build a hash of the objects.
    * This means everything should actually implement an async GetHashCode so that I can sort objects and only have to do a deep conversion of objects that have the same hash code.
* group by
* sorting by multiple keys