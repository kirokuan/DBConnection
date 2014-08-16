using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace WhiteLabelTest
{
    public class DBConnection
    {
        private string DBConstr;
        private SqlConnection _Connection;
        public DBConnection(string dbConstr)
        {
            DBConstr = dbConstr;
            _Connection = new SqlConnection(DBConstr);
            _Connection.Open();
            
        }

        public bool Exec(string sql)
        {
            try
            {
                SqlCommand SqlCmd = new SqlCommand(sql, _Connection);
                SqlCmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(sql);
                System.Console.WriteLine(ex.Message);
                return false;
            }
        }
        public List<T> ExecSP<T>(string spName, Dictionary<string, string> param) where T : new()
        {
            return ExecSP<T>(spName,param,GetObject<T>);
        }
        public List<T> ExecSP<T>(string spName, Dictionary<string, string> param, Func<DataRow, T> func) where T : new()
        {
            DataTable dt = ExecSP(spName,param);
            List<T> list = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(func(dr));
            }
            return list;
        }
        public DataTable ExecSP(string spName,Dictionary<string,string> param)
        {
            try
            {
                SqlCommand sqlCmd = new SqlCommand(spName, _Connection);
                sqlCmd.CommandType= CommandType.StoredProcedure;
                foreach (var k in param.Keys)
                {
                    SqlParameter parameter = new SqlParameter();
                    parameter.ParameterName = "@"+k;
                    parameter.Value = param[k];
                    sqlCmd.Parameters.Add(parameter);
                }
                DataTable dt = new DataTable();
                dt.Load(sqlCmd.ExecuteReader());
                return dt;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(spName);
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }

        public int Insert(string sql)
        {
            try
            {
                SqlCommand SqlCmd = new SqlCommand(sql, _Connection);
                SqlCmd.ExecuteNonQuery();
                SqlCommand SqlCmd2 = new SqlCommand("SELECT @@IDENTITY", _Connection);
                 return Convert.ToInt32(SqlCmd2.ExecuteScalar());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(sql);
                System.Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public DataTable GetDataTable(string sql)
        {

            using (SqlCommand cmd = new SqlCommand(sql, _Connection))
            {
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }


        public T GetSingleData<T>(string sql) where T : new()
        {
            return GetSingleData<T>(sql, GetObject<T>);
        }

        public T GetSingleData<T>(string sql, Func<DataRow, T> func) where T : new()
        {
            DataTable dt = GetDataTable(sql);
            if (dt == null || dt.Rows.Count == 0)
            {
                return default(T);//return null
            }
            return func(dt.Rows[0]);
        }

        public List<T> GetDataList<T>(string sql, Func<DataRow, T> func) where T : new()
        {
            DataTable dt = GetDataTable(sql);
            List<T> list = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(func(dr));
            }
            return list;
        }
        public List<T> GetDataList<T>(string sql) where T : new()
        {
            return GetDataList<T>(sql, GetObject<T>);
        }

        public static T GetObject<T>(DataRow dr) where T : new()
        {
            T obj = new T();
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.Public
                                                                       | BindingFlags.Instance).Where(t => dr.Table.Columns.Contains(t.Name)))
            {
                SetFieldValue(obj, fieldInfo.Name, dr[fieldInfo.Name]);
            }
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(BindingFlags.Public
                                                                       | BindingFlags.Instance).Where(t => dr.Table.Columns.Contains(t.Name)))
            {
                SetPropValue(obj, propertyInfo.Name, dr[propertyInfo.Name]);
            }
            return obj;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static void SetPropValue(object src, string propName, object value)
        {
            PropertyInfo propertyInfo = src.GetType().GetProperty(propName);
            propertyInfo.SetValue(src, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }

        public static void SetFieldValue(object src, string propName, object value)
        {
            FieldInfo fieldInfo = src.GetType().GetField(propName);
            fieldInfo.SetValue(src, value);
        }
    }
}
