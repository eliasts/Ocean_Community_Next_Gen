//comment this out if you dont want multithreading or having problems with it
#define THREADS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;



public class Ocean : MonoBehaviour {

	public string _name;//the name of the ocean preset
	private const float pix2 =  2.0f * Mathf.PI;
	public bool spreadAlongFrames = true;
	//values 3 or 4 are recommended when vsync is used!
	public int everyXframe = 5;
	public int fr1, fr2, fr3, fr4;
	private bool ticked = false;
	public float farLodOffset = 0;
	private float flodoff2, flodoff3;
	//the boats foam strength
	public float ifoamStrength = 18f;
	public bool shaderLod = true;
	public bool useShaderLods = false;
	public int numberLods = 1;

	public int defaultLOD = 4;

	public bool fixedTiles;
	public int fTilesDistance = 2;
	public int fTilesLod = 5;
	public int width = 32;
	public int height = 32;
	private int wh;
	private float hhalf;
	private float whalf;
	private int gwgh;
	private int offset;
	private float scaleA;

	public int renderTexWidth = 128;
	public int renderTexHeight = 128;

	public float scale = 0.1f;
	private float waveScale = 1f;
	public float speed = 0.7f;
	public float wakeDistance = 5f;
	public Vector3 size = new Vector3 (150.0f, 1.0f, 150.0f);
	private Bounds bounds;
	public int tiles = 2;

    public static Ocean Singleton { get; private set; }

    public float pWindx=10.0f;
	public float windx {
		get {
			return pWindx;
		}
		set {
			if (value!=pWindx) {
				this.pWindx=value;
				this.InitWaveGenerator();
			}
		}
	}

	public float pWindy=10.0f;
	public float windy {
		get {
			return pWindy;
		}
		set {
			if (value!=pWindy) {
				this.pWindy=value;
				this.InitWaveGenerator();
			}
		}
	}

	private int pNormal_scale=8;
	public int normal_scale {
		get {
			return pNormal_scale;
		}
		set {
			if (value!=pNormal_scale) {
				pNormal_scale=value;
				this.InitWaveGenerator();
			}
		}
	}
	private float pNormalStrength=2f;
	public float normalStrength {
		get {
			return pNormalStrength;
		}
		set {
			if (value!=pNormalStrength) {
				pNormalStrength=value;
			}
		}
	}
	public float choppy_scale = 2.0f;

	public Material material;
	public Material material1;
	public Material material2;

	public Material[] mat = new Material[3];

	public bool followMainCamera = true;
	private int max_LOD = 6;
	private ComplexF[] h0;
	private ComplexF[] t_x;
	private ComplexF[] n0;
	//private ComplexF[] n_x;
	//private ComplexF[] n_y;
	private ComplexF[] data;

	//private Color[] pixelData;
	private Vector3[] baseHeight;

	private Mesh baseMesh;
	private GameObject child;
	private List<List<Mesh>> tiles_LOD;
	private List<List<Mesh>> fTiles_LOD;

	private List<List<Renderer>> rtiles_LOD;

	private int g_height;
	private int g_width;
	private int n_width;
	private int n_height;
    private Vector2 sizeInv;
	
	//private bool normalDone = false;
	private bool reflectionRefractionEnabled = false;
	public float m_ClipPlaneOffset = 0.07f;
	private RenderTexture m_ReflectionTexture = null;
	private RenderTexture m_RefractionTexture = null;
	private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
	private Dictionary<Camera, Camera> m_RefractionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
	private int m_OldReflectionTextureSize = 0;
	private int m_OldRefractionTextureSize = 0;
	public LayerMask renderLayers = -1;

	private Vector3[] vertices;
	private Vector3[] normals;
	private Vector4[] tangents;
	public Transform player;
	public Transform sun;
	public Vector4 SunDir;

	private Vector4 oldSunDir;
	private Light sunLight;
	private Color oldSunColor;

	public float specularity = 0.185f;

	public float foamFactor = 1.1f;
	public Color surfaceColor = new Color (0.3f, 0.5f, 0.3f, 1.0f);
	public Color waterColor = new Color (0.3f, 0.4f, 0.3f);
	public Color fakeWaterColor = new Color (0.3f, 0.4f, 0.3f);

	public Shader oceanShader;
	public bool renderReflection = true;
	public bool renderRefraction = true;

	//Shader wave offset animation speed
	public float waveSpeed = 10f;
	private float prevOffsetValue = 0;
	private float nextOffsetValue = 0;
	private float prevOffsetTime = -100000;
	private const float offsetTimeFreq = 1f/ 60f;
	private int swich;

	//Humidity values
	public bool dynamicWaves;
	public static float wind;
	public float humidity;
	private float prevValue = 0.1f;
	private float nextValue = 0.4f;
	private float prevTime = 1;
	private const float timeFreq = 1f/ 280f;

	public GameObject mist;
	public GameObject mistLow;
	public GameObject mistClouds;
	public bool mistEnabled;
	//public bool waterInteractionEffects;



	private float GetHumidity(float time) {
		int intTime = (int)(time * timeFreq);
		int intPrevTime = (int)(prevTime * timeFreq);
		
		if (intTime != intPrevTime){
			prevValue = nextValue;
			nextValue = Random.value;
		}
		prevTime = time;
		float frac = time * timeFreq - intTime;
		
		return Mathf.SmoothStep(prevValue, nextValue, frac);
	}

	private float GetFloat() {
		float time = Time.time * waveSpeed;
		
		int intTime = (int)(time * offsetTimeFreq);
		int intPrevTime = (int)(prevOffsetTime * offsetTimeFreq);
		
		if (prevOffsetTime < 0){
			nextOffsetValue = -100;
		}
		
		if (intTime != intPrevTime){
			prevOffsetValue = nextOffsetValue;
			if(swich == 0){
				nextOffsetValue = 100;
				swich = 1;	
			}else if(swich == 1){
				nextOffsetValue = -100;
				swich = 0;
			}
		}
		prevOffsetTime = time;
		float frac = time * offsetTimeFreq - intTime;
		
		return Mathf.Lerp(prevOffsetValue, nextOffsetValue, frac);
	}

