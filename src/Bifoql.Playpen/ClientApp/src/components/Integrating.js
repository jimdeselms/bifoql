import React, { Component } from 'react';

import { Playpen } from './Playpen';

import './bifoql.css';

var bookJson = JSON.stringify({books: [
  { name: 'Catcher in the Rye', author: 'J.D. Salinger'},
  { name: '1984', author: 'George Orwell'}
]}, null, 2);

export const Integrating = () => (
  <div className='bifoql-doc'>
    <h1>Integrating with Bifoql</h1>

    <p>
        Integrating with Bifoql is not difficult. There are a few steps:

        <ol>
            <l1>Define your data objects</l1>
            <l1>Defining a schema</l1>
            <li>Create an HTTP service</li>
            <l1>Expose an endpoint for accepting queries</l1>
            <l1>Run the query and return the result</l1>
        </ol>
    </p>

    <h2>Defining your data</h2>

    <p>At the highest level, your Bifoql service returns a single root object. This object typically returns a 
        number of fields which in turn can be simple POCOs, or Bifoql lookups or indexes.</p>

    <p>Each property on a Bifoql object should be a <code>System.Func&lt;object></code>.</p>

    <Playpen readOnly={true} query="using foo.boogers;" />

</div>
);

