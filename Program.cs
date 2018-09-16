using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GestionWookies
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionServerDev))
                {
                    try { 
                        connection.Open();
                    }
                    catch(SqlException)
                    {
                        Console.WriteLine("Connection échouée.");
                        throw;
                    }

                    bool continueSaisie = true;
                    while (continueSaisie)
                    {
                        Console.WriteLine("Veuillez choisir le type d'action :");
                        DisplayEnum(typeof(TypeAction));

                        int action = ReadInteger();
                        switch ((TypeAction) action)
                        {

                            case (TypeAction.QuitterProgramme):
                            {
                                continueSaisie = false;
                            }
                                break;
                            case (TypeAction.Naissance):
                            {
                                Console.WriteLine("Veuillez saisir le nom du wookie nouveau-né : ");
                                string leNom = Console.ReadLine();
                                Wookie leWookie = new Wookie(connection, leNom);
                            }
                                break;
                            case (TypeAction.NaissanceAvecDate):
                            {
                                Console.WriteLine("Veuillez saisir le nom du wookie nouveau-né : ");
                                string leNom = Console.ReadLine();
                                Console.WriteLine("Veuillez saisir sa date de naissance :");
                                string laDate = Console.ReadLine();
                                Wookie leWookie = new Wookie(connection, leNom, laDate);
                            }
                                break;
                            case (TypeAction.Deces):
                            {
                                List<int> listeVivants = ListeVivants(connection, false);
                                Console.WriteLine("Veuillez saisir l'Id du wookie décédé : ");
                                Console.Write("(IDs disponibles : ");
                                for (int i = 0; i < listeVivants.Count; i++)
                                {
                                    Console.Write(listeVivants[i]);
                                    if (i < listeVivants.Count - 1)
                                        Console.Write(", ");
                                }

                                Console.Write(")\n");
                                int num = 0;
                                while (!listeVivants.Contains(num))
                                {
                                    num = ReadInteger();
                                }
                                Console.WriteLine("Veuillez choisir la cause du déces :");
                                DisplayEnum(typeof(TypeDeces));
                                TypeDeces cause = (TypeDeces)ReadInteger();
                                int idDroid = 0;
                                if (cause == TypeDeces.Droide)
                                {
                                    Console.WriteLine("Veuillez saisir l'ID du droide qui a tué le wookie : ");
                                    idDroid = ReadInteger();
                                }
                                Deces(connection, num, cause, idDroid);

                            }
                                break;

                            case (TypeAction.ConsultationMorts):
                            {
                                ListeMorts(connection);
                            }
                                break;
                            case (TypeAction.ConsultationVivants):
                            {
                                ListeVivants(connection);
                            }
                                break;
                            case (TypeAction.ConsultationTues):
                            {
                                ListeTues(connection);
                            }
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            catch(SqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }



        static void Deces(SqlConnection connection, int Id, TypeDeces cause, int numDroide)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                string killer;
                if (numDroide == 0)
                    killer = "NULL";
                else
                    killer = $"{numDroide}";

                command.CommandText = "INSERT INTO [dbo].[DecesWookie]" +
                                        " ([WookieId],[Date],[Cause],[DroideId])" +
                                        " VALUES (" + Id + ", getdate(), " + "'" + cause.ToString() + "'," + killer + ")";
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

        static void ListeMorts(SqlConnection connection)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "select" +
                                      " Wookie.Nom as Nom, DecesWookie.Date as Date, DecesWookie.Cause as Cause" +
                                      " from Wookie" +
                                      " join DecesWookie on DecesWookie.WookieId = Wookie.Id";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Il n'y a aucun wookie décédé dans le registre.");
                    }
                    else
                    {

                        Console.WriteLine("Les wookies décédés : ");
                        Console.WriteLine("Nom\t\tDate de déces\t\tCause de déces");
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["Nom"] + "\t" + reader["Date"] + "\t" + reader["Cause"]);
                        }

                    }
                }
            }
        }

        static void ListeTues(SqlConnection connection)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "select" +
                                " Wookie.Nom, DecesWookie.Date as Date, Droide.Nom as Droide" +
                                " from Wookie" +
                                " join DecesWookie on DecesWookie.WookieId = Wookie.Id" +
                                " join Droide on DecesWookie.DroideId = Droide.Id" +
                                " where DecesWookie.Cause = 'Droide'";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Il n'y a aucun wookie tué dans le registre.");
                    }
                    else
                    {

                        Console.WriteLine("Les wookies tués au combat: ");
                        Console.WriteLine("Nom\t\tDate de déces\t\tTué par");
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["Nom"] + "\t" + reader["Date"] + "\t" + reader["Droide"]);
                        }

                    }
                }
            }
        }

        static List<int> ListeVivants(SqlConnection connection, bool verbose = true)
        {
            List<int> result = new List<int>();
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Wookie.Id as WookieId, Nom, DateNaissance " +
                                      " FROM Wookie" +
                                      " WHERE NOT EXISTS(SELECT NULL From DecesWookie where DecesWookie.WookieId = Wookie.id)";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if(!reader.HasRows && verbose)
                    {
                        Console.WriteLine("Il n'y a pas de wookie vivant dans le registre.");
                    }
                    else
                    {
                        if(verbose)
                            Console.WriteLine("Id\tNom\tDate de naissance");
                        while(reader.Read())
                        {
                            int num;
                            if (int.TryParse(reader["WookieId"].ToString(), out num))
                                result.Add(num);
                            if (verbose)
                                Console.WriteLine(reader["WookieId"] + "\t" + reader["Nom"] + "\t" + reader["DateNaissance"]);
                        }
                    }
                }
            }
            return result;
        }

        static int ReadInteger()
        {
            bool valid = false;
            int num = 0;
            while (!valid)
            {
                valid = int.TryParse(Console.ReadLine(), out num);
            }
            return num;
        }

        static void DisplayEnum(Type te)
        {
            string[] actions = Enum.GetNames(te);
            int[] values = (int[])Enum.GetValues(te);
            for (int i = 0; i < actions.Length; i++)
            {
                Console.Write($"{values[i]} : {actions[i]}{(i == actions.Length - 1 ? ".\n" : ", ")}");
            }
        }
    }
}