	IEnumerator AddMist () {
		while(true){
			if(player != null && mistEnabled){
				Vector3 pos = new Vector3(player.transform.position.x + Random.Range(-30, 30), player.transform.position.y + 5, player.transform.position.z + Random.Range(-30, 30));
				if(wind >= 0.7f){
                    if (mistClouds != null && mist != null)
                    {
                        GameObject mistParticles = Instantiate(mist, pos, new Quaternion(0, 0, 0, 0)) as GameObject;
                        mistParticles.transform.parent = transform;
                        GameObject clouds = Instantiate(mistClouds, pos, new Quaternion(0, 0, 0, 0)) as GameObject;
                        clouds.transform.parent = transform;
                    }
				}else if(wind > 0.4f){
                    if(mist != null)
                    {
					    GameObject mistParticles = Instantiate(mist, pos, new Quaternion(0,0,0,0)) as GameObject;
					    mistParticles.transform.parent = transform;
					    yield return new WaitForSeconds(0.5f);
                    }
                }
                else if(mistLow != null){
					GameObject mistParticles = Instantiate(mistLow, pos, new Quaternion(0,0,0,0)) as GameObject;
					mistParticles.transform.parent = transform;
					yield return new WaitForSeconds(1f);
				}
			}
			yield return new WaitForSeconds(0.5f);
			
		}
	}
	
	public void SetWaves (float wind) {
		waveScale = Mathf.Lerp(0, scale, wind);
    }
	
	public float GetWaterHeightAtLocation(float x, float y) {

        x = x / size.x;
        x = (x-Mathf.FloorToInt(x)) * width;
        y = y / size.z;
        y = (y-Mathf.FloorToInt(y)) * height;

        int index = (int)width * Mathf.FloorToInt(y) + Mathf.FloorToInt(x);

        return data[index].Re * waveScale / (width * height);
    }

	float GaussianRnd () {
		float x1 = Random.value;
		float x2 = Random.value;
	
		if (x1 == 0.0f) x1 = 0.01f;
	
		return (float)(System.Math.Sqrt (-2.0 * System.Math.Log (x1)) * System.Math.Cos (2.0 * Mathf.PI * x2));
	}

    // Phillips spectrum
	float P_spectrum (Vector2 vec_k, Vector2 wind) {
		float A = vec_k.x > 0.0f ? 1.0f : 0.05f; // Set wind to blow only in one direction - otherwise we get turmoiling water
	
		float L = wind.sqrMagnitude / 9.81f;
		float k2 = vec_k.sqrMagnitude;
		// Avoid division by zero
		if (vec_k.sqrMagnitude == 0.0f) {
			return 0.0f;
		}
		float vcsq=vec_k.magnitude;	
		return (float)(A * System.Math.Exp (-1.0f / (k2 * L * L) - System.Math.Pow (vcsq * 0.1, 2.0)) / (k2 * k2) * System.Math.Pow (Vector2.Dot (vec_k / vcsq, wind / wind.magnitude), 2.0));// * wind_x * wind_y;
	}

    void Awake() {
        Singleton = this;

		mat[0] = material;
		mat[1] = material1;
		mat[2] = material2;
    }


    void Start () {

		ticked = false;

		fr1 =0; fr2 = 1; fr3 = 2; fr4 = 3;

		if(everyXframe<4) fr4 = 2;

		sunLight = sun.GetComponent<Light>();

		bounds = new Bounds(new Vector3(size.x/2f,0f,size.z/2f),new Vector3(size.x+size.x*0.08f,0,size.z+size.z*0.08f));

        // normal map size
        n_width = 128;
		n_height = 128;

		wh = width*height;

		hhalf=height/2f;
		whalf=width/2f;
		
		//Avoid division every frame, so do it only once on start up
		sizeInv = new Vector2(1f / size.x,  1f / size.z);
		
		SetupOffscreenRendering ();

	
		//pixelData = new Color[n_width * n_height];

		// Init the water height matrix
		data = new ComplexF[width * height];

		// tangent
		t_x = new ComplexF[width * height];

		//n_x = new ComplexF[n_width * n_height];
		//n_y = new ComplexF[n_width * n_height];

		// Geometry size
		g_height = height + 1;	
		g_width = width + 1;

		gwgh = g_width*g_height-1;
		offset = g_width * (g_height - 1);

		scaleA = choppy_scale / wh;

		flodoff3 = farLodOffset/2;
		flodoff2 = farLodOffset/4;

		tiles_LOD = new List<List<Mesh>>();
		fTiles_LOD = new List<List<Mesh>>();

		rtiles_LOD = new List<List<Renderer>>();


		for (int L0D=0; L0D<max_LOD; L0D++) {
			tiles_LOD.Add (new List<Mesh>());
			rtiles_LOD.Add (new List<Renderer>());
		}

		for (int L0D=0; L0D<max_LOD; L0D++) {
			fTiles_LOD.Add (new List<Mesh>());
		}

		GameObject tile;

		int ntl = LayerMask.NameToLayer ("Water");

		int chDist; // Chebychev distance	

		for (int y=0; y<tiles; y++) {
			for (int x=0; x<tiles; x++) {
				chDist = System.Math.Max (System.Math.Abs (tiles / 2 - y), System.Math.Abs (tiles / 2 - x));
				chDist = chDist > 0 ? chDist - 1 : 0;
				float cy = y - Mathf.Floor(tiles * 0.5f);
				float cx = x - Mathf.Floor(tiles * 0.5f);
				tile = new GameObject ("WaterTile"+chDist.ToString());
                
                Vector3 pos=tile.transform.position;
				pos.x = cx * size.x;
				pos.y = transform.position.y;
				pos.z = cy * size.z;
				tile.transform.position=pos;
				tile.AddComponent (typeof(MeshFilter));
				tile.AddComponent <MeshRenderer>();
                Renderer renderer = tile.GetComponent<Renderer>();

				//shader/material lod (needs improvement)
				if(useShaderLods && numberLods>1) {
					if(numberLods==2) {
						if(chDist==0) renderer.material = material;
						if(chDist>1)  renderer.material = material1;
					}else if(numberLods==3){
						if(chDist==0 ) renderer.material = material;
						if(chDist==1)  renderer.material = material1;
						if(chDist>1)  renderer.material = material2;
					}
				} else {
					renderer.material = material;
				}

                
                //Make child of this object, so we don't clutter up the
                //scene hierarchy more than necessary.
                tile.transform.parent = transform;
			
				//Also we don't want these to be drawn while doing refraction/reflection passes,
				//so we'll add the to the water layer for easy filtering.
				tile.layer = ntl;
			
				// Determine which L0D the tile belongs
				if(fixedTiles){
					if(chDist < fTilesDistance) {
							tiles_LOD[chDist].Add((tile.GetComponent<MeshFilter>()).mesh);
							rtiles_LOD[chDist].Add( tile.GetComponent<Renderer>());
						}
				    else {
							fTiles_LOD[fTilesLod].Add((tile.GetComponent<MeshFilter>()).mesh);
						}
				}else{
					tiles_LOD[chDist].Add((tile.GetComponent<MeshFilter>()).mesh);
					rtiles_LOD[chDist].Add( tile.GetComponent<Renderer>());
				}
			}
		}

	
		// Init wave spectra. One for vertex offset and another for normal map
		h0 = new ComplexF[width * height];
		n0 = new ComplexF[n_width * n_height];
		
		InitWaveGenerator();
		UpdateWaterColor ();
		GenerateHeightmap ();
		StartCoroutine(AddMist());
		shader_LOD(!shaderLod, material, numberLods);
	}
	
