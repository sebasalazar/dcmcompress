using System;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using Npgsql;
using FilePatient;
using InterfaceDb;
 
public class Backup{

    /** 
    * Declaracion de Strings que correspondera a las grandes consultas de el programa!
    */
    static readonly string totalStudy = "SELECT DISTINCT(study.pk), patient.pat_id, patient.pat_name, series.institution, study.ref_physician, patient.pk AS patientPk, study.created_time " +
                "FROM patient INNER JOIN study ON ( patient.pk = study.patient_fk) INNER JOIN series ON (study.pk = series.study_fk) " +
                    "INNER JOIN instance ON (series.pk = instance.series_fk ) INNER JOIN files ON (instance.pk = files.instance_fk) " + 
                    "INNER JOIN filesystem ON (files.filesystem_fk = filesystem.pk) "+
                    "WHERE ( {0}  = date_part('month',study.created_time) AND date_part('year', CURRENT_TIMESTAMP) = date_part('year', study.created_time) ) " + 
                    "AND series.institution = '{1}' "+
                    "ORDER BY pk";
    
    static readonly string absolutPath = "SELECT files.filepath FROM patient INNER JOIN study ON ( patient.pk = study.patient_fk) INNER JOIN series ON (study.pk = series.study_fk) " + 
                    "INNER JOIN instance ON (series.pk = instance.series_fk ) INNER JOIN files ON (instance.pk = files.instance_fk) INNER JOIN filesystem ON (files.filesystem_fk = filesystem.pk) " + 
                    "WHERE study.pk = {0}";
    static readonly string backup = "/home/dicom/backup/";
    /**
    *Declaracion del metodo principal
    */
    
    public static void Main(string[] args){
        
        try{
            if(Directory.Exists(backup))
                Directory.Delete(backup, true);
        }
        catch(Exception e){
            Console.WriteLine("The process failed: {0}", e.ToString());
        }
        System.IO.Directory.CreateDirectory(backup);
        Console.WriteLine(args.Length);
        
        string month;    
        if(args.Length >= 1){
            try{
                month = Convert.ToInt32(args[0]).ToString();
            }
            catch{
                Console.WriteLine("not number");
                Environment.Exit( 0 );
                return;
            }
        }
        else{
            month = "date_part('month',CURRENT_TIMESTAMP)";
        }
        
        Db db = new Db();
        db.Connect();
        string fileLine = "";
        string sql = String.Format("SELECT DISTINCT(series.institution) FROM series INNER JOIN study ON (series.study_fk = study.pk) WHERE ( {0} = date_part('month',study.created_time))", month);
        IDataReader reader = db.Query(sql);        
        
        string fullPath = "";
        while(reader.Read()) {
            
            string institution = reader.GetString(reader.GetOrdinal("institution"));
            string nameInst = institution.Replace(" ","").Replace("/","-").Trim();
            System.IO.Directory.CreateDirectory(String.Format("{0}/{1}",backup,nameInst));
            
            string fullId = String.Format(totalStudy ,month ,institution );
            institution = institution.Replace("/","-");
            
            StreamWriter fs = File.AppendText(String.Format("{0}/{1}/{2}.csv",backup,nameInst, institution));
            fs.WriteLine("Paciente ID, Study ID, Institution, Nombre Paciente, Doc Referencia, Adicionales");
            IDataReader readerId = db.Query(fullId);
            
            
            while(readerId.Read()){
                
                
                int studyId = readerId.GetInt32(readerId.GetOrdinal("pk"));
                int patPk = readerId.GetInt32(readerId.GetOrdinal("patientPk"));
                string pat_id = readerId.GetString(readerId.GetOrdinal("pat_id")).Replace(" ","").Trim();
                string path =  String.Format(absolutPath, studyId);
                string pat_name = readerId.GetString(readerId.GetOrdinal("pat_name"));
                string ref_physician = null;
                try{
                    ref_physician = readerId.GetString(readerId.GetOrdinal("ref_physician"));
                }
                catch {
                    Console.WriteLine("No se pudo hacer cast");
                    ref_physician = "NULL";
                }
                string time = readerId.GetDateTime(readerId.GetOrdinal("created_time")).ToString();
                fileLine = String.Format("{0}, {1}, {2}, {3}, {4}, {5},", pat_id, studyId, institution, pat_name, ref_physician, time);
                IDataReader readerPath = db.Query(path);
                
                while(readerPath.Read()){
                    fullPath += String.Format("/home/dicom/dcm4chee-standalone-psql-2.11.0/server/default/archive/{0} ", readerPath.GetString(readerPath.GetOrdinal("filepath")));
                }
                readerPath.Close();
                readerPath = null;
                
                path = String.Format("{0}/{1}/{2}",backup,nameInst, pat_id);
                string system = String.Format("tar -jvcf {0}.tar.bz2  {1}", path, fullPath);
                Process tarbz2 = System.Diagnostics.Process.Start(system);
                tarbz2.WaitForExit();
                string filesForPatient = String.Format("SELECT archivo_id FROM archivos WHERE patient_fk={0}",patPk);
                IDataReader filesPat = db.Query(filesForPatient);
                
                if( db.Count(filesPat) > 0){
                    System.IO.Directory.CreateDirectory(path);
                    Files fl = new Files();
                    fileLine += fl.WriteFile(path, patPk);
                }
                
                filesPat.Close();
                
                IDataReader diag = db.Query(String.Format("SELECT diagnostico_id FROM diagnostico WHERE study = {0}", studyId));
                
                if(db.Count(diag) > 0){
                    if(!File.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    Files fl = new Files();
                    fileLine +=fl.WriteDiagnostic(path,studyId);
                    
                }
                
                diag.Close();
                fs.WriteLine(fileLine);
                fs.Flush();
                fileLine = "";
                
                
                fullPath = null;
                system = null;
            }
            readerId.Close();
            readerId = null;
            
            string mkiso = String.Format("mkisofs  -R -v -o /var/iso/{3}-{0}.iso {1}/{2}",nameInst, backup ,nameInst, month );
            Process iso = System.Diagnostics.Process.Start(mkiso);
            iso.WaitForExit();
            Directory.Delete(backup, true);
            fs.Close();
            
        }
        
        
        reader.Close();
        reader = null;
        db.Close();
    }
    
 }