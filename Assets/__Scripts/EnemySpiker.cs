﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpiker : PT_MonoBehaviour , Enemy {
	[SerializeField]
	private float _touchDamage = 1;
	public float touchDamage {
		get { return(_touchDamage); }
		set { _touchDamage = value; }
	}
	//The pos Property is already implemented in PT_Monobehaviour.
	public string typeString {
		get { return(roomXMLString); }
		set { roomXMLString = value; }
	}
	public float speed = 5f;
	public string roomXMLString = "{";

	public bool _____________;

	public Vector3 moveDir;
	public Transform characterTrans;

	void Awake() {
		characterTrans = transform.Find ("CharacterTrans");
	}

	// Use this for initialization
	void Start () {
		//Set the move direction based on the character in Rooms.xml
		switch (roomXMLString) {
		case"^":
			moveDir = Vector3.up;
			break;
		case "v":
			moveDir = Vector3.down;
			break;
		case"{":
			moveDir = Vector3.left;
			break;
		case"}":
				moveDir= Vector3.right;
			break;	
		}
	
	}

	void FixedUpdate() {
		rigidbody.velocity = moveDir * speed;
	}

	//This has the same structure as the dmg method in EnemyBug
	public void Damage(float amt, ElementType eT, bool damageOverTime = false) {
	//Nothing dmgs this enemy.
	}

	void OnTriggerEnter(Collider other) {
	//Check to see if a wall was hit.
		GameObject go = Utils.FindTaggedParent (other.gameObject);
		if (go == null)
			return; //In case nothing is tagged.

		if (go.tag == "Ground") {
		//Make sure that the ground tile is in the direction we're moving.
			//A dot product will help us with this (see the Useful concepts
			//Reference).
			float dot = Vector3.Dot (moveDir, go.transform.position - pos);
			if(dot > 0) {
				moveDir *= -1; //Reverse direction.
			}
		}
	}

}