	void InitWaveGenerator() {
		// Wind restricted to one direction, reduces calculations
		Vector2 windDirection = new Vector2 (windx, windy);

		// Initialize wave generator	
		for (int y=0; y<height; y++) {
			for (int x=0; x<width; x++) {
				float yc = y < height / 2f ? y : -height + y;
				float xc = x < width / 2f ? x : -width + x;
				Vector2 vec_k = new Vector2 (2.0f * Mathf.PI * xc / size.x, 2.0f * Mathf.PI * yc / size.z);
				h0 [width * y + x] = new ComplexF (GaussianRnd (), GaussianRnd ()) * 0.707f * (float)System.Math.Sqrt (P_spectrum (vec_k, windDirection));
			}
		}

		for (int y=0; y<n_height; y++) {
			for (int x=0; x<n_width; x++) {	
				float yc = y < n_height / 2f ? y : -n_height + y;
				float xc = x < n_width / 2f ? x : -n_width + x;
				Vector2 vec_k = new Vector2 (2.0f * Mathf.PI * xc / (size.x / normal_scale), 2.0f * Mathf.PI * yc / (size.z / normal_scale));
				n0 [n_width * y + x] = new ComplexF (GaussianRnd (), GaussianRnd ()) * 0.707f * (float)System.Math.Sqrt (P_spectrum (vec_k, windDirection));
			}
		}		
	}

	void GenerateHeightmap () {

		Mesh mesh = new Mesh ();

		int y = 0;
		int x = 0;

		// Build vertices and UVs
		Vector3 []vertices = new Vector3[g_height * g_width];
		Vector4 []tangents = new Vector4[g_height * g_width];
		Vector2 []uv = new Vector2[g_height * g_width];

		Vector2 uvScale = new Vector2 (1.0f / (g_width - 1f), 1.0f / (g_height - 1f));
		Vector3 sizeScale = new Vector3 (size.x / (g_width - 1f), size.y, size.z / (g_height - 1f));

		for (y=0; y<g_height; y++) {
			for (x=0; x<g_width; x++) {
				Vector3 vertex = new Vector3 (x, 0.0f, y);
				vertices [y * g_width + x] = Vector3.Scale (sizeScale, vertex);
				uv [y * g_width + x] = Vector2.Scale (new Vector2 (x, y), uvScale);
			}
		}
	
		mesh.vertices = vertices;
		mesh.uv = uv;

		for (y=0; y<g_height; y++) {
			for (x=0; x<g_width; x++) {
				tangents [y * g_width + x] = new Vector4 (1.0f, 0.0f, 0.0f, -1.0f);
			}
		}
		mesh.tangents = tangents;	
	
		for (int L0D=0; L0D<max_LOD; L0D++) {
			Vector3[] verticesLOD = new Vector3[(int)(height / System.Math.Pow (2, L0D) + 1) * (int)(width / System.Math.Pow (2, L0D) + 1)];
			Vector2[] uvLOD = new Vector2[(int)(height / System.Math.Pow (2, L0D) + 1) * (int)(width / System.Math.Pow (2, L0D) + 1)];
			int idx = 0;
 
			for (y=0; y<g_height; y+=(int)System.Math.Pow(2,L0D)) {
				for (x=0; x<g_width; x+=(int)System.Math.Pow(2,L0D)) {
					verticesLOD [idx] = vertices [g_width * y + x];

					//offset the far away flat lods so no gaps are visible in the horizon.
					if(L0D>=fTilesDistance) verticesLOD [idx].y += farLodOffset;

					uvLOD [idx++] = uv [g_width * y + x];
				}			
			}

			for (int k=0; k<tiles_LOD[L0D].Count; k++) {
				Mesh meshLOD = tiles_LOD [L0D][k];
				meshLOD.vertices = verticesLOD;
				meshLOD.uv = uvLOD;
				meshLOD.bounds = bounds; 
			}

			for (int k=0; k<fTiles_LOD[L0D].Count; k++) {
				Mesh meshLOD = fTiles_LOD [L0D][k];
				meshLOD.vertices = verticesLOD;
				meshLOD.uv = uvLOD;
			}
		}

		// Build triangle indices: 3 indices into vertex array for each triangle
		for (int L0D=0; L0D<max_LOD; L0D++) {
			int index = 0;
			int width_LOD = (int)(width / System.Math.Pow (2, L0D) + 1);
			int[] triangles = new int[(int)(height / System.Math.Pow (2, L0D) * width / System.Math.Pow (2, L0D)) * 6];
			for (y=0; y<(int)(height/System.Math.Pow(2,L0D)); y++) {
				for (x=0; x<(int)(width/System.Math.Pow(2,L0D)); x++) {
					// For each grid cell output two triangles
					triangles [index++] = (y * width_LOD) + x;
					triangles [index++] = ((y + 1) * width_LOD) + x;
					triangles [index++] = (y * width_LOD) + x + 1;

					triangles [index++] = ((y + 1) * width_LOD) + x;
					triangles [index++] = ((y + 1) * width_LOD) + x + 1;
					triangles [index++] = (y * width_LOD) + x + 1;
				}
			}
			for (int k=0; k<tiles_LOD[L0D].Count; k++) {
				Mesh meshLOD = tiles_LOD [L0D][k];
				meshLOD.triangles = triangles;
			}
			for (int k=0; k<fTiles_LOD[L0D].Count; k++) {
				Mesh meshLOD = fTiles_LOD [L0D][k];
				meshLOD.triangles = triangles;
			}
		}
	
		baseMesh = mesh;

		//Generate data for fixed tiles
		for (int k=0; k< fTiles_LOD[fTilesLod].Count; k++) {
			Mesh meshLOD = fTiles_LOD [fTilesLod][k];
			meshLOD.RecalculateNormals();
			meshLOD.tangents = Enumerable.Repeat(new Vector4(0.01f,0.01f,0.01f,0.01f), meshLOD.vertexCount).ToArray();
		}	
	}

	public void AssignFolowTarget(Transform tr) {
		player = tr;
	}

