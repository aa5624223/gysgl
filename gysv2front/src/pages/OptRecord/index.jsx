import React, { Component } from 'react'
import { Table,message,Modal, Button} from 'antd'

//引入API
import { request} from '../../api'
//引入工具
import {ConvertFomrData} from '../../utils'
//配置 
import { V_LogInfo_columns } from '../../config/table-columns'
import './index.less'
//操作日志
export default class OptRecord extends Component {
    SearchDataTemp = {};
    state = {
        dataSource: [],
        dataTotal: 0,
        loading: false,
        isModalShow:false,
        condition:{},//当前查询条件的封装
        page:{},//当前页数
        ModalData:{}, 
        V_LogInfo_columns:[] 
    }
    SearchData = async (condition={},page={})=>{
        let Cur_condition = this.state.condition;
        Cur_condition = {...Cur_condition,...condition};
        if(page==={}){//只有查询请求，没有翻页
            page = {page:1,pageSize:20}
        }
        let formData =  ConvertFomrData({data:JSON.stringify(Cur_condition),...page});
        this.setState({loading:true});
        const result = await request(formData,'Home/getLogInfo','POST');
        if(result.status===0){
            const {Lw_V_LogInfo,Lw_V_LogInfo_Count} = result.data;
            this.setState({dataSource:Lw_V_LogInfo,loading:false,dataTotal:Lw_V_LogInfo_Count,V_LogInfo_columns:V_LogInfo_columns(this),condition:Cur_condition})
        }else{
            message.error("服务器无响应");
            this.setState({loading:false})
        }
    }
    ModalOpen = (record)=>{
        this.setState({isModalShow:true,ModalData:record})
    }
    ModalOK = ()=>{
        this.setState({isModalShow:false})
    }
    ModalCancel = ()=>{
        this.setState({isModalShow:false})
    }
    componentDidMount = ()=>{
        this.SearchData();
    }
    componentWillUnmount = ()=>{
    }
    render() {
        const {dataSource,dataTotal,loading,isModalShow,ModalData,current,V_LogInfo_columns,condition} = this.state;
        return (
            <div className="main">
                <div style={{float:"left",marginBottom:'5px',width:"78%",fontSize:'22px',fontWeight:600}}>
                    查询条件:
                    {condition.TypeName?"操作类型:"+condition.TypeName+",":""}
                    {condition.UserCode?"操作人编码:"+condition.UserCode+",":""}
                    {condition.UserName?"操作人:"+condition.UserName+",":""}
                    {condition.DateTime10?"操作时间:"+condition.DateTime10+"至"+condition.DateTime11+",":""}
                </div>
                <div style={{float:'left',width:'21%',textAlign:"right"}}>
                    <Button type="default" onClick={()=>this.SearchData()}>重置条件</Button>
                </div>
                <Table
                    dataSource={dataSource} 
                    bordered 
                    rowKey = "ID"
                    sticky={true}
                    columns={V_LogInfo_columns}
                    //scroll={{y: 600 }} 
                    size="small"
                    loading = {loading}
                    pagination={{
                        position: ['bottomCenter'],
                        pageSizeOptions:[20],
                        current:current,
                        total:dataTotal,
                        showTotal:(total, range) => `一共 ${total} 条数据`,
                        pageSize:20,
                        onChange:(page,pageSize)=>{
                            this.SearchData({},{page:page,pageSize:pageSize});
                        }
                    }}
                >
                </Table>
                <Modal title="详细" visible={isModalShow} okText="确认" cancelText="关闭" onOk={()=>this.ModalOK()} onCancel={()=>this.ModalCancel()}>
                    <h2>操作类型</h2><p>&emsp;&emsp;{ModalData.TypeName}</p>
                    <h2>操作人</h2><p>&emsp;&emsp;{ModalData.UserName}</p>
                    <h2>操作人编码</h2><p>&emsp;&emsp;{ModalData.UserCode}</p>
                    <h2>操作时间</h2><p>&emsp;&emsp;{ModalData.DateTime1}</p>
                    <h2>操作内容</h2><p>&emsp;&emsp;{ModalData.Contents}</p>
                </Modal>
            </div>
        )
    }
}
