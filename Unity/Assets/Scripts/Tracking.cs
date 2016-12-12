using UnityEngine;
using System.Collections;
using MySql.Data.MySqlClient;

public class Tracking : MonoBehaviour {

	const string DeviceUniqueIdentifierKey = "DeviceUniqueIdentifierKey";
	private MySqlConnection connection;

	public static Tracking instance;

	public void Awake() {
		if (instance == null) {
			instance = this;
		} 
	}

	void Start () {
		var server = "104.199.106.222";
		var database = "bsg_analytics";
		var uid = "root";
		var password = "1234";			
		var connectionString = "SERVER=" + server + ";" + "DATABASE=" + 
			database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

		connection = new MySqlConnection(connectionString);
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
			Debug.Log("Unable to open connection: " + ex);
			switch (ex.Number)
			{
			case 0:
				System.Console.WriteLine("Cannot connect to server. Contact administrator");
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
		try {
			connection.Close();
			return true;
		}
		catch (MySqlException ex) {
			System.Console.WriteLine(ex.Message);
			return false;
		}
	}

	public void TrackEvent(string eventType, string value)
	{
		Debug.Log ("Tracking event: " + eventType + " of value: " + value);
//		string query = "INSERT INTO game (abc) VALUES('55')";
//
//		if (this.OpenConnection() != true) {
//			Debug.Log ("Unable to insert a record: ");
//		}
//		
//		MySqlCommand cmd = new MySqlCommand(query, connection);
//		cmd.ExecuteNonQuery();
//		this.CloseConnection();
	}
//		
//	private string GetLocalMachineUUID() {
//		string deviceUniquieIdentifier = PlayerPrefs.GetString (DeviceUniqueIdentifierKey);
//		if (string.IsNullOrEmpty(deviceUniquieIdentifier)) {
//			deviceUniquieIdentifier = SystemInfo.deviceUniqueIdentifier;
//			PlayerPrefs.SetString (DeviceUniqueIdentifierKey, deviceUniquieIdentifier);
//		}
//		return deviceUniquieIdentifier;
//	}
}
