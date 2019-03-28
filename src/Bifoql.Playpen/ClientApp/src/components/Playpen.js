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

  handleKeyPress(target) {
    // If you hit Ctrl-enter, it'll submit it without pressing the button.
    if (target.ctrlKey && target.key == 'Enter') {
        this.runQuery();
    }
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
    const initialInput = this.props.input || "{ name: 'Fred' }";
    const initialQuery = this.props.query || "name";

    this.state = { input: initialInput, query: 'name', initialQuery };
    this.runQuery();

    this.changeQuery = this.changeQuery.bind(this);
    this.changeInput = this.changeInput.bind(this);
    this.runQuery = this.runQuery.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
  } 

  renderResults(input, query, response) {
    return (
        <div className='playpen-container'>
          <div className='playpen-inputs'>
              <div className='playpen-input'>
                <div>Input (JSON)</div>
                <textarea onBlur={this.runQuery} onKeyDown={this.handleKeyPress} onChange={this.changeInput} value={this.state.input}></textarea>
              </div>
              <div className='playpen-query'>
                <div>Query</div>
                <textarea onBlur={this.runQuery} onKeyDown={this.handleKeyPress} onChange={this.changeQuery} value={this.state.query}></textarea>
              </div>
          </div>
          <div className='playpen-output'>
            <div>Result</div>
            <pre>{response}</pre>
          </div>
      </div>
    );
  }

  render() {
    let contents = this.renderResults(this.state.input, this.state.query, this.state.response);

    return (
      <div>
        <h1>Playpen</h1>
        <p>You can edit the input data or query below. The input should be in JSON format. Submit your query with Ctrl-Enter, or by clicking outside of the text box.</p>
        {contents}
      </div>
    );
  }
}
