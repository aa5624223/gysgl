
import {SocketIP} from '../utils/StaticSet'
export function ServerMsg(UserID,UserType) {
    let Socket = new WebSocket(SocketIP+`UserID=${UserID}&UserType=${UserType}`);
    return Socket
} 