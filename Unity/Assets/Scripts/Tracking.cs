using UnityEngine;
using System.Collections;
using MySql.Data.MySqlClient;

public class Tracking : MonoBehaviour {

	const string DeviceUniqueIdentifierKey = "DeviceUniqueIdentifierKey";
	private MySqlConnection connection;

	public static Tracking instance;

	long SessionId = 1;

	public void Awake() {
		if (instance == null) {
			instance = this;
		} 
	}

	public void Start () {
		var server = "104.199.106.222";
		var database = "bsg_analytics";
		var uid = "root";
		var password = "1234";			
		var connectionString = "SERVER=" + server + ";" + "DATABASE=" + 
			database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

		connection = new MySqlConnection(connectionString);
		this.OpenConnection ();
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

	public void TrackDevice() {
		string deviceUniquieIdentifier = GetLocalMachineUUID ();
		Debug.Log ("Tracking device: " + deviceUniquieIdentifier);
		string query = string.Format("INSERT INTO devices (device_id, os) VALUES('{0}', '{1}') ON DUPLICATE KEY UPDATE os=os", 
			deviceUniquieIdentifier, SystemInfo.operatingSystem);

		ExecuteQuery (query);
	}

	public void StartSession() {
		Debug.Log ("Tracking session...");
		string query = string.Format("INSERT INTO sessions (device_id, start_time) VALUES('{0}', now())", 
			GetLocalMachineUUID());

		this.SessionId = ExecuteQuery (query);
		Debug.Log ("Session tracked: " + this.SessionId);
	}

	public void StopSession () {
		
	}

	public void TrackEvent(string eventType, string value)
	{
		Debug.Log ("Tracking event: " + eventType + " of value: " + value);
		string query = string.Format("INSERT INTO events (event_type_id, event_data, session_id, event_time) VALUES('{0}', '{1}', '{2}', now())", 
			eventType, value, SessionId);

		ExecuteQuery (query);
	}

	private long ExecuteQuery(string query) {
//		if (this.OpenConnection() != true) {
//			Debug.Log ("Unable to open connection and insert a record: ");
//		}

		MySqlCommand cmd = new MySqlCommand(query, connection);
		cmd.ExecuteNonQuery();
//		this.CloseConnection();
		return cmd.LastInsertedId;
	}
		
	private string GetLocalMachineUUID() {
		string deviceUniquieIdentifier = PlayerPrefs.GetString (DeviceUniqueIdentifierKey);
		if (string.IsNullOrEmpty(deviceUniquieIdentifier)) {
			deviceUniquieIdentifier = SystemInfo.deviceUniqueIdentifier;
			PlayerPrefs.SetString (DeviceUniqueIdentifierKey, deviceUniquieIdentifier);
		}
		return deviceUniquieIdentifier;
	}
}
