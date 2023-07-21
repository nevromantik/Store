using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Store
{
    internal class Program
    {
        static List<Products> cart = new List<Products>();
        static int currentUserId;
        static string currentUserEmail;
        static string currentUserPassword; 
        static void Main(string[] args)
        {

            string connectionString = "Data Source=DESKTOP-4I0O7PP\\SQLEXPRESS;Initial Catalog=store;User ID=SA;Password=ciaociao;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    Login(connection);
                    showMainMenu(connection);
                    
                }
                
             }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();

        }
        public class Customers
        {
            public int IdCliente { get; set; }
            public string Nome { get; set; }
            public string Cognome { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }


        }
        public class Products
        {
            public int IdProdotto { get; set; }
            public string NomeProdotto { get; set; }
            public string Descrizione { get; set; }
            public decimal Prezzo { get; set; }
            public string Categoria { get; set; }

            public int Quantita = 0;
            static decimal Total; 
            public void setQuantity(int quantity)
            {
                this.Quantita = quantity; 
            }
           


        }
        static void Login(SqlConnection connection)
        {
            Console.WriteLine("::Benvenuto/a:: \n" +
                "Inserisci i tuoi dati per accedere al negozio:");
            Console.WriteLine(" Email:");
            string inputEmail = Console.ReadLine();
            Console.WriteLine("Poassword:");
            string inputPsw = Console.ReadLine();
            using (SqlCommand command = new SqlCommand($"SELECT * FROM Cliente WHERE Cliente.Email = '{inputEmail}' AND Cliente.Password = '{inputPsw}'", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Customers customer = new Customers
                            {
                                IdCliente = (int)reader["IdCliente"],
                                Nome = reader["Nome"].ToString(),
                                Cognome = reader["Cognome"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString()
                            };
                            Console.WriteLine($" Bentornato {customer.Nome} {customer.Cognome}! ");
                            currentUserId = customer.IdCliente;
                            currentUserEmail = customer.Email;
                            currentUserPassword = customer.Password; 
                        }

                    }
                    else
                    {
                        Console.WriteLine("Email o password sbagliata. Riprova.");
                    }
                }
            }
        }
        static void showAllCategories(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand($"SELECT DISTINCT Categoria FROM Prodotto ", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string categoria = reader["Categoria"].ToString();
                            Console.WriteLine($" -{categoria} ");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Errore!");
                    }
                }
            }
            Console.WriteLine("Digita il nome della categoria per visualizzarne i prodotti.");


            string inputCategory = Console.ReadLine();

            using (SqlCommand command = new SqlCommand($"SELECT  * FROM Prodotto WHERE Prodotto.Categoria = '{inputCategory}'", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Products product = new Products
                            {
                                IdProdotto = (int)reader["IdProdotto"],
                                NomeProdotto = reader["NomeProdotto"].ToString(),
                                Descrizione = reader["Descrizione"].ToString(),
                                Prezzo = (decimal)reader["Prezzo"],
                                Categoria = reader["Categoria"].ToString()
                            };
                            Console.WriteLine($" -{product.NomeProdotto} \n" +
                                $"{product.Descrizione} \n" +
                                $"Prezzo: {product.Prezzo}$ \n" +
                                $"\n" +
                                $"------------------------------------------------");

                        }

                    }
                    else
                    {
                        Console.WriteLine("Errore!");
                    }
                }
            }

            getSelectedProduct(connection);

        }

        static void showCart(SqlConnection connection)
        {
            foreach(var product in cart)
            {  
                Console.WriteLine($"::::CARRELLO:::: \n" +
                    $"{product.NomeProdotto} \n" +
                    $"{product.Quantita} \n" +
                    $"------------------------------");


                string query = "INSERT INTO Ordine ( DataOrdine, Importo, IdCliente) VALUES (@DataOrdine, @Importo, @idCliente)";




                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    DateTime dataOdierna = DateTime.Today;
                    decimal total1 = cart.Sum(prodotto => prodotto.Quantita * prodotto.Prezzo);

                    command.Parameters.AddWithValue("@DataOrdine", dataOdierna);
                    command.Parameters.AddWithValue("@Importo", total1);
                    command.Parameters.AddWithValue("@IdCliente", currentUserId);


                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            decimal total = cart.Sum(prodotto => prodotto.Quantita * prodotto.Prezzo);

            Console.WriteLine($"TOTALE:  {total}$");

          
        

        }
        static void getSelectedProduct(SqlConnection connection)
        {
            Console.WriteLine("Inserisci il nome del prodotto e la quantità che vuoi acquistare");

            string inputProduct;
            int inputQuantity;

            do
            {
                Console.WriteLine("Nome prodotto (o premi 0 per uscire):");
                inputProduct = Console.ReadLine();

                if (inputProduct == "0")
                    
                    break;

                Console.WriteLine("Quantità:");
                inputQuantity = int.Parse(Console.ReadLine());
                
                using (SqlCommand command = new SqlCommand($"SELECT  * FROM Prodotto WHERE Prodotto.NomeProdotto = '{inputProduct}'", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Products product = new Products
                                {
                                    IdProdotto = (int)reader["IdProdotto"],
                                    NomeProdotto = reader["NomeProdotto"].ToString(),
                                    Descrizione = reader["Descrizione"].ToString(),
                                    Prezzo = (decimal)reader["Prezzo"],
                                    Categoria = reader["Categoria"].ToString()
                                };
                                product.setQuantity(inputQuantity++); 
                                cart.Add(product);
                                Console.WriteLine($"Carrello: +{cart.Count}");

                            }
                        }
                        else
                        {
                            Console.WriteLine("Prodotto non trovato!");
                        }
                    }
                }

                Console.WriteLine("Digita 1 per visualizzare il carrello e procedere all'acquisto \n" +
                    "Digita 2 per continuare ad esplorare lo shop \n" +
                    "Digita 0 per uscire e tornare al menu principale");
                int userChoice = int.Parse(Console.ReadLine());

                switch (userChoice)
                {
                    case 1:
                        showCart(connection);
                        break;
                    case 2:
                        // Lasciare vuoto per continuare con il prossimo inserimento
                        break;
                    case 0:
                        return; // Uscita e ritorno al menu principale
                    default:
                        Console.WriteLine("Scelta non valida!");
                        break;
                }

            } while (true);

        }

        static void changePassword(SqlConnection connection)
        {

            Console.WriteLine($"La tua attuale password è: {currentUserPassword}");
            Console.WriteLine("Inserisci una nuova password: ");
            string newPassword = Console.ReadLine();
            string query = $"UPDATE Cliente SET Password = @NewPassword WHERE IdCliente = {currentUserId}";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NewPassword", newPassword);


                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine($"La tua nuova email: {currentUserPassword}");
        }
        static void changeEmail(SqlConnection connection )
        {

            Console.WriteLine($"La tua attuale email è: {currentUserEmail}");
            Console.WriteLine("Inserisci una nuova email: ");
            string newEmail = Console.ReadLine();
            string query = $"UPDATE Cliente SET Email = @NewEmail WHERE IdCliente = {currentUserId}";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NewEmail", newEmail);
                

                int rowsAffected = command.ExecuteNonQuery();
            }
            Console.WriteLine($"La tua nuova email: {currentUserEmail}");
        }
        static void showAccountSettings(SqlConnection connection)
        {
            Console.WriteLine("::: IMPOSTAZIONI ACCOUNT :::");
            Console.WriteLine("1. Cambia email \n" +
                "2. Cambia password");

            int userChoice = int.Parse(Console.ReadLine());
            switch (userChoice)
            {
                case 1: 
                    changeEmail(connection);
                    break;
                case 2:
                    changePassword(connection);
                    break;
            }
            
           
        }
        static void showMainMenu(SqlConnection connection)
        {
            int userChoice;
        mainMenu:
            Console.WriteLine(":: STORE :: \n" +
                "\n" +
                  "Seleziona la voce desiderata \n" +
                  "1. Visualizza le categorie \n" +
                  "2. Visualizza il carrello  \n" +
                  "3. Impostazioni account \n" 
                 );

            do
            {
                userChoice = int.Parse(Console.ReadLine());

                Console.Clear();

                switch (userChoice)
                {
                    case 1:
                        showAllCategories(connection);
                        if (userChoice == 0)
                        {
                            goto mainMenu;
                        }
                        break;
                    case 2:
                        showCart(connection);
                        break;
                    case 3:
                        showAccountSettings(connection);
                        break;
                   
                }
            } while (userChoice != 0);
        }
    }
}



