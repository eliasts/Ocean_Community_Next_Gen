//This scipt is useful if you don't want physics based buoyancy, but just to have obejct floating/touching the sea surface.

using UnityEngine;
using System.Collections;

public class Boyant : MonoBehaviour {

private Ocean ocean;

public float buoyancy ; //use this var to adjust the level the object floats, 0 is neutral
public bool reactsToChoppiness ;
public float chopinessfactor = 1f;
private Vector3 basePosition;
private Rigidbody rrigidbody;
public bool useRigidbody;


	void Start () {
		//used for choppy offset
		basePosition = transform.position;
		rrigidbody =  GetComponent<Rigidbody>();
		if(rrigidbody == null) useRigidbody = false;
   
		ocean = Ocean.Singleton;
	}
	
	void Update () {
		if(ocean.canCheckBuoyancyNow) {

			if(useRigidbody) {

				rrigidbody.AddForceAtPosition (-Vector3.left * (ocean.GetChoppyAtLocation(basePosition.x, basePosition.z) * chopinessfactor*Random.Range(0.1f,0.5f)), transform.position);
				float targetY = ocean.GetWaterHeightAtLocation2(transform.position.x, transform.position.z) + buoyancy;
				transform.position = new Vector3(transform.position.x , targetY, basePosition.z);

			} else {

				if(!reactsToChoppiness){ basePosition = transform.position; }
				float targetX = basePosition.x;

				if(reactsToChoppiness){
					targetX += ocean.GetChoppyAtLocation2(basePosition.x, basePosition.z) * chopinessfactor;
				}

				float targetY = ocean.GetWaterHeightAtLocation2(targetX, basePosition.z) + buoyancy;
				transform.position = new Vector3(targetX , targetY, basePosition.z);
			}
		}
	}


}
