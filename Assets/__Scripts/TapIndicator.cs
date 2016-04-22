using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*Tap indicator makes use ot PT_Mover class from ProtoTools. this allows it to use
 * a bezier curve to alter....
 * 
 * You'll notice that this adds several public fields to inspector
 * */

public class TapIndicator : PT_Mover {

	public float lifeTime = 0.4f; //How long will it last?
	public float[] scales; //The scales it interpolates
	public Color[] colors; //The colors it interpolates.

	void Awake() {
		scale = Vector3.zero; //This initially hides the indicator.
	}

	void Start() {
		PT_Loc pLoc;
		List<PT_Loc> locs = new List<PT_Loc>();
		//The positions is always the same and always at z=-0.1f
		Vector3 tPos = pos;
		tPos.z = -0.1f;

		//You must have an equal number of scales and colors in the Inspector
		for (int i=0; i<scales.Length; i++) {
			pLoc = new PT_Loc();
			pLoc.scale = Vector3.one * scales[i]; //Each scale.
			pLoc.pos = tPos;
			pLoc.color = colors[i];

			locs.Add (pLoc);
		}

		//Callback is a function delegate that can call a void function when
		//the move is done.

		callback = CallbackMethod; //Call CallbackMethod() when finished.

		//Initialize the move by passing in a series of PT_Locs and duration for
		//The Bezier curve.
		PT_StartMove (locs, lifeTime);
	}

	void CallbackMethod() {
		Destroy (gameObject); //When the move is done, destroy(gameObject)
	}

}
