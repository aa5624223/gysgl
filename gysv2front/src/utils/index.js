import XLSX from "xlsx";
import { Button, Space, Input, DatePicker, Form } from "antd";
import FileSaver from "file-saver";

import { SearchOutlined } from "@ant-design/icons";
import Highlighter from "react-highlight-words";

import moment from "moment";
import { request } from "../api";
import localStore from "./storageUtils";
//引入缓存
import { SERVER_ADDRESS_FILEPATH, SERVER_ADDRESS_DEMOPATH } from "./StaticSet";
/*
    常用的方法集合
*/
//遍历侧边栏树
export const depTree = (Roles, parentId) => {
  const items = [];
  let flg = true; //是否结束递归
  for (let i = 0; i < Roles.length; i++) {
    if (Roles[i].parentId * 1 === parentId) {
      if (Roles[i].UrlKey !== undefined) {
        items.push(createMenuObj(Roles[i]));
      } else {
        items.push(Roles[i]);
      }
      flg = false;
    }
  }
  if (flg) {
    return [];
  } else {
    for (let i = 0; i < items.length; i++) {
      items[i].children = depTree(Roles, items[i].id);
    }
    return items;
  }
};
//排序侧边栏树
export const OrderTree = (items) => {
  for (let i = 0; i < items.length; i++) {
    for (let j = 0; j < items.length; j++) {
      if (items[i].cOrder < items[j].cOrder) {
        let temp = items[i];
        items[i] = items[j];
        items[j] = temp;
      }
    }
  }
  for (let i = 0; i < items.length; i++) {
    if (items[i].children.length !== 0) {
      items[i].children = OrderTree(items[i].children);
    }
  }
  return items;
};
export const createMenuObj = (obj) => {
  let rtObj = {};
  rtObj.id = obj.ID * 1;
  rtObj.title = obj.title;
  rtObj.key = obj.UrlKey;
  rtObj.isOpt = obj.isOpt === "True";
  rtObj.cOrder = obj.cOrder * 1;
  rtObj.isMenu = obj.isMenu === "True";
  rtObj.parentId = obj.parentId * 1;
  rtObj.children = [];
  rtObj.isAuth = obj.isAuth === "True";
  rtObj.isSubMenu = obj.isSubMenu === "True";
  return rtObj;
};
//查找用户 拥有该页面的哪些权限
export const getPageRoles = async (mapKey) => {
  var formData = new FormData();
  formData.append("mapKey", mapKey);
  formData.append("Roles", localStore.getUser().Roles);
  let result = await request(formData,'Home/getSite_Roles_Opt','POST');
  if (result.status === 0) {
    return result.data.Site_Roles;
  } else {
    return [];
  }
};
//查找用户 是否拥有操作权限
export const isOpt = (Arr, title) => {
  const flg = Arr.find((item) => {
    if (item.title === title) {
      return true;
    } else {
      return false;
    }
  });
  return flg !== undefined;
};
//将Json对象转为formData
export const ConvertFomrData = (jsons) => {
  let formData1 = new FormData();
  if(jsons===undefined || jsons==={}){
    return formData1;
  }
  Object.keys(jsons).forEach((key) => {
    if (typeof jsons[key] == "object") {
      if (
        jsons[key] !== null &&
        jsons[key] !== undefined &&
        jsons[key] !== ""
      ) {
        //console.dir(jsons[key]);
        formData1.append(key + "[0]", jsons[key][0].format("yyyy-MM-DD"));
        formData1.append(key + "[1]", jsons[key][1].format("yyyy-MM-DD"));
      }
    } else {
      formData1.append(key, jsons[key]);
    }
  });
  return formData1;
};
//将json数据转为EXCEL进行下载
//json 原始的数据
//Colums Table的列List
function JsonTOexcelData(datas, Colums) {
  //const type = 'xlsx';
  let ExcelDatas = [];
  datas.forEach((data) => {
    let ExcelData = {};
    Colums.forEach((Colum) => {
      ExcelData[Colum.title] = data[Colum.key];
    });
    ExcelDatas.push(ExcelData);
  });
  return ExcelDatas;
}
/**
 * json 数据
 * Colums 标题
 * ColumsWch 每列的宽度
 * fileName 文件名
 */
export function downloadExcel(json, Header, ColumsWch, fileName) {
  console.dir(json);
  json = JsonTOexcelData(json, Header);
  const workbook = XLSX.utils.book_new();
  let ws = XLSX.utils.json_to_sheet(json);
  ws["!cols"] = ColumsWch;
  XLSX.utils.book_append_sheet(workbook, ws, "sheet1"); //把sheet添加到workbook里，第三个参数是sheet名
  const wopts = { bookType: "xlsx", bookSST: false, type: "array" }; //写入的样式bookType:输出的文件类型，type：输出的数据类型，bookSST: 是否生成Shared String Table，官方解释是，如果开启生成速度会下降，但在低版本IOS设备上有更好的兼容性
  const wbout = XLSX.write(workbook, wopts);
  FileSaver.saveAs(
    new Blob([wbout], { type: "application/octet-stream" }),
    `${fileName} ${moment().format("YYYYMMDDHHmmss")}.xlsx`
  ); //保存文件
}