	void updateOceanMaterial() {
		if(material != null){
			if(sun != null){
		        SunDir = sun.transform.forward;
				if(SunDir != oldSunDir) {
					for(int i=0; i<3; i++) { if(mat[i] != null) mat[i].SetVector ("_SunDir", SunDir);  }
					oldSunDir = SunDir;
				   }
				if(sunLight.color != oldSunColor) {
					for(int i=0; i<3; i++) { if(mat[i] != null) mat[i].SetColor("_SunColor", sunLight.color);  }
					oldSunColor = sunLight.color; 
				}
			}

		}
	}


	private Vector3 centerOffset;
	private Thread th0, th1, th2, th3;
	private int lodSkip;
	
	void Update (){

		if(material != null){
			float getfloat = GetFloat();
			for(int i=0; i<3; i++) { if(mat[i] != null) mat[i].SetFloat( "_WaveOffset", getfloat );  }
		}

		#if THREADS
			if(spreadAlongFrames) updWithThreads(); else updNoThreads();
		#else
			updNoThreads();
		#endif
	}


	void updWithThreads() {

		int fint = Time.frameCount % everyXframe;

		if(fint == fr1 || !spreadAlongFrames) {

			updateOceanMaterial();

			float time=Time.time;

			th0 = new Thread( () => { 
				if(dynamicWaves) humidity = GetHumidity(time);
				wind = humidity;
				SetWaves(wind);
				
				for (int y = 0; y<height; y++) {
					for (int x = 0; x<width; x++) {
						int idx = width * y + x;
						float yc = y < hhalf ? y : -height + y;
						float xc = x < whalf ? x : -width + x;
				
						float vec_kx = pix2 * xc / size.x;
						float vec_ky = pix2  * yc / size.z;

						float sqrtMagnitude=(float)System.Math.Sqrt((vec_kx * vec_kx) + (vec_ky * vec_ky));
						float iwkt = (float)System.Math.Sqrt(9.81f * sqrtMagnitude)  * time * speed;

						ComplexF coeffA = new ComplexF ((float)System.Math.Cos(iwkt), (float)System.Math.Sin(iwkt));
						ComplexF coeffB;

						coeffB.Re = coeffA.Re;
						coeffB.Im = -coeffA.Im;

						int ny = y > 0 ? height - y : 0;
						int nx = x > 0 ? width - x : 0;

						data [idx] = h0 [idx] * coeffA + h0[width * ny + nx].GetConjugate() * coeffB;				
						t_x [idx] = data [idx] * new ComplexF (0.0f, vec_kx) - data [idx] * vec_ky;

						// Choppy wave calculations
						if (x + y > 0)
							data [idx] += data [idx] * vec_kx / sqrtMagnitude;
					}
				}
			} );
			th0.Start();
		}

		if(fint == fr2 || !spreadAlongFrames) {
			if(th0 != null) { while(th0.IsAlive) {} }
			th1 = new Thread( () => { 
				Fourier.FFT2 (data, width, height, FourierDirection.Backward);
				Fourier.FFT2 (t_x, width, height, FourierDirection.Backward);
			} );
			th1.Start();
		}


		if(fint == fr3 || !spreadAlongFrames) {
			if(th1 != null) { while(th1.IsAlive) {} }
			// Get base values for vertices and uv coordinates.
			if (baseHeight == null) {
				baseHeight = baseMesh.vertices;
				vertices = new Vector3[baseHeight.Length];
				normals = new Vector3[baseHeight.Length];
				tangents = new Vector4[baseHeight.Length];
			}
		
			th2 = new Thread( () => { 
	
				float scaleB = waveScale / wh;
				float scaleBinv = 1.0f / scaleB;

				for (int i=0; i<wh; i++) {
					int iw = i + i / width;
					vertices [iw] = baseHeight [iw];
					vertices [iw].x += data [i].Im * scaleA;
					vertices [iw].y = data [i].Re * scaleB;

					normals [iw] = Vector3.Normalize(new Vector3 (t_x [i].Re, scaleBinv, t_x [i].Im));
	
					if (((i + 1) % width)==0) {
						int iwi=iw+1;
						int iwidth=i+1-width;
						vertices [iwi] = baseHeight [iwi];
						vertices [iwi].x += data [iwidth].Im * scaleA;
						vertices [iwi].y = data [iwidth].Re * scaleB;

						normals [iwi] = Vector3.Normalize(new Vector3 (t_x [iwidth].Re, scaleBinv, t_x [iwidth].Im));
					}
				}


				for (int i=0; i<g_width; i++) {
					int io=i+offset;
					int mod=i % width;
					vertices [io] = baseHeight [io];
					vertices [io].x += data [mod].Im * scaleA;
					vertices [io].y = data [mod].Re * scaleB;
			
					normals [io] = Vector3.Normalize(new Vector3 (t_x [mod].Re, scaleBinv, t_x [mod].Im));
				}
	    
		
				for (int i=0; i<gwgh; i++) {
			
					//Need to preserve w in refraction/reflection mode
					if (!reflectionRefractionEnabled) {
						if (((i + 1) % g_width) == 0) {
							tangents [i] = Vector3.Normalize((vertices [i - width + 1] + new Vector3 (size.x, 0.0f, 0.0f) - vertices [i]));
						} else {
							tangents [i] = Vector3.Normalize((vertices [i + 1] - vertices [i]));
						}
			
						tangents [i].w = 1.0f;
					} else {
						Vector3 tmp;// = Vector3.zero;
			
						if (((i + 1) % g_width) == 0) {
							tmp = Vector3.Normalize(vertices[i - width + 1] + new Vector3 (size.x, 0.0f, 0.0f) - vertices [i]);
						} else {
							tmp = Vector3.Normalize(vertices [i + 1] - vertices [i]);
						}
				
						tangents [i] = new Vector4 (tmp.x, tmp.y, tmp.z, tangents [i].w);
					}
				}
			});
			th2.Start();
		}

		if(fint == fr4 || !spreadAlongFrames) {	

			if(th2 != null) { while(th2.IsAlive) {} }
			
			if (followMainCamera && player != null) {
				centerOffset.x = Mathf.Floor(player.position.x * sizeInv.x) *  size.x;
				centerOffset.z = Mathf.Floor(player.position.z * sizeInv.y) *  size.z;
				centerOffset.y = transform.position.y;
				if(transform.position != centerOffset) { ticked = true;  transform.position = centerOffset; }
			}		
		
			//Vector3 playerRelPos =  player.position - transform.position;

			//In reflection mode, use tangent w for foam strength
			if (reflectionRefractionEnabled) {
				float deltaTime = Time.deltaTime;
				Vector3 playerPosition =  player.position;
				Vector3 currentPosition = transform.position;

				th3 = new Thread( () => { 
					for (int y = 0; y < g_height; y++) {
						for (int x = 0; x < g_width; x++) {
							int item=x + g_width * y;
							if (x + 1 >= g_width) {
								tangents [item].w = tangents [g_width * y].w;
								continue;
							}
					
							if (y + 1 >= g_height) {
								tangents [item].w = tangents [x].w;
								continue;
							}
				
							float right = vertices[(x + 1) + g_width * y].x - vertices[item].x;
					
							float foam = right/(size.x / g_width);
					
					
							if (foam < 0.0f)
								tangents [item].w = 1f;
							else if (foam < 0.5f)
								tangents [item].w += 3.0f * deltaTime;
							else
								tangents [item].w -= 0.4f * deltaTime;
					
							if (player != null ){
								if(ifoamStrength>0) {
									Vector3 player2Vertex = (playerPosition - vertices[item] - currentPosition);
									// foam around boat
									if (player2Vertex.x >= size.x)
										player2Vertex.x -= size.x;
						
									if (player2Vertex.x<= -size.x)
										player2Vertex.x += size.x;
						
									if (player2Vertex.z >= size.z)
										player2Vertex.z -= size.z;
						
									if (player2Vertex.z<= -size.z)
										player2Vertex.z += size.z;
									player2Vertex.y = 0;
						
									if (player2Vertex.sqrMagnitude < wakeDistance * wakeDistance)
										tangents[item].w += ifoamStrength * deltaTime;
								}
							}
					
					
							tangents [item].w = Mathf.Clamp (tangents[item].w, 0.0f, 2.0f);
						}
					}
				});
				th3.Start();
				while(th3.IsAlive) {}
			}

			tangents [gwgh] = Vector4.Normalize(vertices [gwgh] + new Vector3 (size.x, 0.0f, 0.0f) - vertices [1]);

			lodSkip++;
			if(lodSkip == 2) lodSkip=0;

			for (int L0D=0; L0D<max_LOD; L0D++) {
				
				//this will skip one update of the tiles higher then Lod0
				if(L0D>0 && lodSkip==0 && !ticked) { break; }
				
				int den = (int)System.Math.Pow (2f, L0D);
				int itemcount = (int)((height / den + 1) * (width / den + 1));
		
				Vector4[] tangentsLOD = new Vector4[itemcount];
				Vector3[] verticesLOD = new Vector3[itemcount];
				Vector3[] normalsLOD = new Vector3[itemcount];
	
				int idx = 0;

				for (int y=0; y<g_height; y+=den) {
					for (int x=0; x<g_width; x+=den) {
						int idx2 = g_width * y + x;
						verticesLOD [idx] = vertices [idx2];
						//lower the far lods to eliminate gaps in the horizon when having big waves
						if(L0D>1) {
							if(farLodOffset!=0) {
								if(L0D==2) verticesLOD [idx].y += flodoff2;
								if(L0D==3) verticesLOD [idx].y += flodoff3;
								if(L0D>=4) verticesLOD [idx].y += farLodOffset;
							}
						}

						tangentsLOD [idx] = tangents [idx2];
						normalsLOD [idx++] = normals [idx2];
					}			
				}

				for (int k=0; k< tiles_LOD[L0D].Count; k++) {
					//update mesh only if visible
					if(!ticked) {
						if(rtiles_LOD[L0D][k].isVisible) {
							Mesh meshLOD = tiles_LOD [L0D][k];
							meshLOD.vertices = verticesLOD;
							meshLOD.normals = normalsLOD;
							meshLOD.tangents = tangentsLOD;
						}
					} else {
							Mesh meshLOD = tiles_LOD [L0D][k];
							meshLOD.vertices = verticesLOD;
							meshLOD.normals = normalsLOD;
							meshLOD.tangents = tangentsLOD;
					}
				}	
			}

			if(ticked) ticked = false;
		}
	}

