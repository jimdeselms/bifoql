# Maps #

Maps map named keys to values; they're like JSON objects. Here's an object:

```
{
    name: "This is an object",
    values: [ 1, 2, 3]
}
```

Maps can mix literal values with other expressions. 

```
{
    name: "Fred",
    foo: @.this.that
}
```

Maps allow a number of shorthands. Here's an example:

```
// This map projection...
{
    name: name
    address: address | { street1, zipCode }
    orders: orders |< { orderId }
}

// is equivalent to this:
{
    name,
    address { street1, zipCode },
    orders |< { orderId }
}
```

## Referencing values in a map ##

Consider this map:
```
$map = {
    name: "Fred",
    "zip code": 12345
};

```

To access the properties of a map, you can use the `.` operator, or you can use an indexing syntax, like in Javascript:

```
$map.name
$map['zip code']
$map.'zip code'
```

## Spread operator

You can merge maps together with the spread operator, similar to Javascript:

```
$map1 = { a: 1 };
$map2 = { b: 2, a: 500 };

{ ...$map1, ...$map2 }
```

As with Javascript, if there are conflicts, the later spread will take precedence.