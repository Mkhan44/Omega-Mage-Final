using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public Button startText;
	public Text infoText;

	// Use this for initialization
	void Start () {

		startText = startText.GetComponent<Button> ();

	
	}

	public void StartLevel()
	{
		startText.enabled = false;

		Application.LoadLevel ("__OmegaMage_Scene_0");
	}
	

}