	void updNoThreads() {

		updateOceanMaterial();

		int fint = Time.frameCount % everyXframe;

		if(fint == fr1 || !spreadAlongFrames) {
			float time=Time.time;
			if(dynamicWaves) humidity = GetHumidity(time);
			wind = humidity;
			SetWaves(wind);

			
			for (int y = 0; y<height; y++) {
				for (int x = 0; x<width; x++) {
					int idx = width * y + x;
					float yc = y < hhalf ? y : -height + y;
					float xc = x < whalf ? x : -width + x;
				
					float vec_kx = pix2 * xc / size.x;
					float vec_ky = pix2  * yc / size.z;

					float sqrtMagnitude=(float)System.Math.Sqrt((vec_kx * vec_kx) + (vec_ky * vec_ky));
					float iwkt = (float)System.Math.Sqrt(9.81f * sqrtMagnitude)  * time * speed;

					ComplexF coeffA = new ComplexF ((float)System.Math.Cos(iwkt), (float)System.Math.Sin(iwkt));
					ComplexF coeffB;

					coeffB.Re = coeffA.Re;
					coeffB.Im = -coeffA.Im;

					int ny = y > 0 ? height - y : 0;
					int nx = x > 0 ? width - x : 0;

					data [idx] = h0 [idx] * coeffA + h0[width * ny + nx].GetConjugate() * coeffB;				
					t_x [idx] = data [idx] * new ComplexF (0.0f, vec_kx) - data [idx] * vec_ky;

					// Choppy wave calculations
					if (x + y > 0)
						data [idx] += data [idx] * vec_kx / sqrtMagnitude;
				}
			}
		}

		if(fint == fr2 || !spreadAlongFrames) {
			Fourier.FFT2 (data, width, height, FourierDirection.Backward);
			Fourier.FFT2 (t_x, width, height, FourierDirection.Backward);
		}

		if(fint == fr3 || !spreadAlongFrames) {
			// Get base values for vertices and uv coordinates.
			if (baseHeight == null) {
				baseHeight = baseMesh.vertices;
				vertices = new Vector3[baseHeight.Length];
				normals = new Vector3[baseHeight.Length];
				tangents = new Vector4[baseHeight.Length];
			}
	
			float scaleB = waveScale / wh;
			float scaleBinv = 1.0f / scaleB;

			for (int i=0; i<wh; i++) {
				int iw = i + i / width;
				vertices [iw] = baseHeight [iw];
				vertices [iw].x += data [i].Im * scaleA;
				vertices [iw].y = data [i].Re * scaleB;

				normals [iw] = Vector3.Normalize(new Vector3 (t_x [i].Re, scaleBinv, t_x [i].Im));
	
				if (((i + 1) % width)==0) {
					int iwi=iw+1;
					int iwidth=i+1-width;
					vertices [iwi] = baseHeight [iwi];
					vertices [iwi].x += data [iwidth].Im * scaleA;
					vertices [iwi].y = data [iwidth].Re * scaleB;

					normals [iwi] = Vector3.Normalize(new Vector3 (t_x [iwidth].Re, scaleBinv, t_x [iwidth].Im));
				}
			}


			for (int i=0; i<g_width; i++) {
				int io=i+offset;
				int mod=i % width;
				vertices [io] = baseHeight [io];
				vertices [io].x += data [mod].Im * scaleA;
				vertices [io].y = data [mod].Re * scaleB;
			
				normals [io] = Vector3.Normalize(new Vector3 (t_x [mod].Re, scaleBinv, t_x [mod].Im));
			}
	    
		
			for (int i=0; i<gwgh; i++) {
			
				//Need to preserve w in refraction/reflection mode
				if (!reflectionRefractionEnabled) {
					if (((i + 1) % g_width) == 0) {
						tangents [i] = Vector3.Normalize((vertices [i - width + 1] + new Vector3 (size.x, 0.0f, 0.0f) - vertices [i]));
					} else {
						tangents [i] = Vector3.Normalize((vertices [i + 1] - vertices [i]));
					}
			
					tangents [i].w = 1.0f;
				} else {
					Vector3 tmp;// = Vector3.zero;
			
					if (((i + 1) % g_width) == 0) {
						tmp = Vector3.Normalize(vertices[i - width + 1] + new Vector3 (size.x, 0.0f, 0.0f) - vertices [i]);
					} else {
						tmp = Vector3.Normalize(vertices [i + 1] - vertices [i]);
					}
				
					tangents [i] = new Vector4 (tmp.x, tmp.y, tmp.z, tangents [i].w);
				}
			}

		}

		if(fint == fr4 || !spreadAlongFrames) {	
			
			if (followMainCamera && player != null) {
				centerOffset.x = Mathf.Floor(player.position.x * sizeInv.x) *  size.x;
				centerOffset.z = Mathf.Floor(player.position.z * sizeInv.y) *  size.z;
				centerOffset.y = transform.position.y;
				if(transform.position != centerOffset) { ticked = true;  transform.position = centerOffset; }
			}		
		
			//Vector3 playerRelPos =  player.position - transform.position;

			//In reflection mode, use tangent w for foam strength
			if (reflectionRefractionEnabled) {
				float deltaTime = Time.deltaTime;
				Vector3 playerPosition =  player.position;
				Vector3 currentPosition = transform.position;

				for (int y = 0; y < g_height; y++) {
					for (int x = 0; x < g_width; x++) {
						int item=x + g_width * y;
						if (x + 1 >= g_width) {
							tangents [item].w = tangents [g_width * y].w;
							continue;
						}
					
						if (y + 1 >= g_height) {
							tangents [item].w = tangents [x].w;
							continue;
						}
				
						float right = vertices[(x + 1) + g_width * y].x - vertices[item].x;
					
						float foam = right/(size.x / g_width);
					
					
						if (foam < 0.0f)
							tangents [item].w = 1f;
						else if (foam < 0.5f)
							tangents [item].w += 3.0f * deltaTime;
						else
							tangents [item].w -= 0.4f * deltaTime;
					
						if (player != null ) {
							if(ifoamStrength>0) {
								Vector3 player2Vertex = (playerPosition - vertices[item] - currentPosition);
								// foam around boat
								if (player2Vertex.x >= size.x)
									player2Vertex.x -= size.x;
						
								if (player2Vertex.x<= -size.x)
									player2Vertex.x += size.x;
						
								if (player2Vertex.z >= size.z)
									player2Vertex.z -= size.z;
						
								if (player2Vertex.z<= -size.z)
									player2Vertex.z += size.z;
								player2Vertex.y = 0;
						
								if (player2Vertex.sqrMagnitude < wakeDistance * wakeDistance)
									tangents[item].w += ifoamStrength * deltaTime;
							}
						}
					
						tangents [item].w = Mathf.Clamp (tangents[item].w, 0.0f, 2.0f);
					}
				}

			}

			tangents [gwgh] = Vector4.Normalize(vertices [gwgh] + new Vector3 (size.x, 0.0f, 0.0f) - vertices [1]);

			lodSkip++;
			if(lodSkip == 2) lodSkip=0;

			for (int L0D=0; L0D<max_LOD; L0D++) {
				
				//this will skip one update of the tiles higher then Lod0
				if(L0D>0 && lodSkip==0 && !ticked) { break; }

				int den = (int)System.Math.Pow (2f, L0D);
				int itemcount = (int)((height / den + 1) * (width / den + 1));
		
				Vector4[] tangentsLOD = new Vector4[itemcount];
				Vector3[] verticesLOD = new Vector3[itemcount];
				Vector3[] normalsLOD = new Vector3[itemcount];
	
				int idx = 0;

				for (int y=0; y<g_height; y+=den) {
					for (int x=0; x<g_width; x+=den) {
						int idx2 = g_width * y + x;
						verticesLOD [idx] = vertices [idx2];
						//lower the far lods to eliminate gaps in the horizon when having big waves
						if(L0D>1) {
							if(farLodOffset!=0) {
								if(L0D==2) verticesLOD [idx].y += flodoff2;
								if(L0D==3) verticesLOD [idx].y += flodoff3;
								if(L0D>=4) verticesLOD [idx].y += farLodOffset;
							}
						}

						tangentsLOD [idx] = tangents [idx2];
						normalsLOD [idx++] = normals [idx2];
					}			
				}
				for (int k=0; k< tiles_LOD[L0D].Count; k++) {
					//update mesh only if visible
					if(!ticked) {
						if(rtiles_LOD[L0D][k].isVisible) {
							Mesh meshLOD = tiles_LOD [L0D][k];
							meshLOD.vertices = verticesLOD;
							meshLOD.normals = normalsLOD;
							meshLOD.tangents = tangentsLOD;
						}
					} else {
							Mesh meshLOD = tiles_LOD [L0D][k];
							meshLOD.vertices = verticesLOD;
							meshLOD.normals = normalsLOD;
							meshLOD.tangents = tangentsLOD;
					}
				}	
			}
			if(ticked) ticked = false;
		}
	}

