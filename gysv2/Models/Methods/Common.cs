using gysv2.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


/// <summary>
/// 
/// 根据实体类生成sql
/// 
/// </summary>
public class Common
{

    /// <summary>
    /// 根据设置的bean条件实体类 
    /// 返回查询的sql字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bean"></param>
    /// <param name="page">当前页数-1表示不分页 从1开始</param>
    /// <param name="pageSize">每页数据个数</param>
    /// <returns></returns>
    public static string find<T>(T bean)
    {
        if (bean == null)
        {
            return null;
        }
        List<string> SelProperty = new List<string>();
        List<string> WhereProperty = new List<string>();
        string sql;
        foreach (System.Reflection.PropertyInfo p in bean.GetType().GetProperties())
        {
            Object[] objs = p.GetCustomAttributes(typeof(Property), false);
            if (objs.Length == 0)
            {//不是属性
                continue;
            }
            else
            {//是属性
                Property Pro = (Property)objs[0];
                if (!Pro.NotSelect)
                {
                    SelProperty.Add(p.Name);
                }
                //不是主键也不是外键
                if (string.IsNullOrEmpty(Pro.PType))
                {
                    object str = p.GetValue(bean, null);
                    if (str != null)
                    {
                        Type PType = p.PropertyType;
                        if (typeof(long) == PType || typeof(int) == PType || typeof(decimal) == PType || typeof(float) == PType)
                        {
                            if (typeof(long) == PType)
                            {
                                long temp = long.Parse(str.ToString());
                                if (temp == 0)
                                {
                                    continue;
                                }
                            }
                            if (Pro.Model == 0)
                            {
                                if (str.ToString() == "-1")
                                {
                                    continue;
                                }
                                else
                                {
                                    WhereProperty.Add(p.Name + $"={str.ToString()}");
                                }
                            }
                            else if (Pro.Model == 1)
                            {
                                if (str.ToString() == "-1")
                                {
                                    continue;
                                }
                                else
                                {
                                    WhereProperty.Add(p.Name + $"<={str.ToString()}");
                                }
                            }
                            else if (Pro.Model == 3)
                            {
                                continue;
                            }
                            else
                            {
                                if (str.ToString() == "-1")
                                {
                                    continue;
                                }
                                else
                                {
                                    WhereProperty.Add(p.Name + $">={str.ToString()}");
                                }

                            }
                        }
                        else if (typeof(string) == PType)
                        {
                            if (string.IsNullOrWhiteSpace(str.ToString()))
                            {
                                continue;
                            }
                            if (Pro.StringModel == 1)
                            {//是日期类型的字符串
                             //"2021-02-01T02:50:00.245Z,2021-02-28T02:50:00.245Z"
                                string[] Dates = str.ToString().Split(',');
                                if (Dates.Length == 0)
                                {
                                    continue;
                                }
                                else if (Dates.Length == 1)
                                {
                                    //Dates[0]
                                    switch (Pro.Model)
                                    {
                                        case 0://精确匹配
                                            WhereProperty.Add(p.Name + $"='{Dates[0]}'");
                                            break;
                                        case 1://小于等于
                                            WhereProperty.Add(p.Name + $"<='{Dates[0]}'");
                                            break;
                                        case 2://大于等于
                                            WhereProperty.Add(p.Name + $">='{Dates[0]}'");
                                            break;
                                    }
                                }
                                else//有两个值 介于查询
                                {
                                    if (string.IsNullOrEmpty(Dates[0]))
                                    {
                                        WhereProperty.Add(p.Name + $"<='{Dates[1]}'");
                                    }
                                    else
                                    {
                                        WhereProperty.Add(p.Name + $" between '{Dates[0]}' AND '{Dates[1]}'");
                                    }
                                }
                            }
                            else
                            {//普通的字符串
                                if (Pro.Model == 0)
                                {
                                    WhereProperty.Add(p.Name + $"='{str.ToString()}'");
                                }
                                else if (Pro.Model == 1)
                                {
                                    WhereProperty.Add(p.Name + $" like '%{str.ToString()}%'");
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(Pro.Colums))
                                    {
                                        WhereProperty.Add($"{p.Name} in ({str.ToString()})");
                                    }
                                    else
                                    {
                                        WhereProperty.Add($"{Pro.Colums} in ({str.ToString()})");
                                    }

                                }
                            }
                        }
                        else if (typeof(DateTime) == PType || typeof(DateTime?) == PType)
                        {
                            DateTime Dt = DateTime.Parse(str.ToString());

                            string DtStr = Dt.ToString("yyyy-MM-dd HH:mm:ss");
                            if (DtStr == "0001-01-01 00:00:00")
                            {
                                continue;
                            }
                            if (Pro.Model == 0)
                            {
                                WhereProperty.Add(p.Name + $"='{DtStr}'");
                            }
                            else if (Pro.Model == 1)
                            {
                                WhereProperty.Add(p.Name + $"<='{DtStr}'");
                            }
                            else
                            {
                                WhereProperty.Add(p.Name + $">={DtStr}");
                            }
                        }

                    }

                }
                else if (Pro.PType == "PK")
                {//是主键
                    object str = p.GetValue(bean, null);
                    if (str != null && str.ToString() != "0")
                    {
                        WhereProperty.Add(p.Name + $"={str.ToString()}");
                    }
                }
                else if (Pro.PType == "FK")
                {//是外键

                }
            }
        }
        string SelectStr = string.Join(",", SelProperty);
        string WhereStr = "";
        if (WhereProperty.Count > 0)
        {
            WhereStr = " WHERE " + string.Join(" AND ", WhereProperty);
        }

