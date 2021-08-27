using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gysv2.Socket
{
    public class TestSocket
    {
        private string msg = "默认信息";
        public void socketServer()
        {
            
            string serverIP = "ws://localhost:44301/Api/Socket";
            List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
            WebSocketServer server = new WebSocketServer(serverIP);
            server.Start(socket =>
            {
                //打开连接
                socket.OnOpen = () =>
                {
                    allSockets.Add(socket);
                };
                //关闭连接
                socket.OnClose = () =>
                {
                    allSockets.Remove(socket);
                };
                //接收到客户端的消息
                socket.OnMessage = (message) =>
                {
                    int i = 0;
                    socket.Send("Has receive the message");
                };
            });
        }
    }
}