	/*
    Prepares the scene for offscreen rendering; spawns a camera we'll use for for
    temporary renderbuffers as well as the offscreen renderbuffers (one for
    reflection and one for refraction).
    */
	
	void SetupOffscreenRendering () {

		//set the uv scaling of normal and foam maps to 1/64 so they scale the same on all sizes!
		material.SetFloat ("_Size", 0.015625f);
		
		//Hack to make this object considered by the renderer - first make a plane
		//covering the watertiles so we get a decent bounding box, then
		//scale all the vertices to 0 to make it invisible.
		gameObject.AddComponent (typeof(MeshRenderer));
        //GetComponent<Renderer>().material.renderQueue = 1001;
        Renderer renderer = GetComponent<Renderer>();
        renderer.receiveShadows = false;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Mesh m = new Mesh ();
		
		Vector3[] verts = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		Vector3[] n = new Vector3[4];
		int[] tris = new int[6];
		
		float minSizeX = -1024;
		float maxSizeX = 1024;
		
		float minSizeY = -1024;
		float maxSizeY = 1024;
		
		verts [0] = new Vector3 (minSizeX, 0.0f, maxSizeY);
		verts [1] = new Vector3 (maxSizeX, 0.0f, maxSizeY);
		verts [2] = new Vector3 (maxSizeX, 0.0f, minSizeY);
		verts [3] = new Vector3 (minSizeX, 0.0f, minSizeY);
		
		tris [0] = 0;
		tris [1] = 1;
		tris [2] = 2;
		
		tris [3] = 2;
		tris [4] = 3;
		tris [5] = 0;
		
		m.vertices = verts;
		m.uv = uv;
		m.normals = n;
		m.triangles = tris;

		MeshFilter mfilter = gameObject.GetComponent<MeshFilter>();
		
		if (mfilter == null)
			mfilter = gameObject.AddComponent<MeshFilter>();
		
		mfilter.mesh = m;
		m.RecalculateBounds ();
		
		//Hopefully the bounds will not be recalculated automatically
		verts [0] = Vector3.zero;
		verts [1] = Vector3.zero;
		verts [2] = Vector3.zero;
		verts [3] = Vector3.zero;
		
		m.vertices = verts;
		
		reflectionRefractionEnabled = true;
	}
	
	/*
    Called when the object is about to be rendered. We render the refraction/reflection
    passes from here, since we only need to do it once per frame, not once per tile.
    */
	void OnWillRenderObject ()
	{
		if (this.renderReflection || this.renderRefraction)
			RenderReflectionAndRefraction ();
	}
	
	public void RenderReflectionAndRefraction()
	{
		int cullingMask = ~(1 << 4) & renderLayers.value;
		Camera cam = Camera.current;
		if( !cam )
			return;

		Camera reflectionCamera, refractionCamera;
		CreateWaterObjects( cam, out reflectionCamera, out refractionCamera );
		
		// find out the reflection plane: position and normal in world space
		Vector3 pos = transform.position;
		Vector3 normal = transform.up;

		UpdateCameraModes( cam, reflectionCamera );
		UpdateCameraModes( cam, refractionCamera );
		
		// Render reflection if needed
		if(this.renderReflection)
		{
			// Reflect camera around reflection plane
			float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
			Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
			
			Matrix4x4 reflection = Matrix4x4.zero;
			CalculateReflectionMatrix (ref reflection, reflectionPlane);
			Vector3 oldpos = cam.transform.position;
			Vector3 newpos = reflection.MultiplyPoint( oldpos );
			reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
			
			// Setup oblique projection matrix so that near plane is our reflection
			// plane. This way we clip everything below/above it for free.
			Vector4 clipPlane = CameraSpacePlane( reflectionCamera, pos, normal, 1.0f );
			reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
			
			reflectionCamera.cullingMask = cullingMask; // never render water layer
			reflectionCamera.targetTexture = m_ReflectionTexture;
            //reflectionCamera.gameObject.AddComponent<FogLayer>().fog = true;
            GL.invertCulling = true;
			reflectionCamera.transform.position = newpos;
			Vector3 euler = cam.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
			reflectionCamera.Render();
			reflectionCamera.transform.position = oldpos;
            GL.invertCulling = false;
            material.SetTexture( "_Reflection", m_ReflectionTexture );
		}
		
		// Render refraction
		if(this.renderRefraction)
		{
			refractionCamera.worldToCameraMatrix = cam.worldToCameraMatrix;
			
			// Setup oblique projection matrix so that near plane is our reflection
			// plane. This way we clip everything below/above it for free.
			Vector4 clipPlane = CameraSpacePlane( refractionCamera, pos, normal, -1.0f );
			refractionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
			
			refractionCamera.cullingMask = cullingMask; // never render water layer
			refractionCamera.targetTexture = m_RefractionTexture;
			//reflectionCamera.gameObject.AddComponent<FogLayer>().fog = true;
			refractionCamera.transform.position = cam.transform.position;
			refractionCamera.transform.rotation = cam.transform.rotation;
			refractionCamera.Render();
			material.SetTexture( "_Refraction", m_RefractionTexture );
		}
	}

	// Cleanup all the objects we possibly have created
	void OnDisable()
	{
		if( m_ReflectionTexture ) {
			DestroyImmediate( m_ReflectionTexture );
			m_ReflectionTexture = null;
		}
		if( m_RefractionTexture ) {
			DestroyImmediate( m_RefractionTexture );
			m_RefractionTexture = null;
		}
		foreach (KeyValuePair<Camera, Camera> kvp in m_ReflectionCameras)
			DestroyImmediate( (kvp.Value).gameObject );
		m_ReflectionCameras.Clear();
		foreach (KeyValuePair<Camera, Camera> kvp in m_RefractionCameras)
			DestroyImmediate( (kvp.Value).gameObject );
		m_RefractionCameras.Clear();
	}

	private void UpdateCameraModes( Camera src, Camera dest )
	{
		if( dest == null )
			return;
		// set water camera to clear the same way as current camera
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;		
		if( src.clearFlags == CameraClearFlags.Skybox )
		{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if( !sky || !sky.material )
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}
		// update other values to match current camera.
		// even if we are supplying custom camera&projection matrices,
		// some of values are used elsewhere (e.g. skybox uses far plane)
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}
	
