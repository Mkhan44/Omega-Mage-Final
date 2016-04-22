using UnityEngine;
using System.Collections;
using System.Collections.Generic; //Enables List<>s
using System.Linq; //Enables LINQ queries

//The MPhase enum is used to track the phase of mouse interaction
public enum MPhase {
	idle,
	down,
	drag
}

//The ElementType enum 
public enum ElementType {
earth,
	water,
	air,
	fire,
	aether,
	none
}

//MouseInfo stores information about hte mouse in each frame of interaction.
[System.Serializable]

public class MouseInfo {
	public Vector3 loc; //3D loc of the mouse near z=0.
	public Vector3 screenLoc; //Screen position of the mouse.
	public Ray ray; //Ray from the mouse into 3D space.
	public float time; //Time this mouseInfo was recorded.
	public RaycastHit hitInfo; //Info about what was hit by the ray.
	public bool hit; //Whether the mouse was over any collider.

	//These methods see if the mouseRay hits anything.
	public RaycastHit Raycast() {
		hit = Physics.Raycast (ray, out hitInfo);
		return(hitInfo);
	}

	public RaycastHit Raycast(int mask) {
		hit = Physics.Raycast (ray, out hitInfo, mask);
		return(hitInfo);
	}
}


//Mage is a subclass of PT_MonoBehaviour
public class Mage : PT_MonoBehaviour {
	static public Mage S;
	static public bool DEBUG = true;

	public float mTapTime = 0.1f; //How long is considered a tap.
	public GameObject tapIndicatorPrefab; //Prefab of the tap indicator.
	public float mDragDist = 5; //Min dist in pixels to be a drag.

	public float activeScreenWidth = 1; //%of the screen to use.

	public float speed = 2; //The speed at which _Mage walks.

	public GameObject[] elementPrefabs; //The Element_Sphere prefabs
	public float elementRotDist = 0.5f; //Radius of rotation
	public float elementRotSpeed = 0.5f; //Period of rotation
	public int maxNumSelectedElements = 1;
	public Color[] elementColors;

	//These set the min and max distance between two line points.
	public float lineMinDelta = 0.1f;
	public float lineMaxDelta = 0.5f;
	public float lineMaxLength = 8f;
	public GameObject fireGroundSpellPrefab;

	public bool _________________;

	public Transform spellAnchor; //The parent transform for all spells.

	public float totalLineLength;

	public List<Vector3> linePts; //Poitns to be shown in the line.
	protected LineRenderer liner; //Ref to the LineRenderer component.
	protected float lineZ = -0.1f; //Z depth of the line.
	//Protected variables are between public and private.
	//Protected variables can be seen by this class or any subclasses.
	//Only public variables appear in the inspector.



	public MPhase mPhase = MPhase.idle;
	public List<MouseInfo> mouseInfos = new List<MouseInfo>();
	public string actionStartTag; // ["Mage", "Ground", "Enemy"]

	public bool walking = false;
	public Vector3 walkTarget;
	public Transform characterTrans;

	public List<Element> selectedElements = new List<Element>();


	void Awake() {
		S = this; //Set the Mage singleton.
		mPhase = MPhase.idle;

		//Find the charactetTrans to rotate with Face()
		characterTrans = transform.Find ("CharacterTrans");

		//Get the LineRenderer component and disable it.
		liner = GetComponent<LineRenderer> ();
		liner.enabled = false;

		GameObject saGO = new GameObject ("Spell Anchor");
		//^Create and empty GameObject named "Spell Anchor". when you create
		//a new gameobject this way, it's at ...(pg 761)
		spellAnchor = saGO.transform; //Get its transform.
	}

