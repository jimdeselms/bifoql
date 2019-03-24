# Arrays #

Arrays in Bifoql work like they do in most languages.

Arrays can be of mixed types:

```
[ 1, 'foo', [1, 'x'], [], 5 ]
```

## Indexes and spreads

You can get individual array elements. If the element doesn't exist, then it returns `undefined`
```
// Gets the 5th element of an array
array[5] 
```

You can get elements from the end of an array.
```
// Returns 'blue'
['red', 'white', 'blue'][-1]
```

You can use spreads to get a range of elements.

The lower bound is inclusive, and the upper bound is exclusive, that is, it'll return everything up to that point.
```
// Returns [10, 15]
[5, 10, 15, 20, 25][1..3]
```

The lower and upper bounds are optional. If the lower bound is omitted, it'll return everything up to the upper bound.
If the upper bound is ommitted, it'll return everything starting at the lower bound, to the end.

The upper and lower bounds can also be negative:

```
[1, 2, 3, 4, 5][2..] // [3, 4, 5]
[1, 2, 3, 4, 5][..2] // [1, 2]
[1, 2, 3, 4, 5][-1..-1] // [2, 3, 4]
```
## Filters
For any array, you can use the `[?` syntax to filter the result:

```
[10, 20, 30, 40][? @ > 20] // [30, 40]
```