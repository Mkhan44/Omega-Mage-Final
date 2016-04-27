﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBug : PT_MonoBehaviour {
	public float speed = 0.5f;
	public float health = 10;

	public bool ___________;

	private float _maxHealth;
	public Vector3 walkTarget;
	public bool walking;
	public Transform characterTrans;
	//Stores dmg for each element each frame
	public Dictionary<ElementType,float> damageDict;
	// Dictionaries do not appear in Unity inspector.


	void Awake() {
		characterTrans = transform.Find ("CharacterTrans");
		_maxHealth = health; //Used to put a top cap on healing.
		ResetDamageDict ();
	}

	//Resets the values for the damageDict
	void ResetDamageDict() {
	if (damageDict == null) {
			damageDict = new Dictionary<ElementType, float>();
		}
		damageDict.Clear ();
		damageDict.Add (ElementType.earth , 0);
		damageDict.Add (ElementType.water, 0);
		damageDict.Add (ElementType.air, 0);
		damageDict.Add (ElementType.fire, 0);
		damageDict.Add (ElementType.aether, 0);
		damageDict.Add (ElementType.none, 0);
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
	 * NOTE: This same cod can be used to heal the instance.
	 */
	public void Damage(float amt,ElementType eT, bool damageOverTime=false) {
	//If it's DOT, then only damage the fractional amount for this frame.
		if (damageOverTime) {
			amt *= Time.deltaTime;
		}

		//Treat different damage types differently (most are default)
		switch (eT) {
		case ElementType.fire:
			//Only the max damage from one fire source affects this instance
			damageDict[eT] = Mathf.Max (amt,damageDict[eT]);
			break;

		case ElementType.air:
			//air doesn't damage EnemyBugs, so do nothing
			break;

		default:
			//By default, damage is added to the other damage by same element.
			damageDict[eT] += amt;
			break;
		}


	}

	//LateUpdate() is automatically called by Unity every frame. Once all the
	//Updates() on all instances have been called, then LateUpdate() is called
	//on all instances.

	void LateUpdate() {
	//Apply dmg from the diff element types.

		//Iteration through a Dictionary uses a KeyValuePair
		//entry.Key is the ElementType , while entry.Value is the float.
		float dmg = 0;
		foreach (KeyValuePair<ElementType,float> entry in damageDict) {
			dmg+= entry.Value;
		}

		health -= dmg;
		health = Mathf.Min (_maxHealth, health); //Limit health if healing.

		ResetDamageDict (); //Prepare for next frame.
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