	void Update() {
		//Find whether the mouse boutton 0 was pressed or released this frame.
		bool b0Down = Input.GetMouseButtonDown (0);
		bool b0Up = Input.GetMouseButtonUp (0);

		//Handle all input here (except for Inventory buttons)
		/*
		There are only a few possible actions:
		1. tap on ground to move to point
		2. drag on ground w/ no spell selected to move mage
		3. drag on ground w/spell to cast along ground
		4. tap on enemy to attack (or force-push away w/o element)
*/
		//An example of using < to return a bool value
		bool inActiveArea = (float)Input.mousePosition.x / Screen.width < activeScreenWidth;

		//This is handled as an if statement instead of switch because a tap 
		//can sometimes happen within a single frame.

		if (mPhase == MPhase.idle) {
			if(b0Down && inActiveArea) {
			mouseInfos.Clear (); //Clear the mouseInfos
			AddMouseInfo (); //And add a first MouseInfo.

			//If the mouse was clicked on something, it's a valid MouseDown
			if (mouseInfos [0].hit) {
				MouseDown ();
				mPhase = MPhase.down;
				}
			}
		}

	if(mPhase == MPhase.down) {
		AddMouseInfo(); //Add a MouseInfo for this frame.
		if(b0Up) {
			MouseTap(); //This was a tap.
			mPhase = MPhase.idle;
		} else if (Time.time - mouseInfos[0].time > mTapTime) {
		//If it's been down longer than a tap, this may be a drag, but to
			//be a drag, it must also have moved a certain number of
			//pixels across the screen.
			float dragDist = (lastMouseInfo.screenLoc - mouseInfos [0].screenLoc).magnitude;
			if(dragDist >= mDragDist) {
					mPhase = MPhase.drag;
			}

				//However, drag will immediately start after mTapTime if there
				//are no elements selected.
				if(selectedElements.Count == 0 ) {
					mPhase = MPhase.drag;
			}
		}
	}

	if(mPhase == MPhase.drag) {
		AddMouseInfo();
		if(b0Up) {
		//The mouse button was released
			MouseDragUp();
			mPhase = MPhase.idle;

		} else {
			MouseDrag(); //Still dragging.
			
		}

	}
		OrbitSelectedElements ();
	
}

		//Pulls info about the Mouse, add it to mouseInfos, and returns it
	MouseInfo AddMouseInfo() {
		MouseInfo mInfo = new MouseInfo ();
		mInfo.screenLoc = Input.mousePosition;
		mInfo.loc = Utils.mouseLoc; //Gets the position of the mosue at z=0
		mInfo.ray = Utils.mouseRay; //Gets the ray from the Main camera through
		//the mosue pointer.
		mInfo.time = Time.time;
		mInfo.Raycast (); //Default is to raycast with no mask.


		if (mouseInfos.Count == 0) {
			//If this is the first mouseInfo
			mouseInfos.Add (mInfo); //Add mInfo to mouseInfos
		} else {
			float lastTime = mouseInfos [mouseInfos.Count - 1].time;
			if (mInfo.time != lastTime) {
				//If time has passed since the last mouseInfo
				mouseInfos.Add (mInfo); //Add mInfo to MouseInfos
			}
			//This time test is necessary because AddMouseInfo() could be
			//called twice in one frame.
		}
		return(mInfo); //Return mInfo as well.
	}
	
public MouseInfo lastMouseInfo {
//Access to the latest MouseInfo
	get {
		if(mouseInfos.Count == 0) return(null);
		return(mouseInfos[mouseInfos.Count-1]);
		}
  	  }


	void MouseDown() {
	//The mouse was pressed on something (it could be a drag or tap)
		if (DEBUG)
			print ("Mage.MouseDown()");

		GameObject clickedGO = mouseInfos [0].hitInfo.collider.gameObject;
		// ^If the mouse wasn't clicked on anything, this would throw
		// an error because hitINfo would be null, however, we know that MouseDown()
		// is only called when the mouse WAS clicked on something, so
		//hitinfo is guaranteed to be defined.

		GameObject taggedParent = Utils.FindTaggedParent (clickedGO);
		if (taggedParent == null) {
			actionStartTag = "";
		} else {
			actionStartTag = taggedParent.tag;
			// ^ This should be either "ground", "Mage" or "Enemy"
		}
	}

	void MouseTap() {
	//Something was tapped like a button
		if(DEBUG) print ("Mage.MouseTap()");
		//Now this cares what was tapped
		switch(actionStartTag) {
		case "Mage":
			//Do nothing
			break;
		case "Ground":
			//Move to tapped point @ z = 0 whether or not an element is selected
			WalkTo(lastMouseInfo.loc); //Walk to the first mouseINfopos
			ShowTap (lastMouseInfo.loc); //Show where the player tapped
			break;
		}
	}

