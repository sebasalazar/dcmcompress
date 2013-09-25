using System;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using Npgsql;
using InterfaceDb;

public class Compress{
   
    public static void Main(string[] args){
    
        int pk = Validate(args);        
        if (pk < 0)
            return;
        Db db = new Db();
        db.Connect();
        
        String query = String.Format(queryPath, pk);
        String fullPath = null;
        IDataReader reader = db.Query(query);
        string pat_name = null;
        db.Close();
        
        while(reader.Read()) {
            pat_name = reader.GetString(reader.GetOrdinal("pat_name")).Replace("^","").Trim().ToLower().Replace(" ","-").Replace(".","-")+pk;
            String archivo = String.Format("/opt/dcm4chee/server/default/archive/{0} ", reader.GetString(reader.GetOrdinal("filepath")));
            if (File.Exists(archivo)) {
	      fullPath += archivo;
            }
        }
        if (fullPath == null)
            return;
        reader.Close();
        reader = null;
        if(!Create(pk, pat_name, fullPath))
            Console.WriteLine("-1");
        fullPath = null;
        return;
        
    }
    
    private static bool Zip(int pk, string pat_name, string fullPath){
        string system = String.Format("7za a -tzip  /srv/web/medipacs.cl/www/htdocs/zip/{0}.zip  {1}", pat_name, fullPath);
        Process tarbz2 = System.Diagnostics.Process.Start(system);
        tarbz2.WaitForExit();
        if(File.Exists(String.Format(path_patient, pat_name, "zip")))
            return true;
        else
            return false;
    }
    
    private static bool Zip2(int pk, string pat_name, string fullPath){
	// Console.WriteLine(fullPath);
        // string system = String.Format("/usr/bin/zip -5 /srv/web/medipacs.cl/www/htdocs/zip/{0}.zip  {1}", pat_name, fullPath);
        string system = String.Format("-5 /srv/web/medipacs.cl/www/htdocs/zip/{0}.zip {1}", pat_name, fullPath);
        // Console.WriteLine(system);
	Process tarbz2 = System.Diagnostics.Process.Start("/usr/bin/zip", system);
        tarbz2.WaitForExit();
        if(File.Exists(String.Format(path_patient, pat_name, "zip")))
            return true;
        else
            return false;
    }
    
    private static bool Bzip2(int pk, string pat_name, string fullPath){
        string system = String.Format("tar -jvcf /srv/web/medipacs.cl/www/htdocs/zip/{0}.tar.bz2  {1}", pat_name, fullPath);
        Process tarbz2 = System.Diagnostics.Process.Start(system);
        tarbz2.WaitForExit();
        if(File.Exists(String.Format(path_patient, pat_name, "tar.bz2")))
            return true;
        else
            return false;
    }    
    
    private static bool Zip7(int pk, string pat_name, string fullPath){
        string system = String.Format("7z a /srv/web/medipacs.cl/www/htdocs/zip/{0}.7z  {1}", pat_name, fullPath);
        Process tarbz2 = System.Diagnostics.Process.Start(system);
        tarbz2.WaitForExit();
        if(File.Exists(String.Format(path_patient, pat_name, "7z")))
            return true;
        else
            return false;
    }
    
    private static bool Create(int pk, string pat_name, string fullPath){
        
        if(!File.Exists(String.Format(path_patient, pat_name,"zip"))){
            if(!Zip2(pk, pat_name, fullPath))
                if(!Zip(pk, pat_name, fullPath))
                    if(!Zip7(pk, pat_name, fullPath))
                        if(!Bzip2(pk, pat_name, fullPath))
                            return false;
                        else
                            return true;
        }
        return true;
    }
    
    private static int Validate(string[] args){
        
        if(args.Length >= 1){
            try{
                return Convert.ToInt32(args[0]);
            }
            catch{
                Console.WriteLine("not number");
                Environment.Exit( 0 );
                return -1;
            }
        }
        else{
            return -1;
        }
    }
    
    static readonly string queryPath = "SELECT patient.pat_name, files.filepath "+
	"FROM patient INNER JOIN study ON (patient.pk = study.patient_fk) INNER JOIN series ON (study.pk = series.study_fk) "+
	"INNER JOIN instance ON (series.pk = instance.series_fk ) INNER JOIN files ON (instance.pk = files.instance_fk) "+
	"INNER JOIN filesystem ON (files.filesystem_fk = filesystem.pk) WHERE (study.pk = {0} ) ";
	
    static readonly string path_patient = "/srv/web/medipacs.cl/www/htdocs/zip/{0}.{1}";

    

}
