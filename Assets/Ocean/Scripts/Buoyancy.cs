using UnityEngine;
using System.Collections.Generic;

public class Buoyancy : MonoBehaviour {

	private Ocean ocean;
	public int renderQueue;

	public bool useFixedUpdate = false;
	public bool moreAccurate = false;
	public float magnitude = 2f;
	public float ypos = 0.0f;
	private List<Vector3> blobs;
	private List<float[]> prevBoya;

	//private bool engine = false;
	private List<float> sinkForces;

	public float CenterOfMassOffset = -1f;
	public float dampCoeff = .1f;

	//buoyancy slices. (Cannot be smaller then 2)
	//Raise these numbers if you want more accurate simulation. However it will add overhead. So keep it as small as possible.
	public int SlicesX = 2;
	public int SlicesZ = 2;

	public int interpolation = 3;
	private int intplt;
	public bool ChoppynessAffectsPosition = false;
	public float ChoppynessFactor = 0.2f;

	public bool WindAffectsPosition = false;
	public float WindFactor = 0.1f;

	public bool xAngleAddsSliding = false;
	public float slideFactor = 0.1f;

	public bool cvisible, wvisible, svisible;
	public Renderer _renderer ;

	public bool sink = false;
	public float sinkForce = 3;

	private float iF; 
	private bool interpolate = false;

	private Rigidbody rrigidbody;
	private int tick, tack;
	private Vector3 wpos, cpos;
	private bool useGravity;

	private float accel;
	private int prevAngleX, currAngleX;

	private float bbboyancy;
	private float prevBuoyancy;


    void Start () {

		if(!_renderer) {
			_renderer = GetComponent<Renderer>();
			if(!_renderer) {
				_renderer = GetComponentInChildren<Renderer>();
			}
		}

		if(_renderer && renderQueue>0) _renderer.material.renderQueue = renderQueue;

		if(!_renderer) {
			if(cvisible) { Debug.Log("Renderer to check visibility not assigned."); cvisible = false; }
			if(wvisible) { Debug.Log("Renderer to check visibility not assigned."); wvisible = false; }
			if(svisible) { Debug.Log("Renderer to check visibility not assigned."); svisible = false; }
		}

		if(dampCoeff<0) dampCoeff = Mathf.Abs(dampCoeff);

		rrigidbody =  GetComponent<Rigidbody>();

		useGravity = rrigidbody.useGravity;

		if(interpolation>0) {
			interpolate = true;
			iF = 1/(float)interpolation;
			intplt = interpolation;
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

				if(interpolate) { prevBoya.Add(new float[interpolation]); }
				
				float force =  Random.Range(0f,1f);
				force = force * force;
				totalSink += force;
				sinkForces.Add(force);
				i++;
			}		
		}
		
