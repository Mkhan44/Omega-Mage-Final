using UnityEngine;
using System.Collections;

public class EnemyBug : PT_MonoBehaviour {
	public float speed = 0.5f;
	public float health = 10;

	public bool ___________;

	private float _maxHealth;
	public Vector3 walkTarget;
	public bool walking;
	public Transform characterTrans;


	void Awake() {
		characterTrans = transform.Find ("CharacterTrans");
		_maxHealth = health; //Used to put a top cap on healing.
	}
	
	// Update is called once per frame
	void Update () {
		WalkTo (Mage.S.pos);
	}

	//-------------------walking code--------------
	//All of this walking code is copied directly from Mage

	//Walk to a specific position. The position.z is always 0
	public void WalkTo(Vector3 xTarget) {
		walkTarget = xTarget; //Set the point to walk to.
		walkTarget.z = 0; //Force z= 0
		walking = true; //Now the EnemyBug is walking
		Face(walkTarget); //Look in the direction of the walkTarget.
	}

	public void Face(Vector3 poi) {
		Vector3 delta = poi - pos; //Find vector to the point of interest.
		//Use Atan2 to get the roation around z that points the x-axis of
		//EnemyBug:CharacterTrans towards poi
		float rZ = Mathf.Rad2Deg * Mathf.Atan2 (delta.y, delta.x);
		//Set the rotation of characterTrans (doesn't actually rotate enemy)
		characterTrans.rotation = Quaternion.Euler (0, 0, rZ);
	}

	public void StopWalking() {
		walking = false;
		rigidbody.velocity = Vector3.zero;
	}

	void FixedUpdate() {
	if (walking) {
			if ((walkTarget - pos).magnitude < speed * Time.fixedDeltaTime) {
				//If EnemyBug is very close to walkTarget, just stop there.
				pos = walkTarget;
				StopWalking ();
			} else {
				//Otherwise, move towards walkTarget.
				rigidbody.velocity = (walkTarget - pos).normalized * speed;
			}
		} else {
		//If not walking, velocity should be zero.
			rigidbody.velocity = Vector3.zero;
		}
	}

	/* Damage this instance. by default, the damage is instant, but it can also
	 * be treated as damage over time, where the amt value would be the amount
	 * of damage done every second.
	 * NOTE: This same cod ecan be used to heal the instance.
	 */
	public void Damage(float amt, bool damageOverTime = false) {
	//If it's DOT, then only damage the fractional amount for this frame.
		if (damageOverTime) {
			amt *= Time.deltaTime;
		}

		health -= amt;
		health = Mathf.Min (_maxHealth, health); //Limit health if healing.

		if (health <= 0) {
			Die();
		}
	}

	//Making Die() a seperate function allows us to add things later like
	//diff death animations, dropping something, etc.
	public void Die() {
		Destroy (gameObject);
	}
}
