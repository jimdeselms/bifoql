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
      Bifoql stands for BIg Freaking Object Query Language. The idea is that you have service that exposes a "Big Freaking Object",
      and your clients send queries to filter that big object down and massage it into a smaller object that just gives
      you exactly what you want.
    </p>

</div>
);

