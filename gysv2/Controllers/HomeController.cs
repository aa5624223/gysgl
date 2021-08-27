using Fleck;
using gysv2.Models;
using gysv2.Socket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using project_Lw_Utils;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace gysv2.Controllers
{
    public class HomeController : Controller
    {
        log4net.ILog Log = log4net.LogManager.GetLogger(typeof(HomeController));
        public static List<SocketModel> ClientSockets;
        #region Socket 相关

        /// <summary>
        /// 初始化Socket
        /// </summary>
        public void SocketInit()
        {
            if (ClientSockets == null)
            {
                string ServerPath = HttpContext.Server.MapPath("/WebCfg/Connect.json");
                string ConnPath = HttpContext.Server.MapPath("/WebCfg/Db.json");
                string ServerIP = null;
                //+1为了端口不被占用
                int Port = int.Parse(HttpContext.Request.ServerVariables["SERVER_PORT"].Replace("/",""))+1;
                if (HttpContext.Request.ServerVariables["SERVER_NAME"]=="localhost")
                {
                    ServerIP = "ws://127.0.0.1:" + Port + project_Lw_Utils.Lw_Utils.getSocketStr(ServerPath, "ServerSocket");
                }
                else
                {
                    //这里可能出现最后一个是/的错误
                    ServerIP = "ws://" + HttpContext.Request.ServerVariables["SERVER_NAME"] + ":" + Port +"/" + project_Lw_Utils.Lw_Utils.getSocketStr(ServerPath, "ServerSocket");
                }
               
                WebSocketServer ClientsSocket = new WebSocketServer(ServerIP);
                ClientSockets = new List<SocketModel>();
                ClientsSocket.Start(Socket =>
                {
                    #region Socket连接成功

                    Socket.OnOpen = () => {
                        SocketModel bean = new SocketModel();
                        Hashtable Params = Lw_Utils.GetParams(Socket.ConnectionInfo.Path);
                        if (Params["UserID"]==null)
                        {
                            //没有传递必要参数直接关闭Socket
                            Socket.Close(400);
                        }
                        else
                        {
                            bean.UserID = Params["UserID"].ToString();
                            string UserType = Params["UserType"].ToString();
                            //用于保证不重复连接
                            bean.SocketID = Socket.ConnectionInfo.Id.ToString();
                            bean.Socket = Socket;
                            bean.UserType = UserType;
                            SocketModel ft = ClientSockets.Find(it=>it.SocketID == bean.SocketID);
                            if (ft==null)
                            {
                                ClientSockets.Add(bean);
                                //查询所有消息发送回去
                                V_Lw_Msg MsgBean = new V_Lw_Msg();
                                string sql = Common.find(MsgBean);
                                if (UserType== "gys")
                                {
                                    sql += $" WHERE  GysID= {bean.UserID}";
                                }
                                else
                                {
                                    sql += $" WHERE  UserID = ${bean.UserID}";
                                }
                               
                                JObject MsgList =  Common.findCommond(sql,typeof(V_Lw_Msg),1,500, ConnPath);
                                MsgList.Add("MsgType", "Init");
                                Socket.Send(MsgList.ToString());
                            }
                            else//重复连接 不允许
                            {
                                Socket.Close(403);
                            }
                            
                        }
                    };

                    #endregion

                    #region Socket关闭连接

                    Socket.OnClose = () => {
                        SocketModel ft = ClientSockets.Find(it => it.Socket == Socket);
                        if (ft!=null)
                        {
                            ClientSockets.Remove(ft);
                        }
                    };

                    #endregion

                    #region Socket接收到客户端的信息

                    Socket.OnMessage = Message =>
                    {
                        JObject msg = JsonConvert.DeserializeObject<JObject>(Message);
                        string Target = msg["target"].ToString();
                        string Msg = msg["Msg"].ToString();
                        string UserName = msg["UserName"].ToString();
                        List<SocketModel> TargetFt = ClientSockets.FindAll(it=>it.UserID==Target);
                        SocketModel SendFt = ClientSockets.Find(it=>it.Socket==Socket);
                        //1.接收到数据 存入数据库
                        Lw_Msg MsgBean = new Lw_Msg();
                        MsgBean.MsgType = SendFt.UserType;
                        MsgBean.SendeTime = DateTime.Now;
                        
                        MsgBean.Files = "";
                        if (SendFt.UserType == "gys")
                        {
                            MsgBean.GysID = int.Parse(SendFt.UserID);
                            MsgBean.UserID = int.Parse(Target);
                        }
                        else
                        {
                            MsgBean.GysID = int.Parse(Target);
                            MsgBean.UserID = int.Parse(SendFt.UserID);
                        }
                        MsgBean.Msg = Msg;
                        
                        //2.若对方在线 发送给对方
                        if (TargetFt != null)//对方在线 认为对方已经读取了信息
                        {
                            MsgBean.ReceiveTime = DateTime.Now.AddSeconds(1);
                            //将数据封装 传送给对方
                            JObject toMsg = new JObject();
                            toMsg.Add("ID",DateTime.Now);
                            toMsg.Add("Files","");
                            toMsg.Add("Msg",Msg);
                            toMsg.Add("UserName", UserName);
                            toMsg.Add("SendeTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            toMsg.Add("Receive","Receive");
                            toMsg.Add("SendID", SendFt.UserID);//谁发送的
                            toMsg.Add("MsgType","Msg");
                            foreach (SocketModel sk in TargetFt)
                            {
                                sk.Socket.Send(toMsg.ToString());
                            }
                            string sql = Common.add<Lw_Msg>(MsgBean);
                            Common.OptCommond(sql, ConnPath);
                        }
                        else
                        {

                        }
                    };

                    #endregion

                });
            }
        }


        #endregion
        public ActionResult Index()
        {

            //CreateModelsConfig();
            //CreateModels();
            SocketInit();
            return View();
        }

        #region 控制

        // <summary>
        // 登录
        // </summary>
        // <param name = "fc" ></ param >
        // < returns ></ returns >
        [HttpPost]
        public string Login(FormCollection fc)
        {
            JObject msg = new JObject();
            #region 获取客户端数据
            Lw_UserInfo bean = new Lw_UserInfo();
            Lw_GysInfo GysBean = new Lw_GysInfo();
            bean.UserCode = fc["username"];
            bean.PWD = fc["password"];
            GysBean.UserCode = fc["username"];
            GysBean.PWD = fc["password"];
            if (bean.UserCode == null || bean.PWD == null)
            {
                msg.Add("status", 1);
                msg.Add("data", null);
                return msg.ToString();
            }
            #endregion

            #region 查询数据

            string ServerPath = Server.MapPath("/WebCfg/Db.json");
            List<string> sqls = new List<string>();
            string sql1 = Common.find<Lw_UserInfo>(bean);
            string sql2 = Common.find<Lw_GysInfo>(GysBean);
            sqls.Add(sql1);
            sqls.Add(sql2);
            List<Type> Types = new List<Type>();
            Types.Add(typeof(Lw_UserInfo));
            Types.Add(typeof(Lw_GysInfo));
            //只查一条消息
            JObject result = Common.findCommond(sqls, Types, 1, 1, ServerPath);

            #endregion

            #region 返回数据

            msg.Add("status", 0);
            msg.Add("data", result);
            return msg.ToString();

            #endregion

        }

        /// <summary>
        /// 获取所有用户数据
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public string getUserList(FormCollection fc)
        {
            //查找用户数据
            JObject msg = new JObject();
            string UserName = fc["UserName"];


            #region 获取数据
            Lw_UserInfo bean = new Lw_UserInfo();
            //bean.UserCode = fc["username"];
            //bean.PWD = fc["password"];
            #endregion

            #region 查询数据
            string ServerPath = Server.MapPath("/WebCfg/Db.json");
            string sql = Common.find<Lw_UserInfo>(bean);
            if (!string.IsNullOrEmpty(UserName))
            {
                if (sql.IndexOf("WHERE")>0)
                {
                    sql += $" AND UserName LIKE '%{UserName}%' ";
                }
                else
                {
                    sql += $" WHERE UserName LIKE '%{UserName}%' ";
                }
                
            }
            //只查一条消息
            JObject result = Common.findCommond(sql, typeof(Lw_UserInfo), 1, 9999, ServerPath);

            #endregion

            #region 处理数据

            #endregion

            #region 返回数据
            msg.Add("status", 0);
            msg.Add("data", result);
            return msg.ToString();
            #endregion

        }

        /// <summary>
        /// 获取权限数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string getSite_Roles(FormCollection fc)
        {
            JObject msg = new JObject();
            string AuthString = fc["AuthConfig"];
            string type = fc["type"];
            if (type=="gys")//如果是供应商 固定设置权限
            {
                string AuthPath = Server.MapPath("/WebCfg/SiteSet.json");
                AuthString = Lw_Utils.GetPathJsonToString(AuthPath, "GysRoles");
            }
            if (string.IsNullOrEmpty(AuthString))
            {
                msg.Add("status", 1);
                msg.Add("data", new JObject());
            }
            else
            {
                Lw_Site_Roles bean = new Lw_Site_Roles();
                string ServerPath = Server.MapPath("/WebCfg/Db.json");
                string sql = Common.find<Lw_Site_Roles>(bean);
                if (!string.IsNullOrEmpty(AuthString) && AuthString!="ALL")
                {
                    sql += " WHERE id IN (" + AuthString + ") or parentId=0 or isAuth=0";
                }
                JObject result = Common.findCommond(sql, typeof(Lw_Site_Roles), 1, 99999, ServerPath);
                string length = result["Lw_Site_Roles"].ToString();
                if (length == "0")
                {
                    msg.Add("status", 1);
                    msg.Add("data", new JObject());
                }
                else
                {
                    msg.Add("status", 0);
                    msg.Add("data", result);
                }
            }
            return msg.ToString();
        }

        /// <summary>
        /// 获取功能的权限
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public string getSite_Roles_Opt(FormCollection fc)
        {
            JObject msg = new JObject();
            string Role = fc["Roles"];
            string mapKey = fc["mapKey"];

            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(mapKey))
            {
                msg.Add("status", 1);
                msg.Add("data", new JObject());
            }
            else
            {
                Lw_Site_Roles bean = new Lw_Site_Roles();
                bean.UrlKey = mapKey;
                string ServerPath = Server.MapPath("/WebCfg/Db.json");
                string sql = Common.find<Lw_Site_Roles>(bean);
                JObject result = Common.findCommond(sql, typeof(Lw_Site_Roles), 1, 99999, ServerPath);
                string length = result["Site_Roles"].ToString();
                if (length == "0")
                {
                    msg.Add("status", 1);
                    msg.Add("data", new JObject());
                }
                else
                {
                    //Site_Roles
                    JArray Site_Roles = result.Value<JArray>("Site_Roles");
                    string parentId = Site_Roles[0].Value<string>("ID");
                    bean = new Lw_Site_Roles();
                    sql = Common.find<Lw_Site_Roles>(bean) + " WHERE parentId=" + parentId;
                    result = Common.findCommond(sql, typeof(Lw_Site_Roles), 1, 99999, ServerPath);
                    string[] Roles = Role.Split(',');
                    JArray ret = new JArray();
                    foreach (string item in Roles)
                    {
                        JArray ja = result.Value<JArray>("Site_Roles");
                        foreach (JToken jt in ja)
                        {
                            JObject jo = (JObject)jt;
                            if (jo.Value<string>("ID") == item)
                            {
                                ret.Add(jo);
                            }
                            if (jo.Value<string>("isAuth") == "False")
                            {
                                ret.Add(jo);
                            }
                        }
                    }
                    JObject reo = new JObject();
                    reo.Add("Site_Roles", ret);
                    msg.Add("status", 0);
                    msg.Add("data", reo);
                }
            }
            return msg.ToString();
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获得操作日志
        /// 除了账号是sys的用户
        /// 只能查看自己的操作记录
        /// </summary>
        /// <param name="fc">page,data 封装的json对象 用于查询</param>
        /// <returns></returns>
        public string getLogInfo(FormCollection fc)
        {

            JObject msg = new JObject();

            #region 获取数据
            string OptUserCode = fc["OptUserCode"];
            Lw_V_LogInfo bean = new Lw_V_LogInfo();
            int page = 1;
            int pageSize = 20;
            bean = JsonConvert.DeserializeObject<Lw_V_LogInfo>(fc["data"].ToString());
            if (!string.IsNullOrEmpty(fc["page"]))
            {
                page = int.Parse(fc["page"]);
            }
            if (!string.IsNullOrEmpty(fc["pageSize"]))
            {
                pageSize = int.Parse(fc["pageSize"]);
            }
            //TypeName DateTime1[0] DateTime1[1] UserName: page:pageSize:
            if (OptUserCode == "sys" )
            {
                bean.UserCode = null;
            }
            else
            {
                bean.UserCode = OptUserCode;
            }
            #endregion

            #region 获取sql

            string ServerPath = Server.MapPath("/WebCfg/Db.json");
            string sql = Common.find<Lw_V_LogInfo>(bean);
            //添加额外条件
            string ExtWhere = "";
           
            if (!string.IsNullOrEmpty(bean.Datetime10) && !string.IsNullOrEmpty(bean.Datetime11))
            {
                ExtWhere += $" Datetime1 >= '{bean.Datetime10}' AND Datetime1 <= '{bean.Datetime11}' ";
            }
            else if (!string.IsNullOrEmpty(bean.Datetime10))
            {
                ExtWhere += $" Datetime1 >= '{bean.Datetime10}' ";
            }
            else if (!string.IsNullOrEmpty(bean.Datetime11))
            {
                ExtWhere += $" Datetime1 <= '{bean.Datetime11}' ";
            }
            if (sql.IndexOf("WHERE") < 0 && ExtWhere.Length>1)
            {
                ExtWhere = " WHERE " + ExtWhere;
            }
            else if(ExtWhere.Length > 1)
            {
                ExtWhere = " AND " + ExtWhere;
            }
            //添加日期

            sql += ExtWhere+ " ORDER BY DateTime1 DESC";
            JObject result = null;
            //查询所有数据
            try
            {
                result = Common.findCommond(sql, typeof(Lw_V_LogInfo), page, pageSize, ServerPath);
            }
            catch (Exception _e)
            {
                Log.Error("", _e);
                throw;
            }
            #endregion

            #region 处理数据

            #endregion

            #region 返回数据
            msg.Add("status", 0);
            msg.Add("data", result);
            return msg.ToString();
            #endregion
        }

        /// <summary>
        /// 获得操作日志
        /// 除了账号是sys的用户w
        /// 只能查看自己的操作记录
        /// </summary>
        /// <param name="fc">page,data 封装的json对象 用于查询</param>
        /// <returns></returns>
        public string getLw_News(FormCollection fc)
        {

            JObject msg = new JObject();

            #region 获取数据
            string OptUserCode = fc["OptUserCode"];
            Lw_V_News bean = new Lw_V_News();
            int page = 1;
            int pageSize = 20;
            bean = JsonConvert.DeserializeObject<Lw_V_News>(fc["data"].ToString());
            if (!string.IsNullOrEmpty(fc["page"]))
            {
                page = int.Parse(fc["page"]);
            }
            if (!string.IsNullOrEmpty(fc["pageSize"]))
            {
                pageSize = int.Parse(fc["pageSize"]);
            }
            #endregion

            #region 获取sql

            string ServerPath = Server.MapPath("/WebCfg/Db.json");
            string sql = Common.find<Lw_V_News>(bean);
            //添加额外条件
            string ExtWhere = "";

            if (!string.IsNullOrEmpty(bean.EditDate0) && !string.IsNullOrEmpty(bean.EditDate1))
            {
                ExtWhere += $" EditDate >= '{bean.EditDate0}' AND EditDate <= '{bean.EditDate1}' ";
            }
            else if (!string.IsNullOrEmpty(bean.EditDate0))
            {
                ExtWhere += $" EditDate >= '{bean.EditDate0}' ";
            }
            else if (!string.IsNullOrEmpty(bean.EditDate1))
            {
                ExtWhere += $" EditDate <= '{bean.EditDate1}' ";
            }
            if (sql.IndexOf("WHERE") < 0 && ExtWhere.Length > 1)
            {
                ExtWhere = " WHERE " + ExtWhere;
            }
            else if(ExtWhere.Length > 1)
            {
                ExtWhere = " AND " + ExtWhere;
            }

            //添加日期

            sql += ExtWhere + " ORDER BY EditDate DESC";
            JObject result = null;
            //查询所有数据
            try
            {
                result = Common.findCommond(sql, typeof(Lw_V_News), page, pageSize, ServerPath);
            }
            catch (Exception _e)
            {
                Log.Error("", _e);
                throw;
            }
            #endregion

            #region 处理数据

            #endregion

            #region 返回数据
            msg.Add("status", 0);
            msg.Add("data", result);
            return msg.ToString();
            #endregion
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 修改用户数据
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public string editUser(FormCollection fc)
        {
            //查找用户数据
            JObject msg = new JObject();
            string OptUserCode = fc["OptUserCode"];
            #region 获取数据
            Lw_UserInfo bean = new Lw_UserInfo();
            bean.PWD = fc["PWD"];
            bean.UserCode = fc["UserCode"];
            bean.UserName = fc["UserName"];
            bean.Roles = fc["Roles"];
            var bean2 = new Lw_UserInfo();
            bean2.ID = int.Parse(fc["ID"]);
            bool flg = false;
            #endregion

            #region 获得sql
            try
            {
                string ServerPath = Server.MapPath("/WebCfg/Db.json");
                string sql = Common.updata<Lw_UserInfo>(bean, bean2);
                //只查一条消息
                List<string> sqls = new List<string>();
                string logSql = InsertLog("修改用户", "修改的用户ID:" + bean2.ID, OptUserCode);
                sqls.Add(sql);
                sqls.Add(logSql);
                flg = Common.OptCommond(sqls, ServerPath);
            }
            catch (Exception _e)
            {
                if (_e.Message.IndexOf("UNIQUE KEY ") > 0)
                {
                    msg.Add("status", 2);
                    msg.Add("data", "fail");
                    return msg.ToString();
                }
                throw;
            }
            

            #endregion

            #region 处理数据

            #endregion

            #region 返回数据
            if (flg)
            {
                msg.Add("status", 0);
                msg.Add("data", "OK");
            }
            else
            {
                msg.Add("status", 1);
                msg.Add("data", "fail");
            }
            return msg.ToString();

            #endregion
        }

        #endregion

        #region 删除数据

        #endregion

        #region 添加数据

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public string addUser(FormCollection fc)
        {
            //查找用户数据
            JObject msg = new JObject();
            #region 获取数据
            string OptUserCode = fc["OptUserCode"];
            Lw_UserInfo bean = new Lw_UserInfo();
            bean.PWD = fc["PWD"];
            bean.UserCode = fc["UserCode"];
            bean.UserName = fc["UserName"];
            bool flg = false;
            #endregion

            #region 添加数据
            try
            {
                List<string> sqls = new List<string>();
                string ServerPath = Server.MapPath("/WebCfg/Db.json");
                string sql = Common.add<Lw_UserInfo>(bean);
                string logSql = InsertLog("添加用户", $"成功添加用户,用户编号:{bean.UserCode},用户名:{bean.UserName}", OptUserCode);
                sqls.Add(sql);
                sqls.Add(logSql);
                //只查一条消息
                flg = Common.OptCommond(sqls, ServerPath);
            }
            catch (Exception _e)
            {
                //"违反了 UNIQUE KEY 约束“Lw_UserInfo_UserCode_Unique”。不能在对象“dbo.Lw_UserInfo”中插入重复键。重复键值为 (100096)。\r\n语句已终止。"
                if (_e.Message.IndexOf("UNIQUE KEY ")>0)
                {
                    msg.Add("status", 2);
                    msg.Add("data", "fail");
                    return msg.ToString();
                }
                throw;
            }
            

            #endregion

            #region 处理数据

            #endregion

            #region 返回数据
            if (flg)
            {
                msg.Add("status", 0);
                msg.Add("data", "OK");
            }
            else
            {
                msg.Add("status", 1);
                msg.Add("data", "fail");
            }
            return msg.ToString();

            #endregion
        }

        #endregion

        #region 生成配置

        /// <summary>
        /// 创建实体类配置文件
        /// </summary>
        private void CreateModelsConfig()
        {
            string ServerPath = Server.MapPath("/WebCfg");
            //数据库文件位置
            //DateTime dt = DateTime.Now;
            //配置文件位置
            //string ModelsPath = Server.MapPath($"/WebCfg/Models_{Lw_Utils.CreatenTimestamp()}.json");
            //获取数据库连接
            //string ServerPath = Server.MapPath("/WebCfg/Db.json");
            string ConnStr = Lw_Utils.getConnStr($"{ServerPath}\\Db.json", "CommonStr");
            //获取连接对象
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlDataAdapter ada;
            //JObject jo = new JObject();
            string LogSQL = "";
            //获取表
            //select name from sysobjects as Tb1 where xtype = 'u' or xtype = 'v'
            try
            {
                ada = new SqlDataAdapter();
                ada.SelectCommand = new SqlCommand("select name from sysobjects as Tb1 where xtype = 'u' or xtype = 'v'  ", conn);
                DataSet TableNames_ds = new DataSet();
                ada.Fill(TableNames_ds, "TableNames");
                DataTable TableNames_Tab = TableNames_ds.Tables["TableNames"];
                SqlDataAdapter Tables_ada = new SqlDataAdapter();
                DataSet Tables_ds = new DataSet();
                //获取所有表的字段
                foreach (DataRow dr in TableNames_Tab.Rows)
                {

                    if (!dr.IsNull("name"))
                    {
                        if (dr["name"].ToString()== "vendor-1401" || dr["name"].ToString()== "VIEW1" || dr["name"].ToString() == "VIEW2" || dr["name"].ToString() == "VIEW4")
                        {

                        }
                        else
                        {
                            string Table_Name = dr["name"].ToString();
                            string Table_Find_Sql = $"SELECT * FROM {Table_Name} WHERE 1!=1";
                            LogSQL = Table_Find_Sql;
                            Tables_ada.SelectCommand = new SqlCommand(Table_Find_Sql, conn);
                            Tables_ada.Fill(Tables_ds, Table_Name);
                        }
                    }
                }
                string Models_path = Lw_Utils.CreateFilePath($"{ServerPath}\\Models_{Lw_Utils.CreateTimestamp()}");
                foreach (DataTable Table in Tables_ds.Tables)
                {
                    JObject Model_jo = new JObject();
                    foreach (DataColumn column in Table.Columns)
                    {
                        JObject Property = new JObject();
                        Type Property_type = column.DataType;
                        string Property_Name = column.ColumnName;
                        bool isKey = column.Unique;
                        Property.Add("Name", Property_Name);
                        Property.Add("type", Property_type.Name);
                        JObject Tag = new JObject();
                        if (Property_Name == "ID" || Property_Name == "Id" || Property_Name == "id")
                        {
                            Property.Add("Key", true);
                            Tag.Add("PType", "PK");
                        }
                        if (Property_type.Name == "Int32" || Property_type.Name == "Decimal" || Property_type.Name == "Int64" || Property_type.Name == "Byte")
                        {
                            Tag.Add("Model", 3);
                        }
                        Property.Add("Tag", Tag);
                        Model_jo.Add(Property_Name, Property);
                    }
                    string FileName = $"{Models_path}\\{Table.TableName}.json";
                    Lw_Utils.CreateFileAndWrite(FileName, Model_jo.ToString());
                }
            }

            catch (Exception _e)
            {
                Log.Error("", _e);
            }
            finally
            {
                //if (ada != null)
                //{
                //    ada.Dispose();
                //}
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 同步更新配置文件
        /// Models_1712383832
        /// Key前加new_* 然后运行改方法
        /// </summary>
        private void UpdateModels()
        {
            string ServerPath = Server.MapPath("/WebCfg");
            //数据库文件位置
            //DateTime dt = DateTime.Now;
            //配置文件位置
            //string ModelsPath = Server.MapPath($"/WebCfg/Models_{Lw_Utils.CreatenTimestamp()}.json");
            //获取数据库连接
            //string ServerPath = Server.MapPath("/WebCfg/Db.json");
            string ConnStr = Lw_Utils.getConnStr($"{ServerPath}\\Db.json", "CommonStr");
            //获取连接对象
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlDataAdapter ada;
            JObject jo = new JObject();
            //获取表
            //select name from sysobjects as Tb1 where xtype = 'u' or xtype = 'v'
            try
            {
                ada = new SqlDataAdapter();
                ada.SelectCommand = new SqlCommand("select name from sysobjects as Tb1 where xtype = 'u' or xtype = 'v'", conn);
                DataSet TableNames_ds = new DataSet();
                ada.Fill(TableNames_ds, "TableNames");
                DataTable TableNames_Tab = TableNames_ds.Tables["TableNames"];
                SqlDataAdapter Tables_ada = new SqlDataAdapter();
                DataSet Tables_ds = new DataSet();
                //获取所有表的字段
                foreach (DataRow dr in TableNames_Tab.Rows)
                {
                    if (!dr.IsNull("name"))
                    {
                        string Table_Name = dr["name"].ToString();
                        string Table_Find_Sql = $"SELECT * FROM {Table_Name} WHERE 1!=1";
                        Tables_ada.SelectCommand = new SqlCommand(Table_Find_Sql, conn);
                        Tables_ada.Fill(Tables_ds, Table_Name);
                    }
                }
                //测试 Models_1712382891
                string Models_path = $"{ServerPath}\\Models_1712903313";

                foreach (DataTable Table in Tables_ds.Tables)
                {
                    JObject Model_jo = null;
                    if (System.IO.File.Exists($"{Models_path}\\{Table.TableName}.json"))
                    {
                        Model_jo = Lw_Utils.GetFileJson<JObject>($"{Models_path}\\{Table.TableName}.json");
                    }
                    else
                    {
                        Model_jo = new JObject();
                    }
                    //更新 和 添加
                    foreach (DataColumn column in Table.Columns)
                    {
                        JObject Property = new JObject();
                        Type Property_type = column.DataType;
                        string Property_Name = column.ColumnName;
                        bool isKey = column.Unique;
                        if (Model_jo.ContainsKey(Property_Name))
                        {//存在属性 判断是否有 new_*
                            string DelProp = "";//是否删除该属性
                            JObject PropertyObj = (JObject)Model_jo.GetValue(Property_Name);
                            foreach (JProperty jProperty in PropertyObj.Properties())
                            {
                                if (jProperty.Name.Contains("new_"))
                                {
                                    //删除掉原来的属性 添加new属性
                                    DelProp = jProperty.Name;
                                    string newProperty_Name = DelProp.Replace("new_", "");
                                    PropertyObj[newProperty_Name] = jProperty.Value.ToString();

                                }
                            }
                            //删除掉所有带new的属性
                            if (DelProp != "")
                            {
                                PropertyObj.Remove(DelProp);
                            }
                        }
                        else//不存在这个属性 直接添加
                        {
                            Property.Add("Name", Property_Name);
                            Property.Add("type", Property_type.Name);
                            if (Property_Name == "ID" || Property_Name == "Id" || Property_Name == "id")
                            {
                                Property.Add("Key", true);
                            }
                            Model_jo.Add(Property_Name, Property);
                        }

                    }
                    //进行删除操作 删除掉数据库没有 配置文件有的属性
                    //Model_jo
                    List<string> DelProp_List = new List<string>();
                    foreach (JProperty jProperty in Model_jo.Properties())
                    {
                        string jPropertyName = jProperty.Name;
                        if (!Table.Columns.Contains(jPropertyName))
                        {
                            DelProp_List.Add(jProperty.Name);
                        }
                    }
                    foreach (string PropNmae in DelProp_List)
                    {
                        Model_jo.Remove(PropNmae);
                    }
                    string FileName = $"{Models_path}\\{Table.TableName}.json";
                    Lw_Utils.WriteFile(FileName, Model_jo.ToString());
                }
            }

            catch (Exception _e)
            {
                Log.Error("", _e);
                throw _e;
            }
            finally
            {
                //if (ada != null)
                //{
                //    ada.Dispose();
                //}
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 清除掉数据库中删除掉的数据文件
        /// </summary>
        private void DelOldModels()
        {
            string ServerPath = Server.MapPath("/WebCfg");
            //数据库文件位置
            //DateTime dt = DateTime.Now;
            //配置文件位置
            //string ModelsPath = Server.MapPath($"/WebCfg/Models_{Lw_Utils.CreatenTimestamp()}.json");
            //获取数据库连接
            //string ServerPath = Server.MapPath("/WebCfg/Db.json");
            string ConnStr = Lw_Utils.getConnStr($"{ServerPath}\\Db.json", "CommonStr");
            //获取连接对象
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlDataAdapter ada = null;
            JObject jo = new JObject();
            //获取表
            //select name from sysobjects as Tb1 where xtype = 'u' or xtype = 'v'
            try
            {
                ada = new SqlDataAdapter();
                ada.SelectCommand = new SqlCommand("select name from sysobjects as Tb1 where xtype = 'u' or xtype = 'v'", conn);
                DataSet TableNames_ds = new DataSet();
                ada.Fill(TableNames_ds, "TableNames");
                DataTable TableNames_Tab = TableNames_ds.Tables["TableNames"];
                SqlDataAdapter Tables_ada = new SqlDataAdapter();
                DataSet Tables_ds = new DataSet();
                List<string> Table_List = new List<string>();
                //获取所有表的字段
                foreach (DataRow dr in TableNames_Tab.Rows)
                {

                    if (!dr.IsNull("name"))
                    {
                        string Table_Name = dr["name"].ToString();
                        Table_List.Add(Table_Name.ToLower());
                    }
                }
                //测试 Models_1712382891
                string Models_path = $"{ServerPath}\\Models_1712903313";
                //DirectoryInfo root = new DirectoryInfo(Models_path);
                string[] Files = Lw_Utils.getDirFilesJson(Models_path);
                foreach (string str in Files)
                {

                    int newStrBegin = str.LastIndexOf("\\") + 1;
                    string newStr = str.Substring(newStrBegin).Replace(".json", "").ToLower();
                    string FindStr = Table_List.Find(item =>
                    {
                        return item == newStr;
                    });
                    if (string.IsNullOrEmpty(FindStr))
                    {
                        //数据库中没有删除掉这个文件
                        System.IO.File.Delete(str);
                    }
                    //Table_List
                }
            }

            catch (Exception _e)
            {
                Log.Error("", _e);
                throw _e;
            }
            finally
            {
                //if (ada != null)
                //{
                //    ada.Dispose();
                //}
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 根据配置文件 创建新的实体类
        /// </summary>
        private void CreateModels()
        {
            string ServerPath = Server.MapPath("/WebCfg");
            string Models_path = $"{ServerPath}\\Models_1712903313";
            string[] files = Lw_Utils.getDirFilesJson(Models_path);

            foreach (string fileName in files)
            {
                CodeCompileUnit unit = new CodeCompileUnit();
                CodeNamespace sampleNamespace = new CodeNamespace("gysv2.Models");
                JObject result = Lw_Utils.GetFileJson<JObject>(fileName);
                int begin = fileName.LastIndexOf("\\");
                int end = fileName.LastIndexOf(".json");
                string EntityName = fileName.Substring(begin + 1, end-begin-1);
                //生成类
                CodeTypeDeclaration Customerclass = new CodeTypeDeclaration(EntityName);
                Customerclass.IsClass = true;
                Customerclass.TypeAttributes = TypeAttributes.Public;
                sampleNamespace.Types.Add(Customerclass);
                unit.Namespaces.Add(sampleNamespace);
                //生成属性
                foreach (var Obj in result)
                {
                    JToken ObjValue = Obj.Value;
                    if (Obj.Key == "State")//类的描述
                    {
                        Customerclass.Comments.Add(new CodeCommentStatement("Author:林炜 \n " + ObjValue.ToString()));
                    }
                    else
                    {
                        CodeMemberProperty property = new CodeMemberProperty();
                        //类型
                        property.Type = new CodeTypeReference(Type.GetType(Lw_Utils.SwitchDbTypeToDotNetType(ObjValue["type"].ToString())));
                        property.Name = Obj.Key;
                        //get set
                        property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                        property.HasGet = true;
                        property.HasSet = true;
                        property.SetStatements.Add(new CodeSnippetStatement("[Delete]"));
                        property.GetStatements.Add(new CodeSnippetStatement("[Delete]"));
                        //注释
                        if (ObjValue["Det"] != null)
                        {
                            property.Comments.Add(new CodeCommentStatement(ObjValue["Det"].ToString()));
                        }
                        //Tag添加
                        if (ObjValue["Tag"] != null)
                        {
                            JObject Tags = (JObject)ObjValue["Tag"];
                            List<CodeAttributeArgument> Attrs = new List<CodeAttributeArgument>();
                            foreach (var TagObj in Tags)
                            {
                                //String Integer Float
                                //TagObj.Value.Type
                                string Type = TagObj.Value.Type.ToString();
                                CodePrimitiveExpression cv = null;
                                if (Type == "Integer")

                                {
                                    cv = new CodePrimitiveExpression(int.Parse(TagObj.Value.ToString()));
                                }
                                else if (Type == "Float")
                                {
                                    cv = new CodePrimitiveExpression(decimal.Parse(TagObj.Value.ToString()));
                                }
                                else
                                {
                                    cv = new CodePrimitiveExpression(TagObj.Value.ToString());
                                }
                                CodeAttributeArgument codeAttr = new CodeAttributeArgument(TagObj.Key, cv);
                                Attrs.Add(codeAttr);
                            }
                            if (Attrs.Count == 0)//只写了Tag标签
                            {
                                property.CustomAttributes.Add(new CodeAttributeDeclaration("Property"));
                            }
                            else
                            {
                                property.CustomAttributes.Add(new CodeAttributeDeclaration("Property", Attrs.ToArray()));
                            }
                        }
                        //添加到类里
                        Customerclass.Members.Add(property);
                    }

                }
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.BracingStyle = "C";
                options.BlankLinesBetweenMembers = true;
                string outputFile = Server.MapPath("./TempClass") + $"/{EntityName}.cs";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
                {
                    provider.GenerateCodeFromCompileUnit(unit, sw, options);
                }
                Lw_Utils.ReplaceTypeFileString(Server.MapPath("./TempClass"), ".cs", "{\r\n[Delete]\r\n            }", ";");
            }
        }

        #region 日志操作
        public string InsertLog(string TypeName, string Contents, string UserCode)
        {
            Lw_LogInfo log = new Lw_LogInfo()
            {
                TypeName = TypeName,
                Contents = Contents,
                UserCode = UserCode,
                DateTime1 = DateTime.Now
            };
            return Common.add<Lw_LogInfo>(log);
        }
        #endregion

        #endregion

    }
}