        sql = $"SELECT {SelectStr} FROM {typeof(T).Name} {WhereStr}";
        return sql;
    }


    /// <summary>
    /// 根据传递的实体类
    /// 返回添加的sql字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bean"></param>
    /// <returns></returns>
    public static string add<T>(T bean)
    {
        if (bean == null)
        {
            return null;
        }
        List<string> SelProperty = new List<string>();
        List<string> ValProperty = new List<string>();
        string sql;
        foreach (System.Reflection.PropertyInfo p in bean.GetType().GetProperties())
        {
            Object[] objs = p.GetCustomAttributes(typeof(Property), false);
            if (objs.Length == 0)
            {//不是属性
                continue;
            }
            else
            {//是属性
                Property Pro = (Property)objs[0];
                //不是主键也不是外键
                if (string.IsNullOrEmpty(Pro.PType))
                {
                    object str = p.GetValue(bean, null);
                    if (str != null)
                    {

                        Type PType = p.PropertyType;
                        if (typeof(long) == PType || typeof(int) == PType || typeof(decimal) == PType || typeof(float) == PType)
                        {
                            SelProperty.Add(p.Name);
                            ValProperty.Add($"{str.ToString()}");
                        }
                        else if (typeof(string) == PType)
                        {
                            SelProperty.Add(p.Name);
                            ValProperty.Add($"'{str.ToString()}'");
                        }
                        else if (typeof(DateTime) == PType || typeof(DateTime?) == PType)
                        {
                            DateTime Dt = DateTime.Parse(str.ToString());
                            string DtStr = Dt.ToString("yyyy-MM-dd HH:mm:ss");
                            if (DtStr == "0001-01-01 00:00:00")
                            {
                                continue;
                            }
                            SelProperty.Add(p.Name);
                            ValProperty.Add($"'{DtStr}'");
                        }
                    }
                }
                else if (Pro.PType == "PK")
                {//是主键
                }
                else if (Pro.PType == "FK")
                {//是外键
                }
            }
        }
        string SelectStr = "(" + string.Join(",", SelProperty) + ")";
        string ValStr = "";
        if (ValProperty.Count > 0)
        {
            ValStr = "(" + string.Join(" , ", ValProperty) + ")";
        }
        sql = $"INSERT INTO  {typeof(T).Name}{SelectStr}VALUES{ValStr}";
        return sql;
    }

    /// <summary>
    /// 生成批量添加的sql字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="beans"></param>
    /// <returns></returns>
    public static List<string> adds<T>(List<T> beans)
    {
        if (beans.Count == 0)
        {
            return null;
        }
        List<string> sqls = new List<string>();
        beans.ForEach(item =>
        {
            string sql = Common.add<T>(item);
            sqls.Add(sql);
        });
        return sqls;
    }

    /// <summary>
    /// 根据传入实体类ID，
    /// 返回 删除ID=实体类.id的 sql字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bean"></param>
    /// <returns></returns>
    public static string del<T>(T bean)
    {
        if (bean == null)
        {
            return null;
        }
        string PK = "";
        string ValProperty = "";
        string sql;
        foreach (System.Reflection.PropertyInfo p in bean.GetType().GetProperties())
        {
            Object[] objs = p.GetCustomAttributes(typeof(Property), false);
            if (objs.Length == 0)
            {//不是属性
                continue;
            }
            else
            {//是属性
                Property Pro = (Property)objs[0];
                //不是主键也不是外键
                if (string.IsNullOrEmpty(Pro.PType))
                {
                    continue;
                }
                else if (Pro.PType == "PK")
                {//是主键
                    object str = p.GetValue(bean, null);
                    if (str != null && str.ToString() != "0")
                    {
                        Type PType = p.PropertyType;
                        if (Pro.Model == 3)
                        {
                            continue;
                        }
                        if (typeof(long) == PType || typeof(int) == PType || typeof(decimal) == PType || typeof(float) == PType)
                        {
                            PK = p.Name;
                            ValProperty = $"{str.ToString()}";
                        }
                        else if (typeof(string) == PType)
                        {
                            PK = p.Name;
                            ValProperty = $"'{str.ToString()}'";
                        }
                    }
                }
                else if (Pro.PType == "FK")
                {//是外键
                }
            }
        }
        if (string.IsNullOrWhiteSpace(ValProperty))
        {
            return null;
        }
        else
        {
            sql = $"DELETE {typeof(T).Name} WHERE {PK}={ValProperty}";
            return sql;
        }
    }

    public static List<string> dels<T>(List<T> beans)
    {
        if (beans.Count == 0)
        {
            return null;
        }
        List<string> sqls = new List<string>();
        beans.ForEach(item =>
        {
            string sql = Common.del<T>(item);
            sqls.Add(sql);
        });
        return sqls;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bean1">要更新的值</param>
    /// <param name="bean2">条件</param>
    /// <returns></returns>
    public static string updata<T>(T bean1, T bean2)
    {
        if (bean1 == null || bean2 == null)
        {
            return null;
        }
        List<string> SetSql = new List<string>();
        List<string> WhereSql = new List<string>();
        //获得SET 字符串
        foreach (System.Reflection.PropertyInfo p in bean1.GetType().GetProperties())
        {
            Object[] objs = p.GetCustomAttributes(typeof(Property), false);
            if (objs.Length == 0)
            {//不是属性
                continue;
            }
            else
            {
                Property Pro = (Property)objs[0];
                //不是主键也不是外键
                if (string.IsNullOrEmpty(Pro.PType))
                {
                    object str = p.GetValue(bean1, null);
                    if (str != null)
                    {
                        Type PType = p.PropertyType;
                        if (typeof(long) == PType || typeof(int) == PType || typeof(decimal) == PType || typeof(float) == PType)
                        {//数字
                            if (typeof(long) == PType)
                            {
                                long temp = long.Parse(str.ToString());
                                if (temp != 0 && temp != -1)
                                {
                                    SetSql.Add(p.Name + $"={temp}");
                                }
                            }
                            else if (typeof(int) == PType)
                            {
                                int temp = int.Parse(str.ToString());
                                if (temp != 0 && temp != -1)
                                {
                                    SetSql.Add(p.Name + $"={temp}");
                                }
                            }
                            else
                            {
                                decimal temp = decimal.Parse(str.ToString());
                                if (temp != 0 && temp != -1)
                                {
                                    SetSql.Add(p.Name + $"={temp}");
                                }
                            }


                        }
                        else if (typeof(string) == PType)
                        {//字符串
                            SetSql.Add(p.Name + $"='{str.ToString()}'");
                        }
                        else if (typeof(DateTime) == PType || typeof(DateTime?) == PType)
                        {//日期
                            DateTime Dt = DateTime.Parse(str.ToString());
                            string DtStr = Dt.ToString("yyyy-MM-dd HH:mm:ss");
                            if (DtStr == "0001-01-01 00:00:00")
                            {
                                continue;
                            }
                            SetSql.Add(p.Name + $"='{str.ToString()}'");
                        }
                    }
                }
                else if (Pro.PType == "PK")
                {
                    //不做更新
                }
            }

        }
        //获得WHERE字符串
        foreach (System.Reflection.PropertyInfo p in bean2.GetType().GetProperties())
        {
            Object[] objs = p.GetCustomAttributes(typeof(Property), false);
            if (objs.Length == 0)
            {//不是属性
                continue;
            }
            else
            {
                Property Pro = (Property)objs[0];
                //不是主键也不是外键
                if (string.IsNullOrEmpty(Pro.PType))
                {
                    object str = p.GetValue(bean2, null);
                    if (str != null)
                    {
                        Type PType = p.PropertyType;
                        if (Pro.Model == 3)
                        {
                            continue;
                        }
                        if (typeof(long) == PType || typeof(int) == PType || typeof(decimal) == PType || typeof(float) == PType)
                        {//数字
                            WhereSql.Add(p.Name + $"={str.ToString()}");
                        }
                        else if (typeof(string) == PType)
                        {//字符串
                            WhereSql.Add(p.Name + $"='{str.ToString()}'");
                        }
                        else if (typeof(DateTime) == PType || typeof(DateTime?) == PType)
                        {//日期
                            DateTime Dt = DateTime.Parse(str.ToString());
                            string DtStr = Dt.ToString("yyyy-MM-dd HH:mm:ss");
                            if (DtStr == "0001-01-01 00:00:00")
                            {
                                continue;
                            }
                            WhereSql.Add(p.Name + $"='{str.ToString()}'");
                        }
                    }
                }
                else if (Pro.PType == "PK")
                {//主键
                    object str = p.GetValue(bean2, null);
                    if (str != null && str.ToString() != "0")
                    {
                        Type PType = p.PropertyType;
                        if (typeof(long) == PType || typeof(int) == PType || typeof(decimal) == PType || typeof(float) == PType)
                        {
                            WhereSql.Add(p.Name + $"={str.ToString()}");
                        }
                        else if (typeof(string) == PType)
                        {
                            WhereSql.Add(p.Name + $"='{str.ToString()}'");
                        }
                    }
                }
            }

        }
        //不设条件 防止全部更新 
        if (WhereSql.Count == 0 || SetSql.Count == 0)
        {
            return null;
        }
        string SetSqlStr = string.Join(",", SetSql);
        string WhereSqlStr = string.Join(",", WhereSql);
        string sql = $"UPDATE {typeof(T).Name} SET {SetSqlStr} WHERE {WhereSqlStr}";
        return sql;

    }

    /// <summary>
    /// 1.sqls的个数必须和Type的个数相同
    /// 2.sql 1 查询出的对象 必须和Type所指的类型相同
    /// </summary>
    /// <param name="sqls"></param>
    /// <param name="Types"></param>
    /// <returns></returns>
    public static JObject findCommond(List<string> sqls, List<Type> types, int page, int pageSize, string ConnPath)
    {
        if (sqls.Count == 0 || sqls.Count != types.Count)
        {
            return null;
        }

        //获得数据库配置对象
        string connStr = project_Lw_Utils.Lw_Utils.getConnStr2(ConnPath, "CommonStr");
        SqlConnection conn = new SqlConnection(connStr);
        SqlDataAdapter ada = null;
        JObject jo = new JObject();
        //SqlCommand scmd = new SqlCommand();
        try
        {
            string Cmds = string.Join(";", sqls.ToArray());
            ada = new SqlDataAdapter(Cmds, conn);
            DataSet ds = new DataSet();
            ada.Fill(ds);
            int rowbegin = (page - 1) * pageSize;
            int rowend = page * pageSize;
            int j = 0;
            foreach (DataTable tb in ds.Tables)
            {
                int Count = tb.Rows.Count;
                int rowEndInside = rowend;
                // ja = JArray.Parse(JsonConvert.SerializeObject(tb));
                JArray ja = new JArray();
                if (rowbegin <= Count)
                {
                    if (rowEndInside >= Count)
                    {
                        rowEndInside = Count;
                    }
                    for (int i = rowbegin; i < rowEndInside; i++)
                    {
                        DataRow dr = tb.Rows[i];
                        JObject jo3 = new JObject();
                        foreach (DataColumn column in tb.Columns)
                        {
                            object obj = dr[column.ColumnName];
                            jo3.Add(column.ColumnName, obj != null ? obj.ToString() : "");
                        }
                        ja.Add(jo3);
                    }
                }
                jo.Add(types[j].Name, ja);
                //数据总数
                jo.Add(types[j].Name + "_Count", Count);
                j++;
            }

            return jo;
        }
        catch (Exception _e)
        {
            throw _e;
        }
        finally
        {
            if (conn != null && conn.State != System.Data.ConnectionState.Closed)
            {
                conn.Close();
            }
            if (ada != null)
            {
                ada.Dispose();
            }
        }
    }

    public static JObject findCommond(string sql, Type type, int page, int pageSize, string ConnPath)
    {
        try
        {
            List<string> sqls = new List<string>();
            List<Type> Types = new List<Type>();
            sqls.Add(sql);
            Types.Add(type);
            JObject objs = findCommond(sqls, Types, page, pageSize, ConnPath);
            if (objs == null)
            {
                return null;
            }
            else
            {
                return objs;
            }
        }
        catch (Exception _e)
        {
            throw _e;
        }

    }

    public static bool OptCommond(List<string> sqls, string ConnPath)
    {
        if (sqls.Count == 0)
        {
            return false;
        }
        //获得数据库配置对象
        string connStr = project_Lw_Utils.Lw_Utils.getConnStr2(ConnPath, "CommonStr");
        SqlConnection conn = new SqlConnection(connStr);
        SqlTransaction tr = null;
        try
        {
            conn.Open();
            tr = conn.BeginTransaction();
            SqlCommand scmd = new SqlCommand();
            scmd.Connection = conn;
            scmd.Transaction = tr;
            foreach (string sql in sqls)
            {
                scmd.CommandText = sql;
                int i = scmd.ExecuteNonQuery();
            }
            tr.Commit();
        }
        catch (Exception _e)
        {
            if (tr != null)
            {
                tr.Rollback();
            }
            throw _e;


        }
        finally
        {
            if (conn != null && conn.State != System.Data.ConnectionState.Closed)
            {
                conn.Close();
            }
        }
        return true;
    }
    public static bool OptCommond(string sql, string ConnPath)
    {
        try
        {
            List<string> sqls = new List<string>();
            List<Type> Types = new List<Type>();
            sqls.Add(sql);
            bool flg = OptCommond(sqls, ConnPath);
            return flg;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
