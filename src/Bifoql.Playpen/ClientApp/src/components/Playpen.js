import React, { Component } from 'react';
import CodeMirror from 'react-codemirror';

import './Playpen.css';
import 'codemirror/lib/codemirror.css';
import 'codemirror/mode/javascript/javascript';

import 'codemirror/addon/display/rulers.js';

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
    const initialInput = this.props.input ? this.props.input : this.props.fullSize ? '{ person: { name: "Fred"}}' : "";
    const initialQuery = this.props.query ? this.props.query : this.props.fullSize ? 'person.name' : "";

    this.state = { input: initialInput, query: initialQuery };
    this.runQuery();

    this.changeQuery = this.changeQuery.bind(this);
    this.changeInput = this.changeInput.bind(this);
    this.runQuery = this.runQuery.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
  } 

  renderResults(input, query, response) {
    var space = "          ";
    var rulers = [];
      for (var i = 1; i <= 10; i++) {
        rulers.push({color: "#ddd", column: i * 10, lineStyle: "dashed"});
      };
      
    const options = { 
      mode: 'javascript',
      lineNumbers: true,
      extraKeys: {
        'Ctrl-O': this.handleKeyPress
      },
      blur: this.runQuery,
      keyHandled: this.RunQuery,
    };

    const containerClass = this.props.fullSize ? 'playpen-container' : 'playpen-container-compact';

    var input = this.state.input
      ? (
          <div className='playpen-input'>
            <div>Input (JSON)</div>
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeInput} value={this.state.input}></CodeMirror>
          </div>
        )
      : undefined;

    return (
        <div className={containerClass}>
          { this.props.fullSize ? <h1>Bifoql playpen</h1> : undefined }
          <p className='playpen-helptext'>You can edit the input data or query below. The input should be in JSON format. Submit your query with Ctrl-Enter, or by clicking outside of the text box.</p>
          { input }
          <div className='playpen-query'>
            { this.props.fullSize ? <div>Query</div> : undefined }
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeQuery} value={this.state.query}></CodeMirror>
          </div>
          <div className='playpen-output'>
            {/* <CodeMirror keyHandled={this.runQuery} options={ {...options, readOnly: true} } value={this.state.response}></CodeMirror> */}
            <pre onClick={this.runQuery}>{response}</pre>
          </div>
      </div>
    );
  }

  render() {
    return this.renderResults(this.state.input, this.state.query, this.state.response);
  }
}
