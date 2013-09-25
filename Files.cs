using System;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using Npgsql;
using InterfaceDb;
 
namespace FilePatient{
    public class Files{
        private string path;
        private int patient_pk;
        
        
        public Files(string pathInit, int pk){
            path = pathInit;
            patient_pk = pk;
        }
        
        public Files(){
        }
        
        public void setPath(string path){
            this.path = path;
        }
        
        public void setPatient(int patient_pk){
            this.patient_pk = patient_pk;
        }
        
        public void GetFile(){
            if (this.patient_pk > 0){
            
                string query = String.Format("SELECT archivo, nombre  FROM archivos WHERE patient_fk = '{0}'",this.patient_pk);
                Db db = new Db();
                db.Connect();
                IDataReader attach = db.Query(query);
                db.Close();
                while (attach.Read()){
                    long size = attach.GetBytes(attach.GetOrdinal("archivo"),0,null,0,0);
                    string nameAttach = attach.GetString(attach.GetOrdinal("nombre"));
                    string path = String.Format("{0}/{1}", this.path, nameAttach);
                    byte[] poolData = new Byte[size];     
                    size = attach.GetBytes(attach.GetOrdinal("archivo"), 0, poolData, 0, (int)size);
                    BinaryWriter file = new BinaryWriter(File.Create(String.Format(path)));
                    file.Write(poolData);
                }
            }
            else{
                Console.WriteLine("Debe primero setear el paciente!");
            }
        }  
        
        public string WriteFile(string _path, int patient_pk){
            
            string query = String.Format("SELECT archivo, nombre  FROM archivos WHERE patient_fk = '{0}'",patient_pk);
            string files ="";
            Db db = new Db();
            db.Connect();
            IDataReader attach = db.Query(query);
            db.Close();
            while (attach.Read()){
                long size = attach.GetBytes(attach.GetOrdinal("archivo"),0,null,0,0);
                string nameAttach = attach.GetString(attach.GetOrdinal("nombre"));
                files += String.Format("{0}, ",nameAttach);
                string path = String.Format("{0}/{1}", _path, nameAttach);
                byte[] poolData = new Byte[size];     
                size = attach.GetBytes(attach.GetOrdinal("archivo"), 0, poolData, 0, (int)size);
                BinaryWriter file = new BinaryWriter(File.Create(String.Format(path)));
                file.Write(poolData);
                file.Close();
                file=null;
            }
            return files;
        }  
        
        public string WriteDiagnostic(string _path, int study_pk){
            
            string query = String.Format("SELECT diagnostico, titulo FROM diagnostico WHERE study = {0}", study_pk);
            string diag = "";
            Db db = new Db();
            db.Connect();
            IDataReader diagnostic = db.Query(query);
            
            while(diagnostic.Read()){
                string titulo = diagnostic.GetString(diagnostic.GetOrdinal("titulo")).Replace(" ","").Replace("/","").Trim();
                diag += String.Format("{0}, ",titulo);
                string diagnostico = diagnostic.GetString(diagnostic.GetOrdinal("diagnostico"));
                string path = String.Format("{0}/{1}.html", _path, titulo);
                File.WriteAllText(path, diagnostico);
            }
            db.Close();
            return diag;
        }
    
    }
}