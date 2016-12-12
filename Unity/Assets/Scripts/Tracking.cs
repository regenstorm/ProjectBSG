using UnityEngine;
using System.Collections;
using MySql.Data.MySqlClient;

public class Tracking : MonoBehaviour {


	MySqlConnection connection;

	Tracking() {
		var server = "localhost";
		var database = "test";
		var  uid = "root";
		var password = "12345";			var connectionString = "SERVER=" + server + ";" + "DATABASE=" + 
			database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

		connection = new MySqlConnection(connectionString);
	}

	//Constructor
	//Initialize values

	void Start () {
		Main ();
	}

	public static void Main()
	{
		var test = new Tracking();
		test.Insert();

	}

	private bool OpenConnection()
	{
		try
		{
			connection.Open();
			return true;
		}
		catch (MySqlException ex)
		{
			//When handling errors, you can your application's response based 
			//on the error number.
			//The two most common error numbers when connecting are as follows:
			//0: Cannot connect to server.
			//1045: Invalid user name and/or password.
			switch (ex.Number)
			{
			case 0:
				System.Console.WriteLine("Cannot connect to server.  Contact administrator");
				break;

			case 1045:
				System.Console.WriteLine("Invalid username/password, please try again");
				break;
			}
			return false;
		}
	}

	//Close connection
	private bool CloseConnection()
	{
		try
		{
			connection.Close();
			return true;
		}
		catch (MySqlException ex)
		{
			System.Console.WriteLine(ex.Message);
			return false;
		}
	}

	public void Insert()
	{
		string query = "INSERT INTO game (abc) VALUES('55')";

		//open connection
		if (this.OpenConnection() == true)
		{
			//create command and assign the query and connection from the constructor
			MySqlCommand cmd = new MySqlCommand(query, connection);

			//Execute command
			cmd.ExecuteNonQuery();

			//close connection
			this.CloseConnection();
		}
	}
}
