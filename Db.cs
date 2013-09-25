using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using Npgsql;

namespace InterfaceDb{
    
    public class Db{
        
        private IDbConnection dbcon = null;
        
        private IDbCommand dbcmd = null;
        
        public Db(){
        }
        
        public void Connect(){
            
            this.dbcon = new NpgsqlConnection(String.Format("Server=localhost; Database=pacsdb; User ID=pacs; Password=pacs"));
            dbcon.Open();
            Console.WriteLine("Conectado... ok");
        }
        
        public void Connect(string host, string dbname, string user, string passwd){
            
            this.dbcon = new NpgsqlConnection(String.Format("Server={0}; Database={1}; User ID={2}; Password={3}", host, dbname, user, passwd));
            dbcon.Open();
            Console.WriteLine("Conectado... ok");
        }
        
        public void Close(){
        
            try{
                
                if(this.dbcon != null){
                    dbcon.Close();
                    Console.WriteLine("Cerrado... ok");
                }
                else
                    Console.WriteLine("No hay conexion");
            }
            catch(SqlException e){
                Console.WriteLine("Error SQL CLose Conn, Error: {0}", e.Message);
            }
        }
        
        public IDataReader Query(string query){
            
            if(this.dbcon != null){
                this.dbcmd = this.dbcon.CreateCommand();
                dbcmd.CommandText = query;
                IDataReader reader = dbcmd.ExecuteReader();
                dbcmd = null;
                return reader;
            }
            else{
                Console.WriteLine("No hay establecida una conexion con la BD");
                return null;
            }
        }
        
        public int Count(IDataReader reader){
            
            int i = 0;
            
            try{
                while(reader.Read())
                    i++;
                return i;
            }
            catch(SqlException e) {
                Console.WriteLine("Error Sql : {0}", e.Message);
                return -1;
            }
        }
    }
}