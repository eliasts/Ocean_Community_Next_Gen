using UnityEngine;
using System.Collections.Generic;

public class Boyancy : MonoBehaviour
{

	private Ocean ocean;

	private float mag = 1f;
	public float ypos = 0.0f;
	private List<Vector3> blobs;
	private List<float[]> prevBoya;

	//private bool engine = false;
	private List<float> sinkForces;

	[SerializeField] private float CenterOfMassOffset = -1f;
	[SerializeField] private float dampCoeff = .1f;
	//[SerializeField] private float dampCoeffIdle = .68f;

	//buoyancy slices. (Cannot be smaller then 2)
	//Raise these numbers if you want more accurate simulation. However it will add overhead. So keep it as small as possible.
	[SerializeField]private int SlicesX = 3;
	[SerializeField]private int SlicesZ = 3;

	[SerializeField] private bool sink = false;
	[SerializeField] private float sinkForce = 3;
	[SerializeField] private int interpolation = 3;

	private float iF; 
	private bool interpolate = false;

	private Rigidbody rrigidbody;

    void Start () {

		rrigidbody =  GetComponent<Rigidbody>();

		if(interpolation>0) {
			interpolate = true;
			iF = 1/(float)interpolation;
		}

		if(SlicesX<2) SlicesX=2;
		if(SlicesZ<2) SlicesZ=2;

        ocean = Ocean.Singleton;
		
		rrigidbody.centerOfMass = new Vector3 (0.0f, CenterOfMassOffset, 0.0f);
	
		Vector3 bounds = GetComponent<BoxCollider> ().size;

		float length = bounds.z;
		float width = bounds.x;

		blobs = new List<Vector3> ();
		prevBoya = new List<float[]>();

		int i = 0;
		float xstep = 1.0f / ((float)SlicesX - 1f);
		float ystep = 1.0f / ((float)SlicesZ - 1f);
	
		sinkForces = new List<float>();
		
		float totalSink = 0;

		for (int x=0; x<SlicesX; x++) {
			for (int y=0; y<SlicesX; y++) {		
				blobs.Add (new Vector3 ((-0.5f + x * xstep) * width, 0.0f, (-0.5f + y * ystep) * length) + Vector3.up * ypos);

				if(interpolate) {
					prevBoya.Add(new float[interpolation]);
				}
				
				float force =  Random.Range(0f,1f);
				
				force = force * force;
				
				totalSink += force;
				
				sinkForces.Add(force);
				i++;
			}		
		}
		
		// normalize the sink forces
		for (int j=0; j< sinkForces.Count; j++)
		{
			sinkForces[j] = sinkForces[j] / totalSink * sinkForce;
		}
		
	}

	private int tick;
	private bool llerp;

   void FixedUpdate() {

        if (ocean != null) {
			float coef = dampCoeff;

            int index = 0, k=0;

			for(int j = 0; j<blobs.Count; j++) {

                Vector3 wpos = transform.TransformPoint (blobs[j]);

                float buyancy = mag * (wpos.y);

				if (ocean.enabled) buyancy = mag * (wpos.y - ocean.GetWaterHeightAtLocation (wpos.x, wpos.z) );

			    if (sink) { buyancy = Mathf.Max(buyancy, -3) + sinkForces[index++]; }

				float damp = rrigidbody.GetPointVelocity (wpos).y;

				float bbuyancy = buyancy;

				//interpolate last (int interpolation) frames to smooth out the jerkiness
				if(interpolate) {
					prevBoya[k][tick] = buyancy;
					bbuyancy=0;
					for(int i=0; i<interpolation; i++) { bbuyancy += prevBoya[k][i]; }
					bbuyancy *= iF;
				}
				rrigidbody.AddForceAtPosition (-Vector3.up * (bbuyancy + coef * damp), wpos);
				k++;
		    }


			if(interpolate) { tick++; if(tick==interpolation) tick=0; }


        }
    }
	

	public void Sink(bool isActive)
	{
	    sink = isActive;	
	}


}
