using UnityEngine;
using System.Collections;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System;
using System.Reflection;

public enum TrackingEventTypes {
	[Description("MainMenu")] MainMenu, 
	[Description("Credits")] Credits,
	[Description("MissionSelection")] MissionSelection,
	[Description("MissionIntro")] MissionIntro,
	[Description("BattleScreen")] BattleScreen
}

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

	private bool OpenConnection() {
		try {
			connection.Open();
			return true;
		}
		catch (MySqlException ex) {
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

	private bool CloseConnection() {
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

	public void TrackEvent(TrackingEventTypes eventType, string eventMessage)
	{
		Debug.Log ("Tracking event: " + eventType + ": " + eventMessage);
		string query = string.Format("INSERT INTO events (event_type_id, event_data, session_id, event_time) VALUES('{0}', '{1}', '{2}', now())", 
			GetEnumDescription(eventType), eventMessage, this.SessionId);

		ExecuteQuery (query);
	}

	private long ExecuteQuery(string query) {
		MySqlCommand cmd = new MySqlCommand(query, connection);
		cmd.ExecuteNonQuery();
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

	//It's clearly doesn't belong here, but fuck it
	private static string GetEnumDescription(Enum value)
	{
		FieldInfo fi = value.GetType().GetField(value.ToString());

		DescriptionAttribute[] attributes = 
			(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

		if (attributes != null && attributes.Length > 0)
			return attributes[0].Description;
		else
			return value.ToString();
	}
}