export function downloadUpFile(fileName) {
  window.open(SERVER_ADDRESS_FILEPATH + fileName);
}
export function downloadDemoFile(fileName) {
  window.open(SERVER_ADDRESS_DEMOPATH + fileName);
}
export const getColumnSearchProps = (dataIndex, _this) => ({
  filterDropdown: ({
    setSelectedKeys,
    selectedKeys,
    confirm,
    clearFilters,
  }) => (
    <div style={{ padding: 8 }}>
      <Input
        ref={(node) => {
          _this.searchInput = node;
        }}
        placeholder={`输入查询条件`}
        value={selectedKeys[0]}
        onChange={(e) =>
          setSelectedKeys(e.target.value ? [e.target.value] : [])
        }
        onPressEnter={() =>
          _this.SearchData({[dataIndex]:selectedKeys[0]})
        }
        style={{ width: 188, marginBottom: 8, display: "block" }}
      />
      <Space>
        <Button
          type="primary"
          onClick={() => _this.SearchData({[dataIndex]:selectedKeys[0]})}
          icon={<SearchOutlined />}
          size="small"
          style={{ width: 90 }}
        >
          查询
        </Button>
        <Button
          onClick={() => {
            clearFilters();
            _this.SearchData({[dataIndex]:""})
          }
            
          }
          size="small"
          style={{ width: 90 }}
        >
          重置
        </Button>
      </Space>
    </div>
  ),
  filterIcon: (filtered) => (
    <SearchOutlined style={{ color: filtered ? "#1890ff" : undefined }} />
  ),

  onFilterDropdownVisibleChange: (visible) => {
    if (visible) {
      setTimeout(() => _this.searchInput.select(), 100);
    }
  },
  render: (text) =>
    _this.state.searchedColumn === dataIndex ? (
      <Highlighter
        highlightStyle={{ backgroundColor: "#ffc069", padding: 0 }}
        searchWords={[_this.state.searchText]}
        autoEscape
        textToHighlight={text ? text.toString() : ""}
      />
    ) : (
      text
    ),
});
//带选择框的
export const getColumnSearchPropsSelect = (dataIndex, _this, key) => ({
  filterDropdown: ({
    setSelectedKeys,
    selectedKeys,
    confirm,
    clearFilters,
  }) => (
    <div style={{ padding: 8 }}>
      <Form layout="inline">
        <Form.Item label="物料层级">
          <select
            ref={(node) => {
              _this[key] = node;
            }}
          >
            <option value="1">一</option>
            <option value="2">二</option>
            <option value="3">三</option>
            <option value="4">四</option>
            <option value="5">五</option>
          </select>
        </Form.Item>
      </Form>
      <Input
        ref={(node) => {
          _this.searchInput = node;
        }}
        placeholder={`输入查询条件`}
        value={selectedKeys[0]}
        onChange={(e) =>
          setSelectedKeys(e.target.value ? [e.target.value] : [])
        }
        onPressEnter={() =>
          _this.handleSearch(selectedKeys, confirm, dataIndex)
        }
        style={{ width: 188, marginBottom: 8, display: "block" }}
      />
      <Space>
        <Button
          type="primary"
          onClick={() => _this.handleSearch(selectedKeys, confirm, dataIndex)}
          icon={<SearchOutlined />}
          size="small"
          style={{ width: 90 }}
        >
          查询
        </Button>
        <Button
          onClick={() => {
            clearFilters();
            _this.handleReset(clearFilters)
          }
          }
          size="small"
          style={{ width: 90 }}
        >
          重置
        </Button>
      </Space>
    </div>
  ),
  filterIcon: (filtered) => (
    <SearchOutlined style={{ color: filtered ? "#1890ff" : undefined }} />
  ),

  onFilterDropdownVisibleChange: (visible) => {
    if (visible) {
      setTimeout(() => _this.searchInput.select(), 100);
    }
  },
  render: (text) =>
    _this.state.searchedColumn === dataIndex ? (
      <Highlighter
        highlightStyle={{ backgroundColor: "#ffc069", padding: 0 }}
        searchWords={[_this.state.searchText]}
        autoEscape
        textToHighlight={text ? text.toString() : ""}
      />
    ) : (
      text
    ),
});
//日期类型
export const getColumnSearchPropsDate = (dataIndex, _this) => ({
  filterDropdown:({setSelectedKeys,selectedKeys,confirm,clearFilters})=>(
    <div style={{padding:8}}>
      <DatePicker.RangePicker
        showTime={{ format: 'HH:mm' }}
        format="YYYY-MM-DD HH:mm"
        onChange={value=>setSelectedKeys(value)}
        onPressEnter = {()=>_this.SearchData({[dataIndex+"0"]:selectedKeys[0].format("YYYY-MM-DD HH:mm:ss"),[dataIndex+"1"]:selectedKeys[1].format("YYYY-MM-DD HH:mm:ss")})}
      ></DatePicker.RangePicker>
      <div style={{marginTop:"5px"}}>
        <Button type="primary"
          onClick={()=>{
            if(selectedKeys===undefined || selectedKeys[0]===undefined){
              _this.SearchData({[dataIndex+"0"]:"",[dataIndex+"1"]:""})
            }else{
              _this.SearchData({[dataIndex+"0"]:selectedKeys[0].format("YYYY-MM-DD HH:mm:ss"),[dataIndex+"1"]:selectedKeys[1].format("YYYY-MM-DD HH:mm:ss")})
            }
           
          }
          }
          size="small"
          style={{width:90}}
        >查询</Button>
        <Button
          onClick={() => {
            clearFilters();
            _this.SearchData({[dataIndex+"0"]:"",[dataIndex+"1"]:""})
          }}
          size="small"
          style={{ width: 90 }}
        >
          重置
        </Button>
      </div>
    </div>
  )
});