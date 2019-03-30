import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { Glyphicon, Nav, Navbar, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import './NavMenu.css';

export class NavMenu extends Component {
  displayName = NavMenu.name

  render() {
    return (
      <Navbar inverse fixedTop fluid collapseOnSelect>
        <Navbar.Header>
          <Navbar.Brand>
            <Link to={'/'}>Bifoql</Link>
          </Navbar.Brand>
          <Navbar.Toggle />
        </Navbar.Header>
        <Navbar.Collapse>
          <Nav>
            <LinkContainer to={'/queries'} exact>
              <NavItem>
                Writing queries
              </NavItem>
            </LinkContainer>
            <LinkContainer to={'/integrating'} exact>
              <NavItem>
                Integrating with Bifoql
              </NavItem>
            </LinkContainer> 
            <LinkContainer to={'/vsgraphql'} exact>
              <NavItem>
                Why not GraphQL?
              </NavItem>
            </LinkContainer>
            <LinkContainer to={'/playpen'}>
              <NavItem>
                Try it out!
              </NavItem>
            </LinkContainer>
          </Nav>
        </Navbar.Collapse>
      </Navbar>
    );
  }
}
