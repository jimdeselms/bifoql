import React, { Component } from 'react';
const ReactMarkdown = require('react-markdown')

export class Home extends Component {
  displayName = Home.name;

  render() {

    const text = `
# Bifoql

This is where I can put some documentation for Bifoql!


    `;
        return (
          <div>
            <ReactMarkdown source={text}>
            </ReactMarkdown>
          </div>
    );
  }
}
