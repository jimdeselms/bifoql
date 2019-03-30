import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Playpen } from './components/Playpen';
import { Queries } from './components/Queries';
import { VsGraphql } from './components/VsGraphql';
import { Integrating } from './components/Integrating';

export default class App extends Component {
  displayName = App.name

  render() {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/queries' component={Queries} />
        <Route path='/integrating' component={Integrating} />
        <Route path='/vsgraphql' component={VsGraphql} />
        <Route path='/playpen' component={() => <Playpen fullSize={true} />} />
      </Layout>
    );
  }
}
