//This scipt is useful if you don't want physics based buoyancy, but just to have obejct floating/touching the sea surface.

using UnityEngine;
using System.Collections;

public class Boyant : MonoBehaviour {

private Ocean ocean;

public float buoyancy ; //use this var to adjust the level the object floats, 0 is neutral
private bool hasChoppy;
private Vector3 oldPos;


	void Start () {
		ocean = Ocean.Singleton;
		if(ocean.choppy_scale>0) hasChoppy = true;
		oldPos = transform.position;
	}
	
	void Update () {
		if(ocean.canCheckBuoyancyNow[0]==1) {
			float off = 0;
			if(hasChoppy) off = ocean.GetChoppyAtLocation2(transform.position.x, transform.position.z);

			float targetY = ocean.GetWaterHeightAtLocation2(transform.position.x-off, transform.position.z) + buoyancy;
			transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
			oldPos = transform.position;
		} else {transform.position = oldPos;}
	}


}
