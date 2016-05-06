using UnityEngine;
using System.Collections;

//Extends PT_MonoBehaviour
public class WaterGroundSpell : PT_MonoBehaviour {

	public float duration = 5; //Lifetime of the GameObject.
	public float durationVariance = 0.5f;
	// ^ This allows the duration to range from 3.5 to 4.5
	public float fadeTime = 1f; //Length of time to fade.
	public float timeStart; //Birth time of this GameObject.
	public float damagePerSecond = 0;


	void Start() {
		timeStart = Time.time;
		duration = Random.Range (duration - durationVariance, duration + durationVariance);
		// ^ Set the duration to a number between 3.5 and 4.5 (defaults)
	}

	void Update() {
		//Determine a number [0 - 1] (between 0 and 1) that stores the
		//% of duration that has passed.
		float u = (Time.time - timeStart) / duration;
		//At what u value should this start fading.
		float fadePercent = 1 - (fadeTime / duration);
		if (u > fadePercent) { 
			//...then sink into the ground
			float u2 = (u - fadePercent) / (1 - fadePercent);
			// ^u2 is a number (0,1) for just the fadeTime
			Vector3 loc = pos;
			loc.z = u2 * 2; //move lower over time
			pos = loc;
		}

		if (u > 1) {
			Destroy (gameObject); //...Destroy it.
		}
	}

	void OnTriggerEnter(Collider other) {
	//Announce when another object enters the collider
		GameObject go = Utils.FindTaggedParent (other.gameObject);
		if (go == null) {
			go = other.gameObject;
		}
		Utils.tr ("Water hit", go.name);

		EnemyBug recipient = other.GetComponent<EnemyBug> ();
		if (recipient != null) {
			recipient.speed = 0.1f;
		}
		EnemySpiker recipient2 = other.GetComponent<EnemySpiker> ();
		if (recipient2 != null) {
			recipient2.speed =  0.0f;
			recipient2.touchDamage = 0;
		}
	}


	void OnTriggerExit(Collider other) {
		//When enemy exits this field or the instance dissapears, movement speed goes back to normal.
		//Get a reference to the EnemyBug script component of the other
		EnemyBug recipient = other.GetComponent<EnemyBug> ();
			if (recipient != null) {
			recipient.speed = 0.5f;
		}
	
	  }
}
