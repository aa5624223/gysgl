import {Button,Tooltip} from 'antd'
import { SearchOutlined,DownloadOutlined} from '@ant-design/icons';
import {getColumnSearchProps,getColumnSearchPropsDate} from '../utils'
//UserInfo 用户管理的表格配置
export const UserConfig_columns = app=>{
    let column = [
        {
            title: '编号',
            dataIndex: 'UserCode',
            key: 'UserCode',
            width: 50,
        },
        {
            title: '用户名',
            dataIndex: 'UserName',
            key: 'UserName',
            ellipsis: true,
            width: 40,
        },
        {
            title: '操作',
            width: 90,
            fixed: 'right',
            dataIndex: 'operation',
            render: (_, record) => {
                return (
                    <div style={{ textAlign: 'center' }}>
                        {app.state.CopyRowKeys.length===0?<Button type="dashed" size="small" onClick={(event)=>app.ModalCopy(record,event)} >复制权限</Button>:<Button type="default" onClick={(event)=>app.ModalPaste(record,event)} size="small">粘贴权限</Button>}&nbsp;
                        <Button size="small" type="primary" onClick={(event) => app.ModalDel(record,event)} danger>删除</Button>&nbsp;
                        <Button size="small" type="primary" onClick={(event) => app.ModalEdit(record,event)} >编辑</Button>
                    </div>
                )
            }
        }
    ];
    return column;
} 
//操作记录
export const V_LogInfo_columns = (app)=>{
    return [
        {
            title: '操作类型',
            dataIndex: 'TypeName',
            key: 'TypeName',
            width: 30,
            ...getColumnSearchProps('TypeName',app)
        },
        {
            title: '操作人编码',
            dataIndex: 'UserCode',
            key: 'UserCode',
            width: 30,
            ...getColumnSearchProps('UserCode',app)
        },
        {
            title: '操作人',
            dataIndex: 'UserName',
            key: 'UserName',
            width: 30,
            ...getColumnSearchProps('UserName',app)
        },
        {
            title: '操作时间',
            dataIndex: 'DateTime1',
            key: 'DateTime1',
            width: 30,
            ...getColumnSearchPropsDate('DateTime1',app)
        },
        {
            title: '操作内容',
            dataIndex: 'Contents',
            key: 'Contents',
            ellipsis: true,
            width: 30,
        },
        {
            title: '详细',
            width: 20,
            fixed: 'right',
            dataIndex: 'operation',
            render: (_, record) => {
                return (
                    <div style={{ textAlign: 'center' }}>
                        <Tooltip placement="top" title="详细" >
                            <Button size="small" icon={<SearchOutlined />} onClick={() => app.ModalOpen(record)} ></Button>
                        </Tooltip>
                    </div>
                )
            }
        }
    ]
} 
//消息设置
export const G_News_columns = (app)=>{
    return [
        {
            title:'序号',
            width: 8,
            render:(text,record,index)=>{
                return index+1;
            }
        },
        {
            title: '信息标题',
            dataIndex: 'Title',
            key: 'Title',
            width: 40,
            ellipsis: true,
            ...getColumnSearchProps('Title',app),
        },
        {
            title: '日期',
            dataIndex: 'EditDate',
            key: 'EditDate',
            width: 22,
            ...getColumnSearchPropsDate('EditDate',app)
        },
        {
            title: '发送人',
            dataIndex: 'UserName',
            key: 'UserName',
            width: 15,
            ...getColumnSearchProps('UserName',app)
        },
        {
            title: '内容',
            dataIndex: 'Contents',
            key: 'Contents',
            ellipsis: true,
            width: 50,
        },
        {
            title: '操作',
            width: 40,
            fixed: 'right',
            dataIndex: 'operation',
            render: (_, record) => {
                return (
                    <div style={{ textAlign: 'center' }}>
                        <Button size="small" type="primary" onClick={(event) => app.ModalDel(record,event)} danger>删除</Button>&nbsp;
                        <Button size="small" type="primary" onClick={(event) => app.ModalEdit(record,event)}>编辑</Button>
                        &nbsp;
                        <Button size="small" icon={<DownloadOutlined />} onClick={() => app.ModalOpen(record)}>附件</Button>
                    </div>
                )
            }
        }
    ]
}