	void MouseDrag() {
	//The mouse is being drug across something
		if (DEBUG) print ("Mage.MouseDrag()");

		//Drag is meaningless unless the mouse started on the ground
		if(actionStartTag != "Ground") return;

		//If there is no element selected, the player should follow the mouse
		if (selectedElements.Count == 0) {
			//Continuously walk toward the current mouseInfo pos
			WalkTo (mouseInfos [mouseInfos.Count - 1].loc);
		} else {
		//This is a ground spell, so we need to draw a line.
			AddPointToLiner(mouseInfos[mouseInfos.Count-1].loc);
			// ^ add the most recent MouseInfo.loc to liner.
		}
	}

	void MouseDragUp() {
	//The mouse is released after being drug
		if (DEBUG)
			print ("Mage.MouseDragUp()");

		//Drag is meaningless unless the mouse started on the ground.
		if (actionStartTag != "Ground")
			return;

		//If there is no element selected, stop walking now.
		if (selectedElements.Count == 0) {
			//Stop walking when the drag is stopped
			StopWalking ();
		} else {
			CastGroundSpell();
			//Clear the liner
			ClearLiner();
		}
	}

	void CastGroundSpell() {
	//There is not a no-element ground spell, so return
		if (selectedElements.Count == 0)
			return;

		//Because this version of the prototype only allows a single
		//element to be selected, we can use that 0th element to pick the spell.
		switch (selectedElements [0].type) {
		case ElementType.fire:
			GameObject fireGO;
			foreach(Vector3 pt in linePts) {
				fireGO = Instantiate(fireGroundSpellPrefab) as GameObject;
				fireGO.transform.parent = spellAnchor;
				fireGO.transform.position = pt;
			}
			break;
			// TO do: ADD OTHER ELEMENTS HERE!!!392034208457!!!!234392841!
		}
		//Clear the selectedElements; they're consued by the spell.
		ClearElements ();
	}

	//Walk to a specific position. The position.z is always 0.
	public void WalkTo(Vector3 xTarget) {
		walkTarget = xTarget; //Set the point to walk to.
		walkTarget.z = 0; //Force z= 0
		walking = true; //Now the mage is walking.
		Face(walkTarget); //Look in the direction of the walkTarget.
	}

	public void Face(Vector3 poi) {
		Vector3 delta = poi-pos; //Find vector to the point of interest.
		//Use atan2 to get the rot around z that points the X-axis of 
		//Mage:charactertrans toward poi
		float rZ = Mathf.Rad2Deg * Mathf.Atan2 (delta.y, delta.x);
		//Set the rotation of characterTrans (doesn't actually rotate _Mage)
		characterTrans.rotation = Quaternion.Euler (0, 0, rZ);
	}

	public void StopWalking() {
	//Stops the _mage from walking
		walking = false;
		rigidbody.velocity = Vector3.zero;
	}

	void FixedUpdate () {
	if (walking) {
			if ((walkTarget - pos).magnitude < speed * Time.fixedDeltaTime) {
				//If mage is very close to walkTarget , just stop there.
				pos = walkTarget;
				StopWalking ();
			} else {
				//Otherwise, move toward walkTarget
				rigidbody.velocity = (walkTarget - pos).normalized * speed;
			}
		} else {
		 //If not walking, velocity should be zero.
			rigidbody.velocity = Vector3.zero;
		}
	}

	void OnCollisionEnter(Collision coll ) {
		GameObject otherGO = coll.gameObject;
		//Colliding with a wall can also stop walking.
		Tile ti = otherGO.GetComponent<Tile> ();
		if (ti != null) {
		if(ti.height > 0) {
			//Then this ti is a wall, and Mage should stop
				StopWalking ();
			}
		}
	}

	//Show where the player tapped.
	public void ShowTap(Vector3 loc ) {
		GameObject go = Instantiate (tapIndicatorPrefab)as GameObject;
		go.transform.position = loc;
	}

