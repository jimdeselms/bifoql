import React, { Component } from 'react';

import { Playpen } from './Playpen';

var bookJson = JSON.stringify({books: [
  { name: 'Catcher in the Rye', author: 'J.D. Salinger'},
  { name: '1984', author: 'George Orwell'}
]}, null, 2);

export const Home = () => (
  <div>
  <h1>Bifoql</h1>

  <p>
    Bifoql stands for BIg Freaking Object Query Language. The idea is that you have service that exposes a "Big Freaking Object",
    and your clients send queries to filter that big object down and massage it into a smaller object that just gives
    you exactly what you want.
    
    If you think of your service as being a massive JSON object, then your queries filter it down into something neat and tidy.
  </p>
  <p>
    Here's a quick example. Imagine that you have a library service, and you just want to get the names of all the books

    <Playpen input={bookJson} query="books.name" compact={true} />

    Pretty neat, eh?
  </p>
</div>
);

