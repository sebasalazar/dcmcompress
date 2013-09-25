using System;
// using System.IO;
using System.Data;
using InterfaceDb;
// using Npgsql;
using System.Collections;
// using FilePatient;

public class Prueba{

    
    
    public static int Main(){
        Db db = new Db();
        db.Connect();
        IDataReader reader = db.Query("SELECT * FROM study");
        Console.WriteLine(db.Count(reader));
        db.Close();
        
        return 0;
    }
}