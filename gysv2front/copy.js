import React, { Component } from 'react'
import {Table,message,Modal,Button} from 'antd'
//引入API
import { request} from '../../api'
//引入工具
import {ConvertFomrData} from '../../utils'
import {G_News_columns} from '../../config/table-columns'
import {PublicNews} from '../../anaData'
//公共通知
export default class Message1 extends Component {
    state = {
        dataSource: [],
        dataTotal: 0,
        loading: false,
        page:{},//当前页数
        condition:{},//当前查询条件的封装
        columns:[]//表格对应的列
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
            this.setState({dataSource:Lw_V_LogInfo,loading:false,dataTotal:Lw_V_LogInfo_Count,columns:G_News_columns(this),condition:Cur_condition})
        }else{
            message.error("服务器无响应");
            this.setState({loading:false})
        }
    }
    componentDidMount = ()=>{
        
    }
    render() {
        const {dataSource,dataTotal,loading,page,condition,columns} = this.state;
        return (
            <div className="main">
                {/* 条件 */}
                <div style={{float:"left",marginBottom:'5px',width:"70%",fontSize:'22px',fontWeight:600}}>
                查询条件:
                </div>
                <div style={{float:'left',width:'29%',textAlign:"right"}}>
                    <Button type="primary" onClick={()=>this.SearchData()}>发布通知</Button>
                    &nbsp;
                    <Button type="default" onClick={()=>this.SearchData()}>重置条件</Button>
                </div>
                <Table
                    dataSource={dataSource} 
                    bordered 
                    rowKey = "ID"
                    sticky={true}
                    columns={columns}
                    size="small"
                    loading = {loading}
                    pagination={{
                        position: ['bottomCenter'],
                        pageSizeOptions:[20],
                        current:page.page,
                        total:dataTotal,
                        showTotal:(total, range) => `一共 ${total} 条数据`,
                        pageSize:20,
                        onChange:(page,pageSize)=>{
                            this.SearchData({},{page:page,pageSize:pageSize});
                        }
                    }}
                >

                </Table>
            </div>
        )
    }
}
