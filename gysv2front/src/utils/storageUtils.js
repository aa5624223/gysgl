//import { getSuggestedQuery } from "@testing-library/react";
import store from 'store'
/*
    将数据存在本地
*/
const USER_KEY = 'user_key_gysplatform'
const localStore = {
    //1.保存数据
    saveUser (user){
        //localStorage.setItem(USER_KEY,JSON.stringify(user));
        store.set(USER_KEY,user);
    },
    //2.读取数据
    getUser(){
        //return JSON.parse(localStorage.getItem(USER_KEY)||'{}');
        return store.get(USER_KEY)||{}
    },
    //3.删除数据
    removeUser(){
        store.remove(USER_KEY);
    }
}
export default localStore;