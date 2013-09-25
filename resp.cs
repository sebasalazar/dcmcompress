using System;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using Npgsql;
 
public class Test{
    
    public static void Main(string[] args){
                
        string connectionString =
          "Server=localhost;" +
          "Database=pacsdb;" +
          "User ID=pacs;" +
          "Password=pacs;";
            IDbConnection dbcon;
            dbcon = new NpgsqlConnection(connectionString);
            dbcon.Open();
            IDbCommand dbcmdAttach = dbcon.CreateCommand();                    
                    
                
            string attach = String.Format("SELECT archivo, nombre  FROM archivos LIMIT 1");
            Console.WriteLine(attach);
            dbcmdAttach.CommandText = attach;
            IDataReader readerAttach = dbcmdAttach.ExecuteReader();
            
            while(readerAttach.Read()){
                Console.WriteLine("hola");
                long size = readerAttach.GetBytes(readerAttach.GetOrdinal("archivo"),0,null,0,0);
                string nameAttach = readerAttach.GetString(readerAttach.GetOrdinal("nombre"));
                string path = String.Format("/home/dicom/backup/{0}",nameAttach);
                Console.WriteLine(size);
                byte[] poolData = new Byte[size];                
                if (File.Exists(path)) File.Delete(path);
//                 File out_file = File.OpenWrite(path);
                size = readerAttach.GetBytes(readerAttach.GetBytes("archivo"), 0, poolData, 0, (int)size);
//                 out_file.Write(poolData, 0, (int)size);
                readerAttach.GetBytes(1,0,poolData,0,size);
                BinaryWriter file = new BinaryWriter(File.Create(String.Format(path)));
                file.Write(poolData);
            }
            
            readerAttach.Close();
            readerAttach = null;
            dbcmdAttach.Dispose();
            dbcmdAttach = null;
        
        
        dbcon.Close();
        dbcon = null;
    }
 }
