import React, { Component } from "react";
import { Switch, Redirect, Route } from "react-router-dom";
import { Layout, Breadcrumb, message,Modal,Popover,Input, Button,Upload,Tabs, Select} from "antd";
import LinkButton from "../../components/link-button";
import { MenuUnfoldOutlined, MenuFoldOutlined,MessageOutlined,UploadOutlined,DownloadOutlined,PlusOutlined} from "@ant-design/icons";
//引入工具类
import memoryUtils from "../../utils/memoryUtils";
import storageUtils from "../../utils/storageUtils";
import { depTree, OrderTree } from "../../utils";
import store from 'store'

//引入个人页面组件
import Chart from '../../components/Chart'
import LeftNav from "../../components/left-nav";
import logo from "../../asset/images/999.png";
//引入页面
import userConfig from "../userConfig";
import OptRecord from '../OptRecord'
import Policy1 from '../Policy1'
import Policy2 from '../Policy2'
import Policy3 from '../Policy3'
import Message1 from '../Message1'
import Message2 from '../Message2'
import "./index.less";
import { request } from "../../api";

const { Header, Footer, Sider, Content } = Layout;
const { TextArea } = Input;
const { TabPane } = Tabs;
export default class admin extends Component {
  state = {
    collapsed: false,
    menuList: [], //用户侧边栏
    NavTop:[],
    IsMessageShake:false,
    NewMsg:[],//未读消息序列
    ChartVisible:false//是否显示聊天窗
  };
  onCollapse = (collapsed) => {
    this.setState({ collapsed });
  };
  toggle = () => {
    const { collapsed } = this.state;
    this.setState({ collapsed: !collapsed });
  };
  //获取用户权限
  getAuth = async () => {
    const { history } = this.props;
    //获取用户的权限，和侧边栏
    let user = memoryUtils.user;
    let formData = new FormData();
    if (!user || !user.ID) {
      user = storageUtils.getUser();
      memoryUtils.user = user;
    }
    formData.append("type", user.type);
    formData.append("AuthConfig", user.Roles);
    const result = await request(formData, "Home/getSite_Roles", "POST");
    if (result.status === 0) {
      //登录成功
      const { Lw_Site_Roles } = result.data;
      //获取侧边栏
      let menuList = depTree(Lw_Site_Roles, 0);
      menuList = OrderTree(menuList);
      menuList = menuList.filter((item) => !item.isMobile);
      this.setState({
        menuList,
      });
      //用树
    } else {
      history.push({ pathname: "/Login", redict: true });
      message.error("账号还没有设置权限");
    }
  };
  //退出
  handleExit = () => {
    Modal.confirm({
      title: "确定退出当前用户吗?",
      okText: "确定",
      cancelText: "取消",
      onOk: () => {
        memoryUtils.user = {};
        storageUtils.removeUser();
        this.props.history.replace("Login");
      },
    });
  };
  componentDidMount = () => {
    let user = memoryUtils.user;
    const {NavTop} = this.state;
    //const {menuList} = this.state;
    if (!user || !user.ID) {
      user = storageUtils.getUser();
      memoryUtils.user = user;
    }
    if (user!==undefined && user.ID !==undefined && (user.Roles!==undefined || user.type==='gys' )) {
      this.getAuth();
    }
    const StoreNavTop = store.get("Admin_NavTop");
    if( (NavTop.length===undefined || NavTop.length===0) && StoreNavTop===undefined ){
     this.setState({NavTop:["首页"]}) 
    }else if(StoreNavTop!==undefined){
      this.setState({NavTop:StoreNavTop}) 
    }
  };
  changeNavTop = (val)=>{
    store.set("Admin_NavTop",val)
    this.setState({NavTop:val});
  }
  ChartShow = (e)=>{
    this.setState({ChartVisible:true,IsMessageShake:false,NewMsg:[]});
  }
  ChartHide = ()=>{
    this.setState({ChartVisible:false})
  }
  //右下角新消息震动
  MessageShake = (flg,NewMsg)=>{
    let {IsMessageShake} = this.state;
    if(IsMessageShake===flg){//两个状态一样时不触发setstate
      return;
    }else{
      this.setState({IsMessageShake:flg,NewMsg})
    }
  }
  render() {
    const { collapsed, menuList,NavTop, ChartVisible,IsMessageShake,NewMsg} = this.state;
    let user = memoryUtils.user;
    if (!user || !user.ID) {
      user = storageUtils.getUser();
    }
    if (!user || !user.ID) {
      return <Redirect to="/Login" />;
    }
    return (
      <Layout style={{ minHeight: "100vh" }}>
        <Sider
          theme="dark"
          collapsible
          collapsed={collapsed}
          trigger={null}
          onCollapse={this.onCollapse}
          className="left-nav"
        >
          <div className="left-nav-header">
            <img
              src={logo}
              alt="logo"
              className="logo"
              style={{ marginLeft: collapsed ? 12 : 0 }}
            ></img>
            {collapsed ? (
              ""
            ) : (
              <h1 style={{ minWidth: "160px" }}>供应商管理平台</h1>
            )}
          </div>
          <LeftNav menuList={menuList} changeNavTop={this.changeNavTop} />
        </Sider>
        <Layout className="site-layout">
          <Header
            className="site-layout-background Header"
            style={{ padding: 0 }}
          >
            {collapsed ? (
              <MenuUnfoldOutlined
                className="trigger"
                onClick={() => this.toggle()}
              ></MenuUnfoldOutlined>
            ) : (
              <MenuFoldOutlined
                className="trigger"
                onClick={() => this.toggle()}
              ></MenuFoldOutlined>
            )}

            <div className="Header-top">
              <span>欢迎，{user.UserName}</span>
              <LinkButton onClick={this.handleExit}>退出</LinkButton>
            </div>
          </Header>
          <Content style={{ margin: "0 16px" }}>
            <Breadcrumb style={{ margin: "16px 0" }}>
              {NavTop.map((item,index)=>{
                return (
                  <Breadcrumb.Item key={item+index}>{item}</Breadcrumb.Item>
                )
              })}
            </Breadcrumb>
            <div
              className="contentMain"
            >
              <Switch>
                <Redirect from="/Admin" exact to="/Admin/Message/Message1" />
                {/* 系统管理 */}
                {/* 用户管理 */}
                <Route path="/Admin/Admin/userConfig" component={userConfig} />
                {/* 操作记录 */}
                <Route path='/Admin/Admin/OptRecord' component={OptRecord}/>
                {/* 相关政策   */}
                {/* 配套政策 */}
                <Route path='/Admin/Policy/Policy1' component={Policy1}/>
                {/* 质量处理 */}
                <Route path='/Admin/Policy/Policy2' component={Policy2}/>
                {/* 财务政策 */}
                <Route path='/Admin/Policy/Policy3' component={Policy3}/>
                {/* 信息互动 */}
                {/* 公有通知 */}
                <Route path='/Admin/Message/Message1' component={Message1}/>
                {/* 私有通知 */}
                <Route path='/Admin/Message/Message2' component={Message2}/>
              </Switch>
            </div>
          </Content>
          <Footer style={{ textAlign: "center" }}>
            技术支持:常发农装 运营管理部
          </Footer>
        </Layout>
        <div className="BtmFixIcon">
          {IsMessageShake?
          <Popover content={
            <div style={{display:"flex",flexDirection:'column'}}>您有新消息
              {
                Object.keys(NewMsg).map((key)=>{
                  return (<span key={NewMsg[key].ID}>{NewMsg[key].UserName} {NewMsg[key].Count} 条</span>)
                })
              }
            </div>
            
          } trigger="hover">
          {/* shackAnimation */}
          <span className="BtmFixIcon_Outer shackAnimation" onClick={(e)=>this.ChartShow(e)}>
            <MessageOutlined  />
          </span>
        </Popover>
          :
          <span className="BtmFixIcon_Outer" onClick={(e)=>this.ChartShow(e)}>
            <MessageOutlined  />
          </span>
          }
        </div>
        <Chart isShow={ChartVisible} MessageShake={this.MessageShake} user={user} ChartShow={this.ChartShow} ChartHide={this.ChartHide} width={1200}></Chart>
      </Layout>
    );
  }
}
