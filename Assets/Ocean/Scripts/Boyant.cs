//This scipt is useful if you don't want physics based buoyancy, but just to have obejct floating/touching the sea surface.

using UnityEngine;
using System.Collections;

public class Boyant : MonoBehaviour {

private Ocean ocean;

public float buoyancy ; //use this var to adjust the level the object floats, 0 is neutral
public bool reactsToChoppiness ;
public float chopinessfactor = 1f;
public bool showGizmos ;
private Vector3 basePosition;

	void Start () {
		//used for choppy offset
		basePosition = transform.position;
   
		ocean = Ocean.Singleton;
	}
	
	void Update () {
		if(ocean.canCheckBuoyancyNow) {
			//in the case we aren't reacting to choppiness let the position change the normal way
			if(!reactsToChoppiness){
				basePosition = transform.position;
			}

			float targetY = ocean.GetWaterHeightAtLocation(basePosition.x, basePosition.z) + buoyancy;
			float targetX = basePosition.x;

			if(reactsToChoppiness){
				targetX += ocean.GetChoppyAtLocation(basePosition.x, basePosition.z) * chopinessfactor;
			}

			basePosition.y = targetY;
			transform.position = new Vector3(targetX , targetY, basePosition.z);
		}
	}

	void translateBy( Vector3 translation){ basePosition += translation;  }

	void OnDrawGizmos() {
		if(showGizmos){
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere( basePosition, 3 );  
   
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere( transform.position, 2 );
		}
	}

}
