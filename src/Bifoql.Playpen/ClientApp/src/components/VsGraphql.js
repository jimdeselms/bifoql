import React, { Component } from 'react';

import { Playpen } from './Playpen';

import './bifoql.css';

var bookJson = JSON.stringify({books: [
  { name: 'Catcher in the Rye', author: 'J.D. Salinger'},
  { name: '1984', author: 'George Orwell'}
]}, null, 2);

export const VsGraphql = () => (
  <div className='bifoql-doc'>
    <h1>Why not GraphQL?</h1>

    <p>
      Bifoql has a lot of similarites to GraphQL; when I first wrote Bifoql, I didn't know GraphQL existed, and I wrote it to solve a similar problem, but I had different inspirations.
      I was initially inspired by AWS's command line interfaces, which allows you to specify filters to your commands. These commands allow you to pick and choose the fields you want, and to remap
      the results into a completely different shape. I was also inspired by the language "JMESPath" which solves a similar problem by remapping a JSON object into a different JSON object.
    </p>

    <p>
      But I also wanted to be able to write generic APIs for services so that clients didn't have to bug the service providers for new endpoints for every use case. This is the basic functionality that
      GraphQL provides, however, it doesn't have the remapping capabilities of Bifoql.
    </p>

    <p>
      GraphQL is great. It has support for many languages, it's also actively supported by Facebook. It has a user community that can help you solve any problem. It also has type safety, and it supports subscriptions.
      It's also a little bit simpler language.
    </p>

    <p>
      Bifoql allows you to write more complex queries; that way you can move complexity out of your client-side code and put it in your queries. It also allows you to make much more compact results without
      making changes to the GraphQL API. Some examples:
      <ul>
        <li>Using the <code>[? condition]</code> construct, you can filter results server-side</li>
        <li>There's a <code>length()</code> built-in function so that you can return the length of an array; in GraphQL you'd have to download the whole list and count it client side. Or, 
        the server would have to explicitly add a field to get the count.</li>
        <li>You can remap a result to collapse it down to a single field if you want. In GraphQL, if you had a deeply-nested structure, and only wanted a single field, you'd have to download a large
          structure that ultimately contains a single leaf node. In Bifoql, if you just want a single string, you just a single string back.
        </li>
      </ul>
    </p>

    <p>
      In GraphQL, all nodes are download simultaneously, and as a result, you can't write a query to look up another record based on the result of another subquery. Bifoql also takes advantage of 
      parallelization, but it also lets you chain together your queries so that the result of one query can be used as a parameter in another query.
    </p>

    <p>
      Are these additional features in Bifoql compelling enough to cause people to adopt Bifoql over GraphQL? Maybe?!
    </p>

</div>
);

