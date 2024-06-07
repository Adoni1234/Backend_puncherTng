﻿namespace puncherTng.Models
{
    public class AuthInputs
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class MainAccess
    {
        public string code { get; set; } = string.Empty;
    }

    public class AgenteInput()
    {
        public int id_companie { get; set; } 
        public string name { get; set; } = string.Empty ;
        public string lastName { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string Designatio { get; set; } = string.Empty;

    }

    public class UserInput()
    {
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string? status { get; set; } = string.Empty;

    }

    public class InputAccess
    {
        public string code { get; set; } = string.Empty;
    }

    public class InputCompanies
    {
        public string name { get; set; } = string.Empty;
        public string code_identification { get; set; } = string.Empty;

    }
}
