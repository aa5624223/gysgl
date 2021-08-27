import React, { Component } from 'react'
export default class SocketTest extends Component {
    ws = {};
    //当连接失败重写连接 设置连接上限为5次 不再连接
    ReConnect = 0;
    state = {
        SocketMsg:[]
    }
    ClientSocket = ()=>{
        //ws://127.0.0.1:8181/Api/Socket
        //ws://localhost:44301/Api/Socket
        //ws://127.0.0.1:44301/Api/Socket
        let username = '123';
        this.ws = new WebSocket(`ws://127.0.0.1:44302/Api/Socket??UserID=${username}`);
        this.ws.onclose = ()=>{
            console.log("Close");
        }
        this.ws.onerror = ()=>{
            console.log("Erro");
        }
        this.ws.onopen = (event)=>{
            this.ws.send("has Open");
            console.log("open");
        }
        this.ws.onmessage = (event)=>{
            let data = event.data;
            console.log("Server Message:"+data);
        }
    }
    componentDidMount = ()=>{
        setTimeout(() => {
            this.ClientSocket();
        }, 3000);
    }
    render() {
        return (
            <div>
            </div>
        )
    }
}
