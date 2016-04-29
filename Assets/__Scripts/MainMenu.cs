using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public Button startText;

	// Use this for initialization
	void Start () {

		startText = startText.GetComponent<Button> ();
	
	}

	public void StartLevel()
	{
		Application.LoadLevel ("__OmegaMage_Scene_0");
	}
	

}
