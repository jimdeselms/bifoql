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

  changeVariables(newText) {
    this.setState({ variables: newText });
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
    var variables = null;
    try {
      variables = this.state.variables ? JSON.parse(this.state.variables) : null;
      this.setState({ validVariables: true});
    } catch (ex) {
      this.setState({ validVariables: false});
    }

    var body = {
      query: this.state.query,
      input: this.state.input,
      arguments: variables
  };

  if (!this.props.readOnly) {
    fetch('api/Bifoql', { 
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
}

  constructor(props) {
    super(props);
    const initialInput = this.props.input ? this.props.input : "";
    const initialQuery = this.props.query ? this.props.query : this.props.fullSize ? `customer.byId(id: 100) {
  name,
  address {
    street,
    zipCode
  }
}` : "";
    const initialVariables = this.props.variables ? this.props.variables : this.props.fullSize ? {} : "";

    this.state = { 
      input: initialInput, 
      query: initialQuery,
      variables: initialVariables ? JSON.stringify(initialVariables, null, 2) : '',
      showInput: !!initialInput,
      showVariables: !!initialVariables,
      validVariables: true,
    };

    this.runQuery();

    this.changeQuery = this.changeQuery.bind(this);
    this.changeInput = this.changeInput.bind(this);
    this.changeVariables = this.changeVariables.bind(this);
    this.runQuery = this.runQuery.bind(this);
    this.handleKeyPress = this.handleKeyPress.bind(this);
  } 

  renderResults(response) {
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
      readOnly: this.props.readOnly,
      blur: this.runQuery,
      keyHandled: this.RunQuery,
    };

    const containerClass = this.props.fullSize ? 'playpen-container' : 'playpen-container-compact';

    var input = this.state.showInput
      ? (
          <div className='playpen-input'>
            <div>Input (JSON)</div>
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeInput} value={this.state.input}></CodeMirror>
          </div>
        )
      : undefined;

    var variables = this.state.showVariables
      ? (
          <div className='playpen-input'>
            <div>Variables (JSON)</div>
            { this.state.validVariables ? undefined : <div className='playpen-variable-error'>Invalid JSON</div> }
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeVariables} value={this.state.variables}></CodeMirror>
          </div>
        )
      : undefined;

    var output = this.props.readOnly
        ? undefined
        : (
          <div className='playpen-output'>
            <pre onClick={this.runQuery}>{response}</pre>
          </div>
        );

    return (
        <div className={containerClass}>
          { this.props.fullSize ? <h1>Bifoql playpen</h1> : undefined }
          <p className='playpen-helptext'>You can edit the input data or query below. The input should be in JSON format. Submit your query with Ctrl-Enter, or by clicking outside of the text box.</p>
          { input }
          { variables }
          <div className='playpen-query'>
            { this.props.fullSize ? <div>Query</div> : undefined }
            <CodeMirror keyHandled={this.runQuery} options={options} onFocusChange={this.runQuery} onChange={this.changeQuery} value={this.state.query}></CodeMirror>
          </div>
          { output }
      </div>
    );
  }

  render() {
    return this.renderResults(this.state.response);
  }
}
