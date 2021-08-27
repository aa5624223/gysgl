import React, { Component } from "react";
import {
  message,
  Row,
  Col,
  Input,
  Button,
  Table,
  Modal,
  Form,
  Tabs,
  Checkbox,
} from "antd";
import { ExclamationCircleOutlined } from "@ant-design/icons";
//获取API
import { request } from "../../api";
//引入工具类
import memoryUtils from "../../utils/memoryUtils";
import localStore from "../../utils/storageUtils";
import { depTree, OrderTree, ConvertFomrData } from "../../utils";
//获取配置
import { UserConfig_columns } from "../../config/table-columns";
import "./index.less";
//变量设置
const { Search } = Input;
const { TabPane } = Tabs;
const { confirm } = Modal;
/**
 * 用户管理
 */
export default class userConfig extends Component {
  formRef = React.createRef();
  load_Add = false;
  state = {
    dataSource: [], //左侧用户的数据
    dataTotal: 0, //左侧用户数据总数
    selectedRowKeys: [], //用户表格的选择项
    myTabs: [], //所有的标签
    checkList: [], //用户选择的权限
    CopyRowKeys:[],//赋值权限的列表
    //控制
    ModalTitle: "", //弹出窗的标题
    isModalEditShow: "", //是否显示弹出窗
    load_Edit: false,
    loading: false, //控制用户的加载
  };
  //更改用户的权限配置
  submitChange = async () => {};
  //添加用户
  ModalAdd = () => {
    this.setState({ isModalEditShow: true, ModalTitle: "新建用户" }, () => {
      this.formRef.current.resetFields();
    });
  };
  //删除用户对话框
  ModalDel = (record, event) => {
    event.stopPropagation();
    event.stopPropagation();
    confirm({
      title: "是否删除数据?",
      icon: <ExclamationCircleOutlined />,
      okText: "是",
      okType: "danger",
      cancelText: "否",
      onOk: async () => {
        let formData = new FormData();
        formData.append("ID", record.ID);
        const result = await request(formData, "Home/delUser", "POST");
        if (result.status === 0) {
          message.success("删除成功");
          this.SearchData(); //重新查询数据
        }
      },
      onCancel() {},
    });
  };
  //编辑用户
  ModalEdit = (record, event) => {
    event.stopPropagation();
    this.setState({ isModalEditShow: true, ModalTitle: "编辑用户" }, () => {
      this.formRef.current.setFieldsValue(record);
    });
  };
  //查询按钮事件
  onSearch = value =>{
    this.SearchData({UserName:value});
  }
  //查询用户数据
  SearchData = async (condition) => {
    var FormData = ConvertFomrData(condition);
    this.setState({ loading: true });
    const result = await request(FormData, "Home/getUserList", "POST");
    if (result.status === 0) {
      if (result.data == null) {
        this.setState({loading:false});
        message.warn("没有查到用户数据");
        return;
      }
      this.setState({
        dataSource: result.data.Lw_UserInfo,
        dataTotal: result.data.Lw_UserInfo_Count,
        loading: false,
      });
    } else {
      this.setState({loading:false});
      message.warn("没有查到用户数据");
    }
    //FormData.page
    //FormData.pageSize
  };
  //获取用户所拥有的权限
  getSite_Roles = async () => {
    let formData = new FormData();
    let myTabs;
    const user = localStore.getUser();
    console.dir(user);
    if(user ===undefined || user.Roles===undefined){
      return;
    }
    formData.append("AuthConfig", user.Roles);
    const result = await request(formData, "Home/getSite_Roles", "POST");
    if (result.status === 0) {
      const { Lw_Site_Roles } = result.data;
      let menuList = depTree(Lw_Site_Roles, 0);
      menuList = OrderTree(menuList);
      myTabs = menuList.filter((item) => {
        if (item.isAuth) {
          return true;
        } else {
          return false;
        }
      });
      this.setState({ myTabs });
    } else {
      message.error("服务器无响应");
    }
  };
  ModalEditOk = async () => {
    if (this.load_Add) {
      message.warn("请勿重复提交!");
      return;
    }
    const form = this.formRef.current;
    try {
      await form.validateFields();
    } catch (errorInfo) {
      message.warn("请检查数据的正确性");
      return;
    }
    const { ModalTitle } = this.state;
    this.load_Add = true;
    try {
      if (ModalTitle === "编辑用户") {
        let data = form.getFieldsValue(true);
        let formData = new FormData();
        formData.append("ID", data.ID);
        formData.append("PWD", data.PWD);
        formData.append("UserCode", data.UserCode);
        formData.append("UserName", data.UserName);

        const result = await request(formData, "Home/editUser", "POST");
        if (result.status === 0) {
          message.success("编辑用户成功");
          this.SearchData(); //重新查询数据
        } else if (result.status === 2) {
          message.warn("改编号已经被其他用户占用");
        } else {
          message.error("网络错误");
        }
      } else {
        //新建用户
        let data = form.getFieldsValue(true);
        let formData = new FormData();
        formData.append("PWD", data.PWD);
        formData.append("UserCode", data.UserCode);
        formData.append("UserName", data.UserName);
        const result = await request(formData, "Home/addUser", "POST");
        if (result.status === 0) {
          message.success("添加用户成功");
          this.SearchData(); //重新查询数据
        } else if (result.status === 2) {
          message.warn("改编号已经被其他用户占用");
        } else {
          message.error("网络错误");
        }
      }
      this.setState({ isModalEditShow: false });
    } catch (_e) {
    } finally {
      this.load_Add = false;
    }
  };
  ModalEditCancel = () => {
    this.setState({ isModalEditShow: false });
  };
  //获取标签
  getTabs = () => {
    const { myTabs, checkList } = this.state;
    if (myTabs !== undefined && myTabs.length > 0) {
      return (
        <Tabs defaultActiveKey="1">
          {myTabs.map((item) => {
            return (
              <TabPane
                tab={item.isMobile ? "(手机版)" + item.title : item.title}
                key={item.key}
              >
                {item.children.map((item2) => {
                  if (item2.isAuth) {
                    let checked1;
                    if (checkList.length > 0) {
                      checked1 = checkList.find((it) => {
                        return it * 1 === item2.id;
                      });
                    }
                    return (
                      <div key={item2.key + "_div"}>
                        <h2>
                          <Checkbox
                            checked={checked1 !== undefined}
                            style={{ fontWeight: 600 }}
                            onChange={(e) => this.CheckOnchange(e)}
                            valueProp={item2.id}
                          >
                            {" "}
                            {item2.title}
                          </Checkbox>
                        </h2>
                        &emsp;&emsp;
                        {item2.children === undefined
                          ? ""
                          : item2.children.map((item3) => {
                              let checked2;
                              if (checkList.length > 0) {
                                checked2 = checkList.find(
                                  (it) => it * 1 === item3.id
                                );
                              }
                              if (item3.isAuth) {
                                return (
                                  <Checkbox
                                    checked={checked2 !== undefined}
                                    key={item3.key + "_checkBox"}
                                    onChange={(e) => this.CheckOnchange(e)}
                                    valueProp={item3.id}
                                  >
                                    {item3.title}
                                  </Checkbox>
                                );
                              } else {
                                return "";
                              }
                            })}
                      </div>
                    );
                  } else {
                    return "";
                  }
                })}
              </TabPane>
            );
          })}
        </Tabs>
      );
    }
  };
  //用户表格的选择
  //表格选择配置
  onSelectChange = (selectedRowKeys) => {
    this.setState({ selectedRowKeys });
  };
  //表格行配置
  onRow = record => {
    return {
        onClick: event => {// 点击行事件 获取当前需要修改的keys
            //Roles
            let checkList = record.Roles.split(',');
            this.setState({ selectedRowKeys: [record.ID], checkList });
        },
    }
  }
  //用户选择权限 复选框 选择
  CheckOnchange = (event) => {
    const { checkList } = this.state;
    let oldCheckList;
    const val = event.target.valueProp;
    if (event.target.checked) {
      checkList.push(val + "");
      this.setState({ checkList });
    } else {
      oldCheckList = checkList.filter((item) => item * 1 !== val);
      this.setState({ checkList: oldCheckList });
    }
  };
  //更改权限
  submitChange = async () => {
    //this.state.checkList 更改的list
    //selectedRowKeys 更改的id
    const { selectedRowKeys, checkList } = this.state;
    if (
      selectedRowKeys[0] === undefined ||
      selectedRowKeys[0] === 0 ||
      selectedRowKeys[0] === ""
    ) {
      message.warn("请先选择用户");
      return;
    }
    var List = checkList.join(",");
    if (List.length !== 0 && List[0] === ",") {
      List = List.substr(1);
    }
    //接下来提交数据
    var formData = new FormData();
    formData.append("ID", selectedRowKeys[0]);
    formData.append("Roles", List);
    this.setState({ load_Edit: true });
    const result = await request(formData,'Home/editUser','POST');
    if (result.status === 0) {
      message.success("更改权限成功");
      this.SearchData(); //重新查询数据
      this.setState({ load_Edit: false });
    }
  };
  //复制权限
  ModalCopy = (record,event)=>{
    event.stopPropagation();
    this.setState({CopyRowKeys:record.Roles.split(',')})
    message.success("权限已复制");
  }
  //粘贴权限
  ModalPaste = (record,event)=>{
    event.stopPropagation();
    const {CopyRowKeys} = this.state;
    console.dir(record.ID);
    this.setState({checkList:CopyRowKeys,CopyRowKeys:[],selectedRowKeys: [record.ID]})
    message.success("权限已粘贴");
  }
  componentDidMount = () => {
    this.SearchData();
    this.getSite_Roles();
  };
  render() {
    const {
      dataSource,
      dataTotal,
      load_Edit,
      loading,
      selectedRowKeys,
      ModalTitle,
      isModalEditShow,
    } = this.state;
    const rowSelection = {
      columnWidth: "16px",
      checkStrictly: true,
      type: "radio",
      fixed: true,
      selectedRowKeys: selectedRowKeys,
      onChange: this.onSelectChange,
    };
    const Column = UserConfig_columns(this);
    return (
      <Row className="Content-main main" style={{minHeight:'650px'}}>
        <Col span={1}></Col>
        <Col span={10} className="left_content">
          <Row style={{ marginBottom: "10px" }}>
            <Col span={12}>
              <Search
                placeholder="输入用户名搜索"
                enterButton
                onSearch={this.onSearch}
              />
            </Col>
            <Col span={1}></Col>
            <Col span={11}>
              <Button
                type="primary"
                onClick={() => this.submitChange()}
                loading={load_Edit}
              >
                提交更改
              </Button>
              &emsp;
              <Button type="primary" onClick={() => this.ModalAdd()}>
                添加
              </Button>
            </Col>
          </Row>
          <Table
            className="TabUserConfig"
            dataSource={dataSource}
            bordered
            rowKey="ID"
            sticky={true}
            columns={Column}
            scroll={{ y: 600 }}
            size="small"
            loading={loading}
            pagination={false}
            rowSelection={rowSelection}
            onRow={this.onRow}
          ></Table>
        </Col>
        <Col span={1}></Col>
        <Col span={12}>{this.getTabs()}</Col>
        <Modal
          title={ModalTitle}
          visible={isModalEditShow}
          onOk={() => this.ModalEditOk()}
          onCancel={() => this.ModalEditCancel()}
        >
          <Form ref={this.formRef}>
            <Form.Item
              name="UserCode"
              label="用户编号"
              rules={[
                {
                  required: true,
                  message: "请输入用户编号",
                },
              ]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name="UserName"
              label="用户名称"
              rules={[
                {
                  required: true,
                  message: "请输入用户名称",
                },
              ]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name="PWD"
              label="用户密码"
              rules={[
                {
                  required: true,
                  message: "请输入用户密码",
                },
              ]}
            >
              <Input />
            </Form.Item>
          </Form>
        </Modal>
      </Row>
    );
  }
}
