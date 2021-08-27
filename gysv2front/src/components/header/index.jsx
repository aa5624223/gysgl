import React, { Component } from 'react'
import {Modal} from 'antd'
import {withRouter} from 'react-router-dom'
import LinkButton from '../link-button'
import './index.less'
class Header extends Component {
    state = {
        cTime:'',
        UserName:'',
    }
    componentDidMount=()=>{
        // setInterval(() => {
        //     let time = getTime();
        //     this.setState({
        //         cTime:time
        //     })
        // }, 1000); 
        // getTime();
        const {UserName} = this.props;
        this.setState({UserName});
    }
    render() {
        const {UserName} = this.state;
        const pathname = this.props.history.location.pathname;
        ///Admin/Upload/ExcelUp
        return (
            <div className="Header">
                <div className="Header-top">
                    <span>欢迎，{UserName}</span>
                    <LinkButton>退出</LinkButton>
                </div>
                <div className="Header-btm">
                    <div className="Header-btm-left">
                        <h1 style={{fontSize:'100%',margin:'0px'}}></h1>
                    </div>
                    <div className="Header-btn-right">
                        {/* <span>时间:{cTime}</span> */}
                    </div>
                </div>
            </div>
        )
    }
}

// function getTime(){
//         var now = Date.now();
//         const Year = now.getFullYear;
//         const Month = now.getMonth+1;
//         const Date = now.getDate();
//         const Hour = now.getHours();
//         const Minu = now.getMinutes();
//         const Second = now.getSeconds();
//         return `${Year}-${Month>9?Month:'0'+Month}-${Date} ${Hour}:${Minu}:${Second}`;  
// }
export default withRouter(Header)