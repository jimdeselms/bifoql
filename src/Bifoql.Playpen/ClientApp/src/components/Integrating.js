import React, { Component } from 'react';

import { Playpen } from './Playpen';

import './bifoql.css';

var bookJson = JSON.stringify({books: [
  { name: 'Catcher in the Rye', author: 'J.D. Salinger'},
  { name: '1984', author: 'George Orwell'}
]}, null, 2);

var code = `public async <object> RunQuery(string query, IReadOnlyDictionary<string, object> arguments)
{
    var obj = new YourRootObject();
    var query = Bifoql.Query.Compile(query);
    return await query.Run(obj, arguments).Result;
}
`;

export const Integrating = () => (
  <div className='bifoql-doc'>
    <h1>Integrating with Bifoql</h1>

    <p>
        Integrating with Bifoql is not difficult; you'll probably want to expose it as an endpoint from an MVC application. At it's core, you've got this snippet of 
        code to execute a Bifoql query and get the result:

        <pre>{code}</pre>
    </p>

    <p>
        In this example, we take in some parameters; you can get them from the query string. Then, we create an instance of our root object. This object can be any C# object that follows a few rules:
        <ul>
            <li>It contains a number of public properties with getters; each of these properties will be exposed to the client.</li>
            <li>The type of these properties can be:
                <ul>
                    <li>Simple types like strings, ints, etc.</li>
                    <li>Complex types that also follow these properties (you can nest these objects as deeply as you want.)</li>
                    <li>A Bifoql type (IBifoqlLookup, IBifoqlMapSync, IBifoqlIndexedLookup, etc.</li>
                    <li>Task&lt;T> where T is a type that follows these rules</li>
                    <li>An IEnumerable&lt;T> of a type that follows these rules</li>
                    <li>An IReadOnlyDictionary&lt;string, T> where T is a a type that follows these rules</li>
                </ul>
            </li>
        </ul>
    </p>

    <h2>Implementing Bifoql interfaces</h2>

    <p>You can also create Bifoql objects by implementing interfaces. This approach might give a little more flexibility. The interfaces are quite simple, and you can find them in <a href="https://github.com/jimdeselms/bifoql/blob/master/src/Bifoql/PublicInterfaces.cs">the Github repo.</a></p>
</div>
);

