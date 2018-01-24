﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;

namespace TidalWave
{
    /// <summary>
    /// Error from Dynamo. Do Not Use.
    /// </summary>
    public class SQL
    {
        /// <summary>
        /// Open a SQL Connection.
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        public static SqlConnection ConnectionOpen(string dbConnection)
        {
            SqlConnection newConn = new SqlConnection(dbConnection);
            if (dbConnection != null)
            {
                try
                {
                    newConn.Open();
                }
                catch
                {
                    throw new ArgumentException("Database Connection Failed");
                }
            }
            return newConn;
        }
        
        /// <summary>
        /// Create a string to use as an INSERT INTO command for SQL Databases.
        /// </summary>
        /// <param name="Table">The path as a string to the Table.</param>
        /// <param name="Columns">The column names as a list of strings.</param>
        /// <param name="Values">The values to be inserted as a list of list.</param>
        /// <returns>A string containing the INSERT INTO commmand.</returns>
        /// <search>sql, database, insert, inject, add</search>
        public static List<string> InsertInto(string Table, string[] Columns, dynamic[][] Values)
        {
            List<string> holding = new List<string>();
            string start = "INSERT INTO ";
            string table = start + Table + "(";
            string column = "";
            string lastColumn = Columns.Last();
            foreach (var k in Columns)
            {
                if (k.Equals(lastColumn))
                    column = column + k + ") VALUES (";
                else
                    column = column + k + ",";
            }
            table = table + column;
            dynamic lastValue = Values.Last();
            foreach(var i in Values)
            {
                string value = "";
                for (int j = 0; j < i.Length; j++)
                {
                    if (j == i.Length - 1)
                        value = value + "\'" + i[j] + "\');";
                    else
                        value = value + "\'" + i[j] + "\',";
                }
                holding.Add(table + value);
            }
            return holding;
        }
        /// <summary>
        /// Select from a Database some Data.
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="statement"></param>
        /// <returns>Returns information as strings.</returns>
        public static List<dynamic> Select(string Connection, string statement)
        {
            List<dynamic> tempConn = new List<dynamic>();
            SqlDataReader theReader = null;
            SqlConnection db = ConnectionOpen(Connection);
            SqlCommand getRunStat = new SqlCommand(statement, db);
            try
            {
                theReader = getRunStat.ExecuteReader();
                while (theReader.Read())
                {
                    object temp = theReader[0];
                    tempConn.Add(temp);
                }
                ConnectionClose(db);
            }
            catch
            {
                throw new ArgumentException("Database Connected Successfully, but reading the Connection Failed.");
            }
            return tempConn;
        }
        internal static List<int> InsertInto(string newConn, string prjNum, DateTime dateTime)
        {
            SqlConnection tempConn = ConnectionOpen(newConn);
            using (tempConn)
            {
                List<int> type = new List<int>();
                string sql = "INSERT INTO drRunStat(rsDateStart) VALUES (@param2)";
                SqlCommand set = new SqlCommand(sql, tempConn);
                set.Parameters.Add("@param2", SqlDbType.DateTime).Value = dateTime;
                set.CommandType = CommandType.Text;
                int id = set.ExecuteNonQuery();
                type.Add(id);
                string sqlPrj = "INSERT INTO drPrjStat(psPrjNumber,psRunKey) VALUES (@param6)";
                SqlCommand setPrj = new SqlCommand(sqlPrj, tempConn);
                setPrj.Parameters.Add("@param6", SqlDbType.VarChar).Value = prjNum;
                //setPrj.Parameters.Add("@param3", SqlDbType.Int).Value = id;
                setPrj.CommandType = CommandType.Text;
                int backID = setPrj.ExecuteNonQuery();
                type.Add(backID);
                string sqlUpdate = "UPDATE drPrjStat SET psRunKey = " + id +" WHERE psKey = " + backID;
                SqlCommand setUpdate = new SqlCommand(sqlUpdate, tempConn);
                setUpdate.ExecuteNonQuery();
                return type;
            };
        }
        /// <summary>
        /// Generates an Update to your SQL Table using 
        /// </summary>
        /// <param name="dateEx">internal</param>
        /// <param name="key">internal</param>
        /// <param name="Connection">internal</param>
        /// <param name="fail">internal</param>
        internal static string Update(DateTime dateEx, int key, string Connection, int fail = 2)
        {
            SqlConnection tempConn = ConnectionOpen(Connection);
            using (tempConn)
            {
                try
                {
                    string commandtext = "";
                    SqlCommand update = new SqlCommand();
                    DateTime dateCom = DateTime.Now;

                    using (update)
                    {
                        update.Connection = tempConn;
                        if (fail < 2)
                        {
                            commandtext = "UPDATE drRunStat SET rsDateEnd = @dateCom WHERE rsKey = @Key";
                        }
                        else
                        {
                            commandtext = "UPDATE drRunStat SET rsDateEnd = @dateCom, rsRunFailure = @fail WHERE rsKey = @Key";
                            update.Parameters.Add("@fail", SqlDbType.Bit).Value = fail;
                        }
                        update.CommandText = commandtext;
                        update.Parameters.Add("@dateCom", SqlDbType.DateTime).Value = dateCom;
                        //update.Parameters.Add("@dateEx", SqlDbType.DateTime).Value = dateEx;
                        update.Parameters.Add("@Key", SqlDbType.Int).Value = key;
                        update.ExecuteNonQuery();
                        //consider tracking rows affected
                    }
                    return "Executed Successfully.";
                }
                catch (SqlException e)
                {
                    System.Diagnostics.Debug.WriteLine("SQL Line 154 : " + e.ToString());
                    return "Execution Failed. " + e.ToString();
                    //throw new ArgumentException("Database Could Not Accept the Insert as Requested. " + e);
                };

            };
        }
        public static List<string> Update(string Table, List<string> Columns, List<List<dynamic>> Values, string Where)
        {
            List<string> holding = new List<string>();
            try
                {
                    string start = "UPDATE ";
                    string table = start + Table + "SET ";
                    for (int i = 0; i < Values.Count; i++)
                    {
                        string column = "";
                        for (int j = 0; j < Values[i].Count; j++)
                        {
                            if (j == Columns.Count - 1)
                                column = column + Columns[j] + " = " + Values[i][j].ToString() + " WHERE ";
                            else
                                column = column + Columns[j] + " = " + Values[i][j].ToString() + ", ";
                        }
                        string where = table + column + Where;
                        holding.Add(where);
                    };
                }
                catch
                {
                    throw new Exception();
                };
            return holding;
        }
        /// <summary>
        /// Closes a SQL Connection.
        /// </summary>
        /// <param name="Connection">The Connection as a SQLConnection object.</param>
        public static void ConnectionClose(SqlConnection Connection)
        {
            try
            {
                Connection.Close();
            }
            catch (Exception)
            {
                throw new ArgumentException("Closing the Database Connection Failed");
            }
        }
    }
}
