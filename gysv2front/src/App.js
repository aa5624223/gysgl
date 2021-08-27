import React, { Component } from 'react'
import {Switch,Route,HashRouter,Redirect} from 'react-router-dom'

//引入页面
import admin from './pages/admin'
import login from './pages/login'
import SocketTest from './pages/SocketTest'
//引入样式
import './App.less'
export default class App extends Component {
    render() {
        return (
            <HashRouter>
                <Switch>
                    <Route path="/test" component={SocketTest}/>
                    <Route path="/admin" component={admin}/>
                    <Route path="/login" component={login}/>
                    <Redirect to="/login"></Redirect>
                </Switch>
            </HashRouter>
        )
    }
}
