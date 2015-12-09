using UnityEngine;
using System.Collections;

public class Boyant : MonoBehaviour {

private Ocean ocean;

public float buoyancy ; //use this var to adjust the level the object floats, 0 is neutral
public bool reactsToChoppiness ;
public bool printDebug ;
private Vector3 basePosition;

	// Use this for initialization
	void Start () {
    //used for choppy offset
    basePosition = transform.position;
   
	ocean = Ocean.Singleton;
		
	}
	
	// Update is called once per frame
	void Update () {
    //in the case we aren't reacting to choppiness let the position change the normal way
    if(!reactsToChoppiness){
        basePosition = transform.position;
    }
    //GameObject currentTile ;
    float choppyTranslation = 0.0f;
    float targetY = ocean.GetWaterHeightAtLocation(basePosition.x, basePosition.z) + buoyancy;
    float targetX = basePosition.x;
    if(reactsToChoppiness){
        choppyTranslation = ocean.GetChoppyAtLocation(basePosition.x, basePosition.z);
        targetX += choppyTranslation;
    }
    basePosition.y = targetY;
    transform.position = new Vector3(targetX , targetY, basePosition.z);


		if(printDebug){
		   // Debug.Log("Base Transform " + basePosition + " global space " + transform.position + " choppy translation " + choppyTranslation + " ");
		}		
	}

	void translateBy( Vector3 translation)
{
    basePosition += translation;
}
 
void OnDrawGizmos()
{
    if(printDebug){
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere( basePosition, 3 );  
   
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere( transform.position, 2 );
    }
}

}
