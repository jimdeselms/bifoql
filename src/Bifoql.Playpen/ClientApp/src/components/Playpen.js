import React, { Component } from 'react';
import CodeMirror from 'react-codemirror';

import './Playpen.css';
import 'codemirror/lib/codemirror.css';
import 'codemirror/mode/javascript/javascript';

export class Playpen extends Component {
  displayName = Playpen.name;

  changeInput(newText) {
    this.setState({ input: newText });
  }

  changeQuery(newText) {
    this.setState({ query: newText });
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

    this.state = { input: initialInput, query: initialQuery };
    this.runQuery();

    this.changeQuery = this.changeQuery.bind(this);
    this.changeInput = this.changeInput.bind(this);
    this.runQuery = this.runQuery.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
  } 

  renderResults(input, query, response) {
    const options = { 
      mode: 'javascript',
      lineNumbers: true,
      extraKeys: {
        'Ctrl-O': this.handleKeyPress
      },
      blur: this.runQuery,
      keyHandled: this.RunQuery,
    };

    const containerClass = this.props.compact ? 'playpen-container-compact' : 'playpen-container';

    var input = this.props.hideInput
      ? undefined
      : (
          <div className='playpen-input'>
            <div>Input (JSON)</div>
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeInput} value={this.state.input}></CodeMirror>
          </div>
        );

    return (
        <div className={containerClass}>
          { this.props.compact ? <strong className='playpen-header'>Bifoql playpen</strong> : <h1>Bifoql playpen</h1> }
          <p className='playpen-helptext'>You can edit the input data or query below. The input should be in JSON format. Submit your query with Ctrl-Enter, or by clicking outside of the text box.</p>
          { input }
          <div className='playpen-query'>
            <div>Query</div>
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeQuery} value={this.state.query}></CodeMirror>
          </div>
          <div className='playpen-output'>
            <button onClick={this.runQuery}>Run query</button>
            <pre>{response}</pre>
          </div>
      </div>
    );
  }

  render() {
    return this.renderResults(this.state.input, this.state.query, this.state.response);
  }
}
