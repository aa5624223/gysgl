import ajax from './ajax'
import localStore from '../utils/storageUtils'
const BASE = window.location.origin+"/";
export const request = (formData,url,type='GET')=>{
    const user = localStore.getUser();
    if(user!=={}){
        formData.append("OptUserCode",user.UserCode);
    }
    return ajax(BASE+url,formData,type)
}