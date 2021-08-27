import React, { Component } from "react";
import { Modal, Input, Button, Upload, Tabs, Select, message ,Form, Badge} from "antd";
import {
  UploadOutlined,
  DownloadOutlined,
  PlusOutlined,
} from "@ant-design/icons";
//引入工具
import moment from "moment";
//引入内存
import { ServerMsg } from "../../api/Socket";

//引入css
import "./index.less";
const { TextArea } = Input;
const { TabPane } = Tabs;
const { Option } = Select;
/**
 * 传值
 * isShow:控制聊天框的显示
 * ChartShow OK按键事件
 * ChartHide 隐藏按键事件
 * width:聊天框宽度 默认为1200px
 */
export default class Chart extends Component {
  formRef = React.createRef();
  Socket = {};
  UserNow = 0;
  charDivs = [];
  state = {
    UserList: {}, //通信的用户
  };
  //发送消息
  SentMsg = () => {
    let Socket = this.Socket;
    const { UserList } = this.state;
    const { user } = this.props;
    const form = this.formRef.current;
    //getFieldsValue
    //setFieldsValue
    let ChartText = form.getFieldsValue(true).reply;
    //213312312
    if(ChartText.trim()===""){
      message.warn('请勿发送空消息');
      return;
    }
    if (Socket.readyState === 1) {
      //this.UserNow
      Socket.send(
        JSON.stringify({ target: this.UserNow, Msg: ChartText ,UserName:user.UserName})
      );
      UserList["user" + this.UserNow].MsgList.push({
        type: "Send",
        UserName: user.UserName,//发送人姓名
        SendeTime: moment().format("yyyy-MM-DD HH:mm:ss"),
        Msg: ChartText,
        Files: "",
        ID: moment().format("yyyy-MM-DD HH:mm:ss")
      });
      //将消息框移动到底部
      this.charDivs = [];
      //清空聊天数据
      form.setFieldsValue({"reply":""})
      //移动聊天窗口到底部
      this.setState({UserList},()=>{
        this.CharDivTopBtm();
      })
    } else {
      message.warn("无法连接服务器");
    }
  };
  componentDidMount = () => {
    let Socket = this.Socket;
    const {MessageShake} = this.props;
    if (Socket.readyState === undefined) {
      const { user } = this.props;
      Socket = ServerMsg(user.ID, user.type);
      Socket.onopen = () => {};
      Socket.onerror = () => {
        message.warn("无法连接服务器");
      };
      Socket.onmessage = (event) => {
        let data = JSON.parse(event.data);
        let { UserList } = this.state;
        switch (data.MsgType) {
          case "Init": //初始化的消息
            //这里的消息数据必须要按发送日期进行排序SendeTime
            //
            //获取通信的用户个数
            const { V_Lw_Msg } = data;
            let UserList2 = {};
            let FindFiled;
            let FindFiled2;
            if (user.type === "gys") {
              FindFiled = "UserID";
              FindFiled2 = "UserName";
            } else {
              FindFiled = "GysID";
              FindFiled2 = "GysName";
            }
            //存放当前未读数据
            let NotReadMsgList = {};
            V_Lw_Msg.forEach((item) => {
              //第一次读取
              if (UserList2["user" + item[FindFiled]] === undefined) {
                UserList2["user" + item[FindFiled]] = item;
                UserList2["user" + item[FindFiled]].UserID2 = item[FindFiled];
                UserList2["user" + item[FindFiled]].UserName2 =item[FindFiled2];
                UserList2["user" + item[FindFiled]].MsgList = [];
                let newObj = this.ServerMsgToClientMsg(item, user, FindFiled2);
                UserList2["user" + item[FindFiled]].MsgList.push(newObj);
                //有未读消息右下角震动 过滤掉自己发的消息 自己发的消息 没有已读的概念
                if(newObj.isRead==="False" && newObj.type==="Receive"){
                  //右下角添加消息
                  if(NotReadMsgList["user"+item[FindFiled]]=== undefined){
                    NotReadMsgList["user"+item[FindFiled]] = {ID:item[FindFiled],UserName:item[FindFiled2],Count:1}
                  }else{
                    NotReadMsgList["user"+item[FindFiled]].Count++;
                  }
                }
                if (this.UserNow === 0) {
                  this.UserNow = UserList2["user" + item[FindFiled]].UserID2;
                }
              } else {
                let newObj = this.ServerMsgToClientMsg(item, user, FindFiled2);
                //有未读消息右下角震动 过滤掉自己发的消息 自己发的消息 没有已读的概念
                if(newObj.isRead==="False" && newObj.type==="Receive"){
                    UserList2["user" + item[FindFiled]].isRead = newObj.isRead;
                    //右下角添加消息
                    if(NotReadMsgList["user"+item[FindFiled]]=== undefined){
                      NotReadMsgList["user"+item[FindFiled]] = {ID:item[FindFiled],UserName:item[FindFiled2],Count:1}
                    }else{
                      NotReadMsgList["user"+item[FindFiled]].Count++;
                    }
                    
                }
                UserList2["user" + item[FindFiled]].MsgList.push(newObj);
              }
            });
            console.dir(UserList2);
            //有未读消息，震动
            if(JSON.stringify(NotReadMsgList)!=="{}"){
              MessageShake(true,NotReadMsgList);
            }
            this.setState({ UserList: UserList2 });
            break;
          case "Msg": //日常的通信消息
            //SendID
            if (UserList["user" + data.SendID] !== undefined) {
              console.dir(data);
              UserList["user" + data.SendID].MsgList.push(data);
              this.setState({ UserList },()=>{
                this.CharDivTopBtm();
              });
            }
            break;
        }
      };
      this.Socket = Socket;
    } else {
      message.warn("无法连接服务器");
    }
  };
  componentWillUnmount = ()=>{
    //关闭Socket
      if(this.Socket.readyState !== undefined){
          this.Socket.close();
      }
  }
  //将服务端发送来的数据转化为客户端可用的
  ServerMsgToClientMsg = (item, user, FindFiled2) => {
    let newObj = {};
    if (user.type === item.MsgType) {
      newObj.type = "Send";
      newObj.UserName = user.UserName;
      //自己发的消息 全部都设置为已读
      newObj.isRead = "True";
    } else {
      newObj.type = "Receive";
      newObj.UserName = item[FindFiled2];
      newObj.isRead = item.isRead;
    }
    newObj.SendeTime = moment(item.SendeTime).format("yyyy-MM-DD HH:mm:ss");
    newObj.Msg = item.Msg;
    newObj.Files = item.Files;
    newObj.ReadTime = moment(item.ReadTime).format("yyyy-MM-DD HH:mm:ss");
    
    newObj.ID = item.ID;
    return newObj;
  };
  //聊天窗到底部
  CharDivTopBtm = ()=>{
    setTimeout(() => {
      //去掉其中多余的数组
      this.charDivs = this.charDivs.filter((item)=>{
        if(item===null){
          return false;
        }else{
          return true;
        }
      })
      console.dir(this.UserNow);
      //聊天窗口移动到底部
      this.charDivs.forEach(item=>{
        if(item===null){
          return;
        }
        if(item.dataset.id*1 === this.UserNow*1){
          item.scrollTop = item.scrollHeight;
        }
      })
    }, 210);
  }
  render() {
    const { UserList } = this.state;
    const { isShow } = this.props;
    return (
      <Modal
        width={this.props.width + "px"}
        style={{ top: "4%" }}
        visible={isShow}
        title="消息通知"
        onOk={() => this.props.ChartShow()}
        onCancel={() => this.props.ChartHide()}
        footer={
          <div className="folter_Div">
            <span className="folter_TextArea">
              <Form ref={this.formRef}>
                <Form.Item
                  name="reply"
                >
                  <TextArea
                    autoSize={{ minRows: 6, maxRows: 6 }}
                  >
                  </TextArea>
                </Form.Item>
              </Form>
            </span>
            <span className="folter_BtnArea">
              <Button
                className="btn"
                type="primary"
                onClick={() => this.SentMsg()}
              >
                回复
              </Button>
            </span>
            <span className="folter_UploadArea">
              <Upload listType="picture" className="upload-list-inline">
                <Button icon={<UploadOutlined />}>上传附件</Button>
              </Upload>
            </span>
          </div>
        }
      >
        <div className="body_Div">
          <Tabs
            className="body_Tabs"
            tabPosition={"left"}
            onTabClick={(key, event) => {
              //key当前要发送的用户
              this.UserNow = key; //更改聊天对象
            }}
            tabBarExtraContent={{
              left: (
                <div className="left">
                  <Select showSearch className="select">
                    <Option value="南京">南京</Option>
                    <Option value="上海">上海</Option>
                    <Option value="东京">东京</Option>
                    <Option value="北京">北京</Option>
                  </Select>
                  <Button className="btn" type="primary">
                    添加
                  </Button>
                </div>
              ),
            }}
          >
            {Object.keys(UserList).map((key) => {
                var node = <TabPane
                // error default
                  tab={<Badge status={UserList[key].isRead==="False"?"error":"default"} text={UserList[key].UserName2}></Badge>}
                  key={UserList[key].UserID2}
                >
                  <div data-id={UserList[key].UserID2} ref={(el)=>{
                    if(isShow&&el!==null){
                      el.scrollTop = el.scrollHeight;
                    }
                    this.charDivs.push(el);
                  }} className="chartBody" style={{overflowY:"auto"}}>
                    {UserList[key].MsgList.map((msg) => {
                      return msg.type === "Send" ? (
                        <div
                          key={"d" + msg.ID}
                          style={{ flexDirection: "column", display: "flex" }}
                        >
                          <span className="ChartReceiver">
                            {msg.SendeTime} {msg.UserName}
                          </span>
                          <span className="ChartContent">{msg.Msg}</span>
                        </div>
                      ) : (
                        <div
                          key={"d" + msg.ID}
                          style={{ flexDirection: "column", display: "flex" }}
                        >
                          <span className="ChartSenter">
                            {msg.SendeTime} {msg.UserName}
                          </span>
                          <span className="ChartContent">{msg.Msg}</span>
                        </div>
                      );
                    })}
                  </div>
                </TabPane>
                return node;
            })}
          </Tabs>
        </div>
      </Modal>
    );
  }
}
