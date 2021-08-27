import React, { Component } from 'react'
import {notification,Button} from 'antd'
export default class notify extends Component {
    //已读
    close = () => {
        console.log(
          'Notification was closed. Either the close button was clicked or duration time elapsed.',
        );
    };
    
    render() {
        return (
            <div>
                
            </div>
        )
    }
}