	// On-demand create any objects we need for water
	private void CreateWaterObjects( Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera )
	{	
		reflectionCamera = null;
		refractionCamera = null;
		
		if(this.renderReflection)
		{
			// Reflection render texture
			if( !m_ReflectionTexture || m_OldReflectionTextureSize != renderTexWidth )
			{
				if( m_ReflectionTexture ) DestroyImmediate( m_ReflectionTexture );
				m_ReflectionTexture = new RenderTexture( renderTexWidth, renderTexHeight, 16 );
				m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
				m_ReflectionTexture.isPowerOfTwo = true;
				m_ReflectionTexture.hideFlags = HideFlags.DontSave;
				m_OldReflectionTextureSize = renderTexWidth;
			}
			
			// Camera for reflection
			m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
			if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
			{
				GameObject go = new GameObject( "Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox) );
				reflectionCamera = go.GetComponent<Camera>();
				reflectionCamera.enabled = false;
				reflectionCamera.transform.position = transform.position;
				reflectionCamera.transform.rotation = transform.rotation;
				reflectionCamera.gameObject.AddComponent<FlareLayer>();
				go.hideFlags = HideFlags.HideAndDontSave;
				m_ReflectionCameras[currentCamera] = reflectionCamera;
			}
		}
		
		if(this.renderRefraction)
		{
			// Refraction render texture
			if( !m_RefractionTexture || m_OldRefractionTextureSize != renderTexWidth )
			{
				if( m_RefractionTexture ) DestroyImmediate( m_RefractionTexture );
				m_RefractionTexture = new RenderTexture( renderTexWidth, renderTexHeight, 16 );
				m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
				m_RefractionTexture.isPowerOfTwo = true;
				m_RefractionTexture.hideFlags = HideFlags.DontSave;
				m_OldRefractionTextureSize = renderTexWidth;
			}
			
			// Camera for refraction
			m_RefractionCameras.TryGetValue(currentCamera, out refractionCamera);
			if (!refractionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
			{
				GameObject go = new GameObject( "Water Refr Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox) );
				refractionCamera = go.GetComponent<Camera>();
				refractionCamera.enabled = false;
				refractionCamera.transform.position = transform.position;
				refractionCamera.transform.rotation = transform.rotation;
				refractionCamera.gameObject.AddComponent<FlareLayer>();
				go.hideFlags = HideFlags.HideAndDontSave;
				m_RefractionCameras[currentCamera] = refractionCamera;
			}
		}
	}

	// Given position/normal of the plane, calculates plane in camera space.
	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}
	
	// Calculates reflection matrix around the given plane
	private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
		reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
		reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
		reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);
		
		reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
		reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
		reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
		reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
		
		reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
		reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
		reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
		reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);
		
		reflectionMat.m30 = 0F;
		reflectionMat.m31 = 0F;
		reflectionMat.m32 = 0F;
		reflectionMat.m33 = 1F;
	}

	void ReflectionQuality (string quality) {
        OnDisable();
		if(quality == "Low"){
			renderTexWidth = 128;
			renderTexHeight = 128;
		}else{
            renderTexWidth = 512;
			renderTexHeight = 512;
		}
	}
	
	public void shader_LOD(bool isActive, Material mat , int lod = 1) {
		if(!isActive){
			renderReflection = false;
			renderRefraction = false;
			mat.SetTexture ("_Reflection", null);
			mat.SetTexture ("_Refraction", null);
			oceanShader.maximumLOD = lod;
		}else{
			OnDisable(); 
			oceanShader.maximumLOD = defaultLOD;
			//disable refraction and reflection for shaderlods != 4
			if(defaultLOD<4) {
				renderReflection = false;
				renderRefraction = false;
				mat.SetTexture ("_Reflection", null);
				mat.SetTexture ("_Refraction", null);
			}
		}
    }

	public void matSetLod(Material mat, int lod) {
		if(mat != null) mat.shader.maximumLOD = lod;
	}

	public void EnableReflection(bool isActive) {
	    renderReflection = isActive;
		if(!isActive){
			material.SetTexture ("_Reflection", null);
		}else{
			OnDisable();
		}
    }

	public void EnableRefraction(bool isActive) {
	    renderRefraction = isActive;
		if(!isActive){
			material.SetTexture ("_Refraction", null);
		}else{
			OnDisable();
		}
    }

	public void UpdateWaterColor() {
		material.SetColor ("_WaterColor", waterColor);
		material.SetColor ("_SurfaceColor", surfaceColor);
	}

	void Mist (bool isActive) {
		mistEnabled = isActive;	
	}


	public void loadPreset(string preset, bool runtime = false) {
		
		using (BinaryReader br = new BinaryReader(File.Open(preset, FileMode.Open))){
			followMainCamera = br.ReadBoolean();
			ifoamStrength = br.ReadSingle();
			farLodOffset = br.ReadSingle();
			//these values cannot be updated at runtime!
			if(!runtime || !Application.isPlaying) {
				tiles = br.ReadInt32();
				size.x = br.ReadSingle();
				size.y = br.ReadSingle();
				size.z = br.ReadSingle();
				width = br.ReadInt32();
				height = br.ReadInt32();
				fixedTiles = br.ReadBoolean();
				fTilesDistance = br.ReadInt32();
				fTilesLod = br.ReadInt32();
			} else {
				br.ReadInt32(); br.ReadSingle(); br.ReadSingle(); br.ReadSingle();
				br.ReadInt32(); br.ReadInt32(); br.ReadBoolean();
				br.ReadInt32(); br.ReadInt32();
			}
			scale = br.ReadSingle();
			choppy_scale = br.ReadSingle();
			speed = br.ReadSingle();
			waveSpeed = br.ReadSingle();
			wakeDistance = br.ReadSingle();
			renderReflection = br.ReadBoolean();
			renderRefraction = br.ReadBoolean();
			renderTexWidth = br.ReadInt32();
			renderTexHeight = br.ReadInt32();
			m_ClipPlaneOffset = br.ReadSingle();
			renderLayers = br.ReadInt32();
			specularity = br.ReadSingle();
			mistEnabled = br.ReadBoolean();
			dynamicWaves = br.ReadBoolean();
			humidity = br.ReadSingle();
			pWindx = br.ReadSingle();
			pWindy = br.ReadSingle();
			waterColor.r = br.ReadSingle();
			waterColor.g = br.ReadSingle();
			waterColor.b = br.ReadSingle();
			waterColor.a = br.ReadSingle();
			surfaceColor.r = br.ReadSingle();
			surfaceColor.g = br.ReadSingle();
			surfaceColor.b = br.ReadSingle();
			surfaceColor.a = br.ReadSingle();
			foamFactor = br.ReadSingle();
			spreadAlongFrames = br.ReadBoolean();
			shaderLod = br.ReadBoolean();
			everyXframe = br.ReadInt32();
			useShaderLods = br.ReadBoolean();
			numberLods = br.ReadInt32();
			fakeWaterColor.r = br.ReadSingle();
			fakeWaterColor.g = br.ReadSingle();
			fakeWaterColor.b = br.ReadSingle();
			fakeWaterColor.a = br.ReadSingle();
			defaultLOD = br.ReadInt32();
		}
	}
}
