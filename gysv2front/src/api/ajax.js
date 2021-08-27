import axios from 'axios'
import { message } from 'antd'
export default function ajax(url, data = {}, type = 'GET') {
    return new Promise((resolve, reject) => {
        let promise;
        if (type === 'GET') {//get请求
            promise = axios.get(url, {//参数
                params: data
            })
        } else if(type === 'POST'){//post请求
            promise = axios.post(url, data)
        }else{//form
            promise = axios.post(url, data,{  
                headers: { 'content-type': 'multipart/form-data'}
            }
                )
        }
        promise.then(response => {
            resolve(response.data)
        }).catch(error => {
            message.error('网络请求出错' + error.message)
        })
    })
}