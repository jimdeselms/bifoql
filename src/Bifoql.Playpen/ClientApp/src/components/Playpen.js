import React, { Component } from 'react';

export class Playpen extends Component {
  displayName = Playpen.name;

  constructor(props) {
    super(props);
    this.state = { input: "{ name: 'Fred' }", query: 'Name', response: '', loading: true };

    var body = {
        query: "name",
        input: JSON.stringify({ name: 'Jim' })
    };

    fetch('api/Bifoql/WeatherForecasts', { 
        method: 'POST', 
        body: JSON.stringify(body),
        headers: {
            'Content-Type': 'application/json'
        }
     })
      .then(response => response.text())
      .then(data => {
        this.setState({ response: data, loading: false });
      });
  }

  static renderResults(input, query, response) {
    return (
      <div>
        <p>{input}</p>
        <p>{query}</p>
        <p>{response}</p>
      </div>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : Playpen.renderResults(this.state.input, this.state.query, this.state.response);

    return (
      <div>
        <h1>Playpen</h1>
        <p>You can edit the input data or query</p>
        {contents}
      </div>
    );
  }
}