		// normalize the sink forces
		for (int j=0; j< sinkForces.Count; j++)	{
			sinkForces[j] = sinkForces[j] / totalSink * sinkForce;
		}

	}

	void Update() {
		if(!useFixedUpdate) update();
    }

	void FixedUpdate() {
		if(useFixedUpdate) update();
    }



	bool visible, lastvisible;
	int lastFrame=-15;

	void update() {

		if (ocean != null) {

			visible = _renderer.isVisible;

            //put object on the correct height of the sea surface when it has visibilty checks on and it became visible again.
            if (visible != lastvisible) {
                if (visible && !lastvisible) {
                    if (Time.frameCount - lastFrame > 15) {
                        //float off = ocean.GetChoppyAtLocation(transform.position.x, transform.position.z);
                        //float y = ocean.GetWaterHeightAtLocation2 (transform.position.x-off, transform.position.z);
                        float y = ocean.GetHeightChoppyAtLocation2(transform.position.x, transform.position.z);
                        transform.position = new Vector3(transform.position.x, y, transform.position.z);
                        lastFrame = Time.frameCount;
                    }
                }
                lastvisible = visible;
            }

            //prevent use of gravity when buoyancy is disabled
            if (cvisible) {
				if(useGravity) {
					if(!visible) {
							rrigidbody.useGravity=false;
							if(wvisible && svisible) return;
					} else {
							rrigidbody.useGravity = true;
						}
				}else {
					if(!visible) { if(wvisible && svisible) return;} 
				}
			}

			float coef = dampCoeff;
			int index = 0, k=0;

			int ran = (int)Random.Range(0, blobs.Count-1);


			for(int j = 0; j<blobs.Count; j++) {

				wpos = transform.TransformPoint (blobs[j]);
				//get a random blob to apply a force with the choppy waves
				if(ChoppynessAffectsPosition) { if(j == ran)  cpos = wpos; }

				if(!cvisible || visible) {
					float buyancy = magnitude * (wpos.y);

					if (ocean.enabled) {
						if(ocean.canCheckBuoyancyNow[0]==1) {
							float off = 0;
								if(ocean.choppy_scale>0) off = ocean.GetChoppyAtLocation(wpos.x, wpos.z);
							if(moreAccurate) {	
								buyancy = magnitude * (wpos.y - ocean.GetWaterHeightAtLocation2 (wpos.x-off, wpos.z));
							}else {
								buyancy = magnitude * (wpos.y - ocean.GetWaterHeightAtLocation (wpos.x-off, wpos.z));
								buyancy = Lerp(prevBuoyancy, buyancy, 0.5f);
								prevBuoyancy = buyancy;
							}
							bbboyancy = buyancy;
						} else {
							buyancy = bbboyancy;
						}
					}

					if (sink) { buyancy = System.Math.Max(buyancy, -3) + sinkForces[index++]; }

					float damp = rrigidbody.GetPointVelocity (wpos).y;

					float bbuyancy = buyancy;

					//interpolate last (int interpolation) frames to smooth out the jerkiness
					//interpolation will be used only if the renderer is visible
					if(interpolate) {
						if(visible) {
							prevBoya[k][tick] = buyancy;
							bbuyancy=0;
							for(int i=0; i<intplt; i++) { bbuyancy += prevBoya[k][i]; }
							bbuyancy *= iF;
						}
					}
					rrigidbody.AddForceAtPosition (-Vector3.up * (bbuyancy + coef * damp), wpos);
					k++;
				}
			}

			if(interpolate) { tick++; if(tick==intplt) tick=0; }

			tack++; if (tack == (int)Random.Range(2, 9) ) tack=0;
			if(tack>9) tack =1;

			//if the boat has high speed do not influence it (choppyness and wind)
			//if it has lower then fact then influence it depending on the speed .
			float fact = rrigidbody.velocity.magnitude * 0.02f;

			//this code is quick and dirty
			if(fact<1) {
				float fact2 = 1-fact;
				//if the object gets its position affected by the force of the choppy waves. Useful for smaller objects).
				if(ChoppynessAffectsPosition) {
					if(!cvisible || visible) {
						if(ocean.choppy_scale>0) {
							if(moreAccurate) {
								if(tack==0) rrigidbody.AddForceAtPosition (-Vector3.left * (ocean.GetChoppyAtLocation2Fast() * ChoppynessFactor*Random.Range(0.5f,1.3f))*fact2, cpos);
								else rrigidbody.AddForceAtPosition (-Vector3.left * (ocean.GetChoppyAtLocation2Fast() * ChoppynessFactor*Random.Range(0.5f,1.3f))*fact2, transform.position);
							} else {
								if(tack==0) rrigidbody.AddForceAtPosition (-Vector3.left * (ocean.GetChoppyAtLocationFast() * ChoppynessFactor*Random.Range(0.5f,1.3f))*fact2, cpos);
								else rrigidbody.AddForceAtPosition (-Vector3.left * (ocean.GetChoppyAtLocationFast() * ChoppynessFactor*Random.Range(0.5f,1.3f))*fact2, transform.position);
							}
						}
					}
				}
				//if the object gets its position affected by the wind. Useful for smaller objects).
				if(WindAffectsPosition) {
					if(!wvisible || visible) {
						if(tack==1) rrigidbody.AddForceAtPosition(new Vector3(ocean.pWindx, 0 , ocean.pWindy) * WindFactor*fact2, cpos);
						else rrigidbody.AddForceAtPosition(new Vector3(ocean.pWindx, 0 , ocean.pWindy) * WindFactor*fact2, transform.position);
					}
				}
			}

			//the object will slide down a steep wave
			//modify it to your own needs since it is a quick and dirty method.
			if(xAngleAddsSliding) {
				if(!svisible || visible) {
					float xangle = transform.localRotation.eulerAngles.x;
					currAngleX = (int)xangle;

					if(prevAngleX != currAngleX) {
						
						float fangle=0f;

						if(xangle>270 && xangle<355) {
							fangle = (360-xangle)*0.1f;
							accel -= fangle* slideFactor; if(accel<-20) accel=-20;
							}

						if(xangle>5 && xangle<90) {
							fangle = xangle*0.1f;
							accel += fangle* slideFactor;  if(accel>20) accel=20;
						}

						prevAngleX = currAngleX;
					}
				
					if((int)accel!=0) rrigidbody.AddRelativeForce (Vector3.forward * accel, ForceMode.Acceleration);
					if(accel>0) { accel-= 0.05f;	if(accel<0) accel=0; }
					if(accel<0) { accel+= 0.05f; if(accel>0) accel=0; }
				}
			}

		}
	}


	public void Sink(bool isActive)	{ sink = isActive; }

	static float Lerp (float from, float to, float value) {
		if (value < 0.0f) return from;
		else if (value > 1.0f) return to;
		return (to - from) * value + from;
	}

}
