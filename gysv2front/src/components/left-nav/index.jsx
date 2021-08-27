import React, { Component } from "react";
import { Link, withRouter } from "react-router-dom";
import { Menu } from "antd";
//引入虚拟数据
import { sideData } from "../../anaData";
import "./index.less";
const { SubMenu } = Menu;
/*
    左侧导航组件
*/
class LeftNav extends Component {
  state = {
    sideList: [],
  };
  //生成侧边栏
  getSideMenu_Map = (menuList,PreEle) => {
    const pathname = this.props.history.location.pathname;
    const {changeNavTop} = this.props;
    return menuList.reduce((pre, item) => {
      //找到要展开的菜单栏
      let cItem = undefined;
      if (item.children !== undefined) {
        cItem = item.children.find((cItem) => {
          return pathname.indexOf(cItem.key) === 0;
        });
      }
      if (cItem) {
        this.openKey = item.key;
      }
      //遍历生成侧边栏
      if (!item.isSubMenu) {
        if (item.isOpt) {
          //调用的是函数
          pre.push(
            <Menu.Item onClick={() => this[item.key]()} key={item.key}>
              {item.title}
            </Menu.Item>
          );
        } else {
          pre.push(
            <Menu.Item key={item.key}>
              <Link to={item.key} onClick={()=>changeNavTop([PreEle.title,item.title])}>
                <span>{item.title}</span>
              </Link>
            </Menu.Item>
          );
        }
      } else {
        pre.push(
          <SubMenu key={item.key} title={item.title}>
            {this.getSideMenu_Map(item.children,item)}
          </SubMenu>
        );
      }
      return pre;
    }, []);
  };
  componentDidMount = () => {
    this.setState({ sideList: sideData });
  };
  render() {
    const { sideList } = this.state;
    const openKey = this.openKey;
    const pathname = this.props.history.location.pathname;
    const {menuList} = this.props;
    const MenuListEle = this.getSideMenu_Map(menuList);
    return (
      <Menu
        theme="dark"
        mode="inline"
        selectedKeys={[pathname]}
        defaultOpenKeys={[openKey]}
      >
        {MenuListEle}
      </Menu>
    );
  }
}
export default withRouter(LeftNav);
