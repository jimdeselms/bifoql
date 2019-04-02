import React, { Component } from 'react';

export class GraphqlExample extends Component {

  constructor(props) {
    super(props);
  } 

  render() {
    return (
      <pre className='graphql-example'>
        {this.props.text}
      </pre>
    )
  }
}
