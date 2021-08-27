import React, { Component } from "react";
import { Form, Input, Button, message } from "antd";
import { Redirect} from "react-router-dom";
import { UserOutlined, LockOutlined } from "@ant-design/icons";
//api
import {request} from '../../api'
//缓存
import memoryUtils from "../../utils/memoryUtils";
import storageUtils from "../../utils/storageUtils";

import logo from "../../asset/images/999.png";
import "./index.less";
const Item = Form.Item;
export default class login extends Component {
  formRef = React.createRef();
  handleSubmit = async (event) => {
    event.preventDefault();
    const form = this.formRef.current;
    const { username, password } = form.getFieldsValue(true);
    var formData = new FormData();
    formData.append("username", username);
    formData.append("password", password);
    const error = form.getFieldsError();
    if (error.find((item) => item.errors.length > 0)) {
      return;
    }
    const result = await request(formData,'Home/Login','POST');
    if (result.status === 0) {
      if (result.data == null || (result.data.Lw_UserInfo.length === 0 &&  result.data.Lw_GysInfo.length===0)) {
        message.warn("账号或密码错误");
        return;
      }
      let user;
      //本公司员工登录
      if(result.data.Lw_UserInfo.length!==0){
        if (
          result.data.Lw_UserInfo[0].Roles === "NULL" ||
          result.data.Lw_UserInfo[0].Roles === undefined ||
          result.data.Lw_UserInfo[0].Roles === ""
        ) {
          message.warn("账号还没有设置权限");
          return;
        }
        user = result.data.Lw_UserInfo[0];
        user.type='user';
      }else if(result.data.Lw_GysInfo.length!==0){
        user = result.data.Lw_GysInfo[0];
        user.type = 'gys';
      }
      message.success("登录成功");
      memoryUtils.user = user; //将当前登录用户保存到内存中
      storageUtils.saveUser(user);
      this.props.history.replace("/admin");
    }else if (result.status === 1){
      message.warn("账号密码错误");
    }else {
      message.error("服务器无响应");
    }
  };
  render() {
    const user = storageUtils.getUser();
    var { redict } = this.props.location;
    if (user && user.ID) {
      console.dir("1");
      if (redict !== true) {
        memoryUtils.user = user; //将当前登录用户保存到内存中
        return <Redirect to="/Admin" />;
      }
    }
    return (
      <div className="login">
        <div className="login-header">
          <img src={logo} alt="logo" className="Loginlogo"></img>
          <h1>农装供应商平台</h1>
        </div>
        <section className="login-content">
          <div>
            <h2>用户登录</h2>
            <Form ref={this.formRef} onSubmitCapture={this.handleSubmit}>
              <Item
                name="username"
                rules={[
                  { required: true, message: "请输入用户名" },
                  {
                    pattern: /^[a-zA-Z0-9_]+$/,
                    message: "用户名必须是数字、字母、下划线、",
                  },
                  { whitespace: true, message: "账号不能为空格" },
                ]}
              >
                <Input
                  placeholder="账号"
                  prefix={
                    <UserOutlined style={{ color: "rgba(0,0,0,0.25)" }} />
                  }
                />
              </Item>
              <Item
                name="password"
                rules={[
                  { required: true, message: "请输入密码" },
                  {
                    pattern: /^[a-zA-Z0-9_@]+$/,
                    message: "密码名必须是数字、字母、下划线、@符号",
                  },
                  { whitespace: true, message: "密码不能为空格" },
                ]}
              >
                <Input.Password
                  placeholder="密码"
                  autoComplete="off"
                  prefix={
                    <LockOutlined style={{ color: "rgba(0,0,0,0.25)" }} />
                  }
                />
              </Item>
              <Item>
                  <Button className="login-button" type="primary" htmlType="submit" >登录</Button>
              </Item>
            </Form>
          </div>
        </section>
      </div>
    );
  }
}
