import React, { Component } from 'react';

import { Playpen } from './Playpen';
import { GraphqlExample } from './GraphqlExample';

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

    <h2>Comparing GraphQL to Bifoql queries</h2>
    <p>
      In the following examples, we use test data similar to the data on <a href="https://graphql.github.io/learn/queries/">GraphQL's tutorial</a>. We'll run through some of the example GraphQL queries, and then show how
      to write the same query in Bifoql. We can also see other kinds of queries that <i>can't</i> be written in GraphQL.
    </p>

    <p>This GraphQL query gets the name of one of the heros from Star Wars:
      <GraphqlExample text={`{
  hero {
    name
  }
}`} /> and the result of the query is <GraphqlExample text={`{
  "data": {
    "hero": {
      "name": "R2-D2"
    }
  }
}`} /></p>

    <p>We can do the same thing in Bifoql. One difference here is that the "hero" takes an empty parameter. Here, "hero" is is an <b>index</b>, meaning it's a way of looking up a result given a set of arguments. And in this
    implementation, this index takes an optional "episode" argument, with a default value of "NEWHOPE".</p>

    <Playpen query={`hero() {
  name
}`} />

    <p>Another difference you can see is that the result set is smaller; it only contains the field that you asked for, "name". One of the key differences between Bifoql and GraphQL is that Bifoql allows you to be 
      very specific about how to structure your result. If the only bit of data we want is the name of our hero, we can use this query:
    </p>

    <Playpen query="hero().name" />

    <p>
    Here's another GraphQL example. In this example, we're looking up a value based on an input. In this query, we get the name and height for a single Star Wars character:
    </p>

    <pre className='graphql-example'>{`{
  human(id: "1000") {
    name
    height
  }
}`}</pre>

    <p>Bifoql's query is similar: </p>

    <Playpen query={`human(id: '1000') {
  name,
  height
}`} />

<h2>Manipulating the results of a query</h2>

  <p>Here's another GraphQL that shows how to convert a human's height into feet; this construct is built into the Star Wars character model defined in the tutorial</p>
  <pre className='graphql-example'>{`{
  human(id: "1000") {
    name
    height(unit: FOOT)
  }
}`}</pre>

  <p>This example also demonstrates that GraphQL has enum data types. Bifoql could support this particular use case too, though it doesn't have a concept of enums (yet?) However, Bifoql
    allows you to manipulate the results of your query, completely bypassing the need to have that conversion interface. In this example, we can create a variable, set it to the conversion
    rate for meters to feet, and then multipy the result (Bifoql supports all sorts of arithmetic operations.) We can also rename the field to make it clear that it's now in feet:</p>

    <Playpen query={`$metersToFeet = 3.28084;
human(id: "1000") {
  name,
  heightInFeet: height * $metersToFeet
}
`}/>

  <p>Additionally, Bifoql allows you to completely remap the response; if you want to return the height in both feet and meters, you can:</p>
  <Playpen query={`$metersToFeet = 3.28084;
human(id: "1000") {
  heightInMeters: height,
  heightInFeet: height * $metersToFeet
}
`}/>

<p>Now, in an ideal world, your API should already have a method to get the height in feet or meters. But if that feature doesn't already exist, you have to ask your API developer to add it. With
  Bifoql, you have much greater flexibility to modify the query response to suit your client's needs. 
</p>

<p>
  Another point that should probably be made is that even though the API might not have a way to get height in feet, it's pretty easy to figure that out in the client code. True! However, Bifoql allows
  you to push complexity out of your client-side code, allowing your client code to be much cleaner.
</p>

<h2>GraphQL aliases in Bifoql</h2>

<p>GraphQL allows you to remap field names using aliases.</p>
<GraphqlExample text={`{
  empireHero: hero(episode: EMPIRE) {
    name
  }
  jediHero: hero(episode: JEDI) {
    name
  }
}`} />

<p>So does Bifoql, though instead enums, we'll just use strings. In this query, we'll also just get the names to reduce the size of the result.</p>
<Playpen query={`{
  empireHero: hero(episode: 'EMPIRE').name,
  jediHero: hero(episode: 'JEDI').name
}`} />

<h2>GraphQL fragments in Bifoql</h2>

<p>In GraphQL, you can define "fragments" which allows you to define a set of fields that you can reuse in other subqueries.</p>
<GraphqlExample text={`{
  leftComparison: hero(episode: EMPIRE) {
    ...comparisonFields
  }
  rightComparison: hero(episode: JEDI) {
    ...comparisonFields
  }
}

fragment comparisonFields on Character {
  name
  appearsIn
  friends {
    name
  }
}`} />

