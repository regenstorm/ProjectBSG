using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {

	/// <summary>
	/// List of available music themes.
	/// </summary>
	public enum MusicTheme {
		MainMenu,
		Battle,
		BattleLost,
		BattleWon,
		NoMusic
	}

	private MusicTheme currentTune = MusicTheme.NoMusic;

	public static MusicManager instance;
	private AudioSource audioSource;

	public AudioClip mainMenuMusic;
	public AudioClip battleMenuMusic;
	public AudioClip battleLostMusic;
	public AudioClip battleWonMusic;

	void Awake() {
		if (!instance) {
			instance = this;
		} else {
			Destroy (this.gameObject);
		}
		DontDestroyOnLoad(transform.gameObject);
	}

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
	}

	/// <summary>
	/// Stops the previous theme and starts the new one.
	/// </summary>
	public void Play(MusicTheme theme) {
		if (currentTune == theme) {
			return;
		}
		audioSource.Stop ();
		switch(theme) {
			case MusicTheme.MainMenu: 	this.audioSource.clip = mainMenuMusic; break;
			case MusicTheme.Battle: 	this.audioSource.clip = battleMenuMusic; break;
			case MusicTheme.BattleLost: this.audioSource.clip = battleLostMusic; break;
			case MusicTheme.BattleWon: 	this.audioSource.clip = battleWonMusic; break;
		}
		currentTune = theme;
		audioSource.Play ();
	}
			
	// Update is called once per frame
	void Update () {
	
	}
}
