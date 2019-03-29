import React, { Component } from 'react';

import { Playpen } from './Playpen';

import './bifoql.css';

var bookJson = JSON.stringify({books: [
  { name: 'Catcher in the Rye', author: 'J.D. Salinger'},
  { name: '1984', author: 'George Orwell'}
]}, null, 2);

export const Home = () => (
  <div className='bifoql-doc'>
    <h1>Learning to write Bifoql queries</h1>

    <p>
      Bifoql stands for BIg Freaking Object Query Language. The idea is that you have service that exposes a "Big Freaking Object",
      and your clients send queries to filter that big object down and massage it into a smaller object that just gives
      you exactly what you want.
    </p>

    <p>    
      If you think of your service as being a massive JSON object, then your queries filter it down into something neat and tidy. However, it's more like a JSON
      object that can have cyclic references and has the ability to call functions to look up other objects.
    </p>

    <h2>Simple queries</h2>
    <p>
      Here's your first example. We have a service that exposes customers. I have a customer ID and I just want to know their phone number. 
    </p>
    
    <aside>All these examples are interactive; just change the query, then click out of it to run it.</aside>

    <Playpen query="customer.byId(id: 232).phone" />

    <p>This is a very simple query that only returns a single piece of information. Let's take a look at the parts of this query. <code>customer</code> is a root-level object which exposes 
    a number of <i>indexes</i> into my service. Indexes are the main API of my service; I send an index parameters, and it looks up the correct result based on those inputs.</p>

    <p>After <code>customer</code>, we have <code>.byId(id: 232)</code>. In my service, <code>byId</code> is an index which accepts an id parameter and gives me back a customer.</p>

    <p>Then, we have <code>.phone</code>. This means, I'm getting back the "phone" property of the customer object.</p>

    <p>You can also get back more than one field. Let's say that I want to get the customer's name, phone number, and zipCode:</p>

    <Playpen query={`customer.byId(id: 232) { 
  phone, 
  address { 
    zipCode 
  }
}`} />

    <p>This query asks for customer 232, and then it constructs a JSON snipped based on the requested values. I want the "phone" field, and then from the "address" field, I want the "zipCode."</p>

    <p>This is nice, but I might want to make it a little tidier:</p>

    <Playpen query={`customer.byRange(startAt: 1000, take: 2)
{
  name,
  street: address.street,
  dob { day, month }
}`} />

    <p>Bifoql allows you to remap an object's fields and give them new names, or reach down into the object and grab fields. It makes for a more concise response.
    In this example, `zipCode: address.zipCode` means that we're getting the "zipCode" field from the customer's "address", and we're putting that into a new field called "zip".</p>

    <p>You're also not limited to querying a single index in Bifoql; this query asks for the names of two customers:</p>

    <Playpen query={`{ 
  customer1: customer.byId(id: 400).name,
  customer2: customer.byId(id: 999).name
}`} />

    <h2>Bifoql arrays</h2>

    <p>In addition to pulling fields off of objects, you can also request arrays of objects. In these queries, we'll use another index provided by our service, `byRange`.</p>

    <Playpen query="customer.byRange(startAt: 1000, take: 5).dob.year" />

    <p>In our test service, the <code>person</code> object exposes a <code>byRange</code> index which allows you to specify a starting point, and the number of customers to return.</p> <aside>Note that Bifoql
    doesn't natively do anything to limit the size of your result set; it's up to the implementor to put in limits to prevent clients from downloading too much data. (More on that later.)</aside>

    <p>You can request multiple fields and remap them:</p>

    <Playpen query={`customer.byId(id: 232) { 
  phone, 
  zip: address.zipCode
}`} />

    <h3>Array indexes</h3>

    <p>In the above examples, we've been relying on the `byRange` index, which allows us to select a subset of records in our database. However, in order to use that `toRange` index, someone
    had to have implemented it in our service. In some simple services, the developer might not want to take the time to write such indexes, and they might only provide you a method of downloading
    all of the records. </p>

    <p>In your client application, however, you still probably want to limit your results. That's where slices come in. In the following advantes, we'll take advantage of another 
    index provided by our service: `all`.</p>

    <aside>When Bifoql filters an array, it does all of its processing server-side. This is great for the client; they only have to download a fraction of the data they want. On the server, it could
      mean that your service is filtering through thousands of records. When designing your indexes, consider adding limits to your result sets, or providing explicit indexes that allow more
      clever server-side filtering.
    </aside>

    <p>You can take the nth record in the array:</p>

    <Playpen query="customer.all()[15].name" />

    <p>You can use negative indexes to, say, get the last item in the array:</p>

    <Playpen query="customer.all()[-1].name" />

    <p>You can take a range of records, and you can also use negative indexes. This query will get the last five items in the array.</p>

    <Playpen query="customer.all()[-6..-1].address.zipCode" />

    <h2>Variables</h2>

    <p>Bifoql allows you to define variables that can be substituted in your queries.</p>

    <p>In this example, we create the <code>$c</code> variable and then use it to create a new array of values:</p>

    <Playpen query="$c = customer.byId(id: 234); [ $c.name, $c.dob.year, $c.address.zipCode ]" />

    <p>When defining a variable, it must always be followed by a <code>;</code></p>

    <aside>Variables can only be defined once in your query. Bifoql is a pure functional language; there are no operations that allow you to change state.</aside>

    <h3>Special variables</h3>
    <p>There are two special variables that are always defined: <code>$</code> and <code>@</code>.</p>

    <p><code>$</code> refers to the root-level item in your query, that is, the input into your query. In all the examples so far, the `person` field is just a field defined on that root-level item. 
    At the top-level of your query, the <code>$</code> variable is optional.</p>

    <Playpen query={`[
  // These two expressions are identical
  $.customer.byId(id: 232).phone
  customer.byId(id: 232).phone
]`} />

    <p><code>@</code> is the other special variable, and it refers to the <i>current context</i>. At the top-level of your query, the current context is the same as the top-level input object. However, as we build objects or use filters or pipes,
    the context can change. Normally, you don't need to explicitly reference the <code>@</code> variable, but there are special cases where you may have to, like when using certain filters.</p>
    <h2>Filters</h2>

    <p>Bifoql also allows you to use boolean conditions to filter the results of a query. This query gets three customers born before 1940:</p>

    <Playpen query="customer.all()[? dob.year <= 1940][0..3] { name, year: dob.year }" />

    <p>Here's a filter that uses Bifoql's built-in <code>starts_with</code> function. This example requires us to use the current context (<code>@</code>).</p>

    <Playpen query="customer.all().name[? starts_with(@, 'Melissa S')]" />

    <h2>Pipes</h2>

    <p>Bifoql allows you to create complex expressions </p>
</div>
);

