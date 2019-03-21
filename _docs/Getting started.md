# Getting started

In Bifoql, you write queries, and execute them against a source object to re-map them into a smaller datastructure.

Technically, Bifoql queries don't have to act on a source object. For the purposes of showing how the queries work, we'll do that. But keep in mind that the real power of Bifoql comes to applying queries to large objects and massaging them into new results.

Let's walk through a few examples with a very simple source object:

```
{
    "accountId": "12345",
    "name": "Fred Wilkerson",
    "address": {
        "street1": "1 Main Street",
        "city": "Townville",
        "zipCode": 54321
    },
    "orderHistory": [
        { orderId: 500, totalUsd: 10.99 },
        { orderId: 593, totalUsd: 14.49 },
        { orderId: 793, totalUsd: 104.49 },
    ],
    "shoppingCart": [
        ...
    ],
    ...
}

```

Imagine that this object represents every bit of information about every customer in my database; this is too much information for a typical web service to return, and for typical consumers of this service won't want all of that data.

Let's say that all I care about is the customer's name. Great! Here's a query for that:

```
name
```

And the result is 
```
"Fred Wilkerson"
```

Let's say that I just want to get the customer's order history. Here's the query:

```
orderHistory
```

And that gives me:
```
"orderHistory": [
    { "orderId": 500, "totalUsd": 10.99 },
    { "orderId": 593, "totalUsd": 14.49 },
    { "orderId": 793, "totalUsd": 104.49 },
],
```

Bifoql queries can also remap a structure into something else. Let's say that I want the customer's name, zipCode, and the amount of their first order. Here's how to do that:

```
{
    name,
    zip: address.zipCode,
    firstOrderAmount: orderHistory[0].totalUsd
}
```

And this gives me:
```
{
    "name": "Fred Wilkerson",
    "zip": 54321,
    "firstOrderAmount": 10.99
}
```

Note that in the example above query, the `name` property is using a shorthand for `name: name`. When composing a map, if the key and the value have the same name, then you may omit the key.

### Pipes

You can change the context by piping a result with the `|` operator. These two examples are equivalent:

```
// This doesn't use a pipe
{
    street1: address.street1,
    zipCode: address.zipCode
}

// Here's an example using a pipe
address |
{
    street1: street1,
    zipCode: zipCode
}

// Even more concise:
address | { street1, zipCode }
```