<p>Bifoql can also do this, though the construct is called an "expression." Expressions in Bifoql are just snippets that can be reused. Expressions are defined by prefacing a snippet with the <code>&amp;</code> operator. And to
expand an expression, you can use the <code>*</code> operator. If you've ever done any C programming, you'll recognize these as the "reference" and "dereference" operators. This example also shows the pipe operator which
takes the value on the left side and pipes it into the left side.</p>

<Playpen query={`$heroFields = &( { name, appearsIn, friends { name } } );

{ 
  leftComparison: hero(episode: 'EMPIRE') | *$heroFields,
  rightComparison: hero(episode: 'JEDI') | *$heroFields
}`} />

<h2>GraphQL variables in Bifoql</h2>
<p>As we've seen earlier, Bifoql has the ability to define variables in the query itself. Though GraphQL doesn't allow variables defined inside the query, they can be provided as inputs into the query. Bifoql 
  supports this as well. Here's a GraphQL with variables:
</p>

<GraphqlExample code={`query HeroNameAndFriends($episode: Episode) {
  hero(episode: $episode) {
    name
    friends {
      name
    }
  }
}`} />

<p>And here's how you would do the same thing in Bifoql. And of course we can simplify the query to return just the names.</p>

<Playpen variables={ ( { episode: 'JEDI' } ) } query={`hero(episode: $episode) {
  name,
  friends: friends.name
}`}/>

<h2>Getting optional fields</h2>

<p>GraphQL has great type safety features. Bifoql, not yet. (coming!) GraphQL allows you to vary the output of a query based on the type of the results. In this GraphQL, 
  depending on the type of the Star Wars object returned, we can return different fields.
</p>

<GraphqlExample text={`
{
  search(text: "an") {
    __typename
    ... on Human {
      name
      height
    }
    ... on Droid {
      name
      primaryFunction
    }
    ... on Starship {
      name
      length
    }
  }
}`}/>

<p>Bifoql doesn't have a way to ask the type of an object, and if you don't ask for a field that isn't defined on the object, you'll get an error. But you can work around it with the <code>??</code> operator. 
</p>

<Playpen query={`search(text: 'an') {
  name,
  height: height ?? undefined,
  primaryFunction: primaryFunction ?? undefined,
  length: length ?? undefined
}`} />

<aside>The expression <code>height ?? undefined</code> will take the "height" property of the current object, and if that's null, it'll be an error. In Bifoql, if an expression resolves to null, undefined, or an
error, it can be replaced with another value with the <code>??</code> operator. You can also use this technique to specify substitute values.</aside>

<h2>Things you can't do with GraphQL</h2>

<p>Bifoql offers a number of features that don't exist in GraphQL. In the following examples, we'll work with the "search" index from the following examples. The search index does allow you to request every record,
  and as a result, could result in a large result set. If you're on a resource-constrained client (like mobile,) your Bifoql query can filter down your results to just what you need.
</p>

<p>Here's a query that uses a filter to get the names and heights of all characters who are taller than 1.75 meters. (Since some items in the list might not have a "height" property, the <code>??</code> converts
the error into 'false', causing the test criteria to fail.)</p>

<Playpen query={`search(text: '') [? height > 1.75 ?? false] { 
  name, 
  height 
}`} />

<p>And what if you just want to know how many things are returned by the query?</p>

<Playpen query={`// OR: search(text: '') | count(@)
count(search(text: ''))`} />

<p>This query demonstrates array slicing to return the first three rows.</p>

<Playpen query={"search(text: '')[0..3].name"} />

<aside>In a perfect world, your API will provide explicit indexes for filtering or paging your results; even though Bifoql allows you to filter the results server-side, if the underlying result set is very large, you're
  still putting on the server to filter the results down. But, if a client needs searching or filtering that doesn't already exist, your clients can get what they need without asking for more features.
</aside>

<p>GraphQL allows you to batch multiple requests together -- as does Bifoql -- however, it's not possible in GraphQL to feed the result of one query into another query. In Bifoql; there's nothing
  preventing you from using the result of a query to feed another one:
</p>

<h2>Summary</h2>

<p>GraphQL is a mature library that is supported in multiple languages. Bifoql is brand new. However, since Bifoql offers rich capabilities to filter and reshape the output, it can satisfy client use cases that the 
  API designers might have never considered.</p>

<p>Additionally, Bifoql allows complexity to be moved out of the client code and into the query; simpler query results means simpler, faster, easier-to-test client-side code.</p>

</div>);

