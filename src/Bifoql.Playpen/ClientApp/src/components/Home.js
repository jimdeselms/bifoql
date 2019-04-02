import React, { Component } from 'react';

import { Playpen } from './Playpen';

import './bifoql.css';
import { VsGraphql } from './VsGraphql';

var bookJson = JSON.stringify({books: [
  { name: 'Catcher in the Rye', author: 'J.D. Salinger'},
  { name: '1984', author: 'George Orwell'}
]}, null, 2);

export const Home = () => (
  <div className='bifoql-doc'>
    <h1>Bifoql</h1>

    <p>
      Bifoql stands for BIg Freaking Object Query Language. The idea is that you have service that exposes a "Big Freaking Object",
      and your clients send queries to filter that big object down and massage it into a smaller object that just gives
      you exactly what you want.
    </p>

    <p>
      In their simplest form, Bifoql queries are very powerful, and can greatly reduce the amount of data retrieved from a service,
      while allowing you to batch multiple requests with a single query.

      Dig a little deeper and Bifoql is a rich language that gives you great flexibility to reshape a service's data
      into any format you want.
    </p>

    <p>
      Bifoql is very similar to Facebook's GraphQL. I wrote it several months ago, not knowing that GraphQL existed.
      If I had used GraphQL, I probably wouldn't have bothered writing Bifoql. But here we are. I do still think Bifoql
      has value, as it adds to the querying capabilities of GraphQL to add filtering, array slicing, and built-in functions
      that allow you to massage responses into the shape that makes the most sense for your application.
    </p>

</div>
);

