using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionWookies
{
    public class Wookie
    {
        private int _id;
        public SqlConnection connection;
        public SqlCommand command;
        private readonly string _nom;
        private string _dateNaissance { get; set; }
        private DateTime _dateDeces { get; set; }



        public Wookie(SqlConnection conn, string nom, string dateNaissance)
        {
            _nom = nom;
            _dateNaissance = dateNaissance;
            connection = conn;
            this.BD_CreerWookie(dateNaissance);
        }

        public Wookie(SqlConnection conn, string nom)
        {
            _nom = nom;
            _dateNaissance = DateTime.Now.ToString();
            connection = conn;
            this.BD_CreerWookie(_dateNaissance);
        }

        public Wookie(int id, string nom, string dateNaissance)
        {
            _nom = nom;
            _dateNaissance = dateNaissance;
            _id = id;
        }

        public Wookie(int id, string nom)
        {
            _nom = nom;
            _id = id;
        }


        public void DeclarerDeces()
        {
            _dateDeces = DateTime.Now;
        }

        public void DeclarerDeces(DateTime dateDeces)
        {
            _dateDeces = dateDeces;
        }


        public void BD_DeclarerDeces()
        {


        }


        private void BD_CreerWookie(string date)
        {
            string maRequete = "INSERT INTO Wookie (Nom,DateNaissance) VALUES(@nomp, @datep)";
            command = new SqlCommand(maRequete, connection);
            command.Parameters.Add(new SqlParameter("@nomp", SqlDbType.NChar, 10));
            command.Parameters.Add(new SqlParameter("@datep", SqlDbType.DateTime, 4));
            command.Parameters["@nomp"].Value = _nom;
            command.Parameters["@datep"].Value = date;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                Console.WriteLine("Écriture échouée.");
                throw;
            }
        }

    }
}
