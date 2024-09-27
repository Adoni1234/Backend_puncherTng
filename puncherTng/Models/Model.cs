namespace puncherTng.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty ;
        public string email { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;

    }


    public class Agentes
    {
        public int Id { get; set; }
        public int id_companie { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string designation { get; set; } = string.Empty;

    }

    public class AccessCode
    {
        public int  Id { get; set; }
        public int IdAgente { get; set; }
        public string code { get; set; } = string.Empty;
    }

    public class History
    {
        public int Id { get; set; }
        public string AdmissionDate { get; set; } = string.Empty;
        public string? ExitDate { get; set; } = string.Empty;
        public int IdAngente { get; set; } 

    }

    public class Access 
    { 
       public int Id { get; set; }
       public string code { get; set; } = string.Empty;
    }

    public class Companies
    {
        public int Id { get; set; }
        public string name { get; set; } = string.Empty;
        public string code_identification { get; set; } = string.Empty;
    }

    public class history_audit
    {
        public int Id { get; set; }
        public string AdmissionDate { get; set; } = string.Empty;
        public string? ExitDate { get; set; } = string.Empty;
        public int IdAngente { get; set; }
    }

}
