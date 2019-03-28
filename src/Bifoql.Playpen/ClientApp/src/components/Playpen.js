import React, { Component } from 'react';
import SplitterLayout from 'react-splitter-layout';
import 'react-splitter-layout/lib/index.css';
import './Playpen.css';

export class Playpen extends Component {
  displayName = Playpen.name;

  changeInput(event) {
    this.setState({ input: event.target.value });
  }

  changeQuery(event) {
    this.setState({ query: event.target.value });
  }

  runQuery() {
    this.setState({ loading: false });
    var body = {
      query: this.state.query,
      input: this.state.input
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

  constructor(props) {
    super(props);
    this.state = { input: "{ name: 'Fred' }", query: 'name', response: '', loading: false };
    this.runQuery();

    this.changeQuery = this.changeQuery.bind(this);
    this.changeInput = this.changeInput.bind(this);
    this.runQuery = this.runQuery.bind(this);
  }

  renderResults(input, query, response) {
    return (
        <div className='playpen-container'>
          <div className='playpen-inputs'>
              <div className='playpen-input'>
                <div>Input (JSON)</div>
                <textarea onBlur={this.runQuery} onChange={this.changeInput} value={this.state.input}></textarea>
              </div>
              <div className='playpen-query'>
                <div>Query</div>
                <textarea onBlur={this.runQuery} onChange={this.changeQuery} value={this.state.query}></textarea>
              </div>
          </div>
          <div className='playpen-output'>
            <button onClick={this.runQuery}>Run your Bifoql query</button>
            <div>Result</div>
            <pre>{response}</pre>
          </div>
      </div>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : this.renderResults(this.state.input, this.state.query, this.state.response);

    return (
      <div>
        <h1>Playpen</h1>
        <p>You can edit the input data or query</p>
        {contents}
      </div>
    );
  }
}
