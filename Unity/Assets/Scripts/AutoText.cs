using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoText : MonoBehaviour {

	public float letterPause	;
	public AudioClip sound;
	public Text textField;
	private AudioSource audioSource;

	private string message;

	void Start () {
		audioSource = this.GetComponent<AudioSource> ();
		message = textField.text;
		textField.text = "";
		StartCoroutine (TypeText());
	}


	IEnumerator TypeText () {
		foreach (char letter in message.ToCharArray()) {
			textField.text += letter;
//			if (!audioSource.isPlaying) {
//				audioSource.Play ();
//			}
//			audioSource.Play ();
			yield return new WaitForSeconds (letterPause);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