	//Choose an element_sphere of elType and adds it to selectedElements.
	public void SelectElement(ElementType elType) {
	if (elType == ElementType.none) {		//If it's the none elemeent...
			ClearElements();		//Then clear all elements...
			return;					//and return.
		}

		if (maxNumSelectedElements == 1) {
		//If only one gen can be selected, clear the existing one...
			ClearElements(); //so it can be replaced.
		}

		//Can't select more than maxNumSelectedElements simultaneously
		if (selectedElements.Count >= maxNumSelectedElements) return;

	//It's okay to add this element.
		GameObject go = Instantiate (elementPrefabs [(int)elType]) as GameObject;
		//^ Note the typecast from ElementType to int in the line above.
		Element el = go.GetComponent<Element> ();
		el.transform.parent = this.transform;

		selectedElements.Add (el); //Add el to the list of selectedElements.
  }

	//Clears all elements from selectedElements and destroys their GameObjects.
	public void ClearElements() {
	foreach (Element el in selectedElements) {
		//Destroy each GameObject in the list.
			Destroy (el.gameObject);
		}
		selectedElements.Clear (); //and clear the list
	}

	//Called every update() to orbit the elements around.
	void OrbitSelectedElements() {
	//If there are none selected, just return.
		if (selectedElements.Count == 0)
			return;

		Element el;
		Vector3 vec;
		float theta0, theta;
		float tau = Mathf.PI * 2; //tau is 369 degrees in radians 

		//Divide the cricle into the number of elements that are orbiting.
		float rotPerElement = tau / selectedElements.Count;

		//The base rotation angle(theta0) is set based on time
		theta0 = elementRotSpeed * Time.time * tau;

		for (int i = 0; i<selectedElements.Count; i++) {
		//Determine the rotation angle for each element.
			theta = theta0 + i*rotPerElement;
			el = selectedElements[i];
			//Use simple trig to turn the angle into a unit vector.
			vec = new Vector3(Mathf.Cos(theta),Mathf.Sin (theta),0);
			//Multiply that unit vector by the elementRotDist
			vec*= elementRotDist;
			//Raise the element to waist height.
			vec.z = -0.5f;
			el.lPos = vec; //Set the position of the Element_Sphere
		}
	}


	//-----------------LineRenderer Code ---------------------------//

	//Add a new point to the line. This ignores the point if it's too close to
	//existing ones and adds extra points if it's too far away.
	void AddPointToLiner(Vector3 pt) {
		pt.z = lineZ; //Set the z of the pt to lineZ to elevate it slightly
		//above the ground.
	
		//linePts.Add (pt);
		//UpdateLiner ();

		//Always add the point if linePts is empty...
		if (linePts.Count == 0) {
			linePts.Add (pt);
			totalLineLength = 0;
			return; //...but wait for a second point to enable the LineRenderer.
		}

		//If the line is too long already, return.
		if (totalLineLength > lineMaxLength)
			return;

		//If there is a previous point (pt0) , then find how far pt is from it.
		Vector3 pt0 = linePts[linePts.Count-1]; //Get the last point in linePts
		Vector3 dir = pt - pt0;
		float delta = dir.magnitude;
		dir.Normalize ();

		totalLineLength += delta;

		//If it's less than the min distance
		if (delta < lineMinDelta) {
		//...Then it's too close, don't add it.
			return;
		}

		//If it's further than the max distance then extra points...
		if (delta > lineMaxDelta) {
		//....Then add extra points in between.
			float numToAdd = Mathf.Ceil (delta/lineMaxDelta);
			float midDelta = delta/numToAdd;
			Vector3 ptMid;
			for(int i=1; i<numToAdd; i++) {
				ptMid = pt0+(dir*midDelta*i);
				linePts.Add (ptMid);
			}
		}
		linePts.Add (pt); //Add the point.
		UpdateLiner (); //Add finally update the line.
	}

	//Update the LineRenderer with the new points
	public void UpdateLiner() {
	//Get the type of the selectedElement
		int el =(int) selectedElements[0].type;

		//Set the line color based on that type.
		liner.SetColors(elementColors[el],elementColors[el]);

		//Update the representation of the ground spell about to be cast.
		liner.SetVertexCount(linePts.Count); //Set the number of vertices
		for(int i=0; i<linePts.Count; i++) {
			liner.SetPosition(i , linePts[i]); //Set each vertex.
		}
		liner.enabled = true; //Enable the line renderer.
	}

	public void ClearLiner() {
		liner.enabled = false; //Disable linerenderer
		linePts.Clear (); //And clear all linePts.
	}
}
  
