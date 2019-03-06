
// If you want to undefine NATIVE plugin support or SIMD or THREADS support, go to Project Settings->Player and remove the aprropriate define symbol(s).

#if !UNITY_STANDALONE
//(make sure to undefine simd if it was enabled by mistake for builds other then standalone)
#undef SIMD
#endif

#if (UNITY_WSA_8_1 ||  UNITY_WP_8_1 || UNITY_WINRT_8_1) && !UNITY_EDITOR
#undef THREADS
#endif

using UnityEngine;
#if UNITY_2017_OR_NEWER
using UnityEngine.XR;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if !UNITY_WEBGL && !UNITY_WEBPLAYER && !(UNITY_WSA_8_1 || UNITY_WP_8_1 || UNITY_WINRT_8_1) || UNITY_EDITOR
using System.IO;
#if THREADS
using System.Threading;
#endif
#endif


#if NETFX_CORE
#if UNITY_WSA_10_0
    using System.Threading.Tasks;
    using static System.IO.Directory;
    using static System.IO.File;
    using static System.IO.FileStream;
#endif
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE) && SIMD
using Mono.Simd;
#endif

[DisallowMultipleComponent]
[System.Serializable]
public class Ocean : MonoBehaviour {

	public int _gaussianMode;//mobile or desktop mode (todo)
	public int _gridMode = 0;//the mesh generation algorithm mode
	public int _gridRes = 0;// grid resolution mode
	public string _name;//the name of the ocean preset
	private const float TWO_PI =  2.0f * Mathf.PI;
	public bool spreadAlongFrames = true;
	//values 2, 3 or 4 are recommended when vsync is used!
	public int everyXframe = 5;
	//render reflection and refraction every x frame
	public int reflrefrXframe = 3;
	public int fr1, fr2, fr2B, fr3, fr4;
	private bool ticked = false, ticked2 = false;
	public float farLodOffset = 0;
	private float flodFact;
	private float[] flodoffset;
	private int ffact, oldffact;
	//the boat's foam strength
	public float ifoamStrength = 18f;
	//the boat's foam width
	public float ifoamWidth = 1f;
	public float foamDuration = 0.4f;
	public bool shaderLod = true;
	public bool useShaderLods = false;
	public int numberLods = 1, lowShaderLod = 0;
	public float foamUV = 2f;
	public float bumpUV = 1f;
	public bool loadSun = false;
	public bool fixedUpdate = false, forceDepth = false;
	public int lodSkipFrames = 1;
	private int lodSkip = 0;
	private bool skipLods;
	public static int hIndex;
	private bool isBoat;
	//if the buoyancy script is safe to check height location
	//tells other scripts that want to access water height data that it is safe to do now.
	//this applies only if the spread job over x frames is used!
	public byte[] canCheckBuoyancyNow;

	//the renderqueue of the tiles materials
	public int renderQueue = 2521;

	private Vector3 centerOffset;
#if !UNITY_WEBGL && !UNITY_WEBPLAYER && !(UNITY_WSA_8_1 || UNITY_WP_8_1 || UNITY_WINRT_8_1) && THREADS
#if !NETFX_CORE
	private Thread th0, th0b, th1,th1b, th2, th3, th3b;
#else
	private Task th0, th0b, th1,th1b, th2, th3, th3b;
#endif
	bool start;
#endif
	
	bool start2;

	public int defaultLOD = 5;

	public bool fixedTiles;
	public int discSize = 0;
	//public int fTilesDistance = 2;
	public int sTilesLod = 0;
	public int width = 32;
	public int height = 32;
	public int wh;
	public float wh1;
	float sizeQX, sizeQZ;
#if !NATIVE
	private float hhalf;
	private float whalf;
	private int offset;
	private float sizeXg_width;
#else
	float [] floats;
	Vector3[] vecs;
#endif
	private int gwgh;
	private float scaleA, oldScaleA;

	public int renderTexWidth = 256;
	public int renderTexHeight = 128;

	public float scale = 0.1f;
	public float waveScale = 1f;
	public float speed = 0.7f;
#if NATIVE
	private float oldSpeed;
#endif
	public float wakeDistance = 5f, waveDistanceFactor = 1f;
	private float oldWaveDistanceFactor = 1f;
	public Vector3 size = new Vector3 (150.0f, 1.0f, 150.0f);
	private Bounds bounds;
	public int tiles = 2;

	private bool previousFogState;
	private Color previousFogColor;
	private float previousFogDensity, previousFogNear, previousFogFar;
	private FogMode previousFogMode;

	//Fixed Gaussian Random tables to have predictable ocean Initialization.
	public float[] gaussRandom1;
	public float[] gaussRandom2;


    public static Ocean Singleton { get; private set; }

    public float pWindx=10.0f;
	private float oldWindx;
	public float windx {
		get {
			return pWindx;
		}
		set {
			if (value!=pWindx) {
				pWindx = value;
			}
		}
	}

	public float pWindy=10.0f;
	private float oldWindy;
	public float windy {
		get {
			return pWindy;
		}
		set {
			if (value!=pWindy) {
				pWindy = value;
			}
		}
	}

	public float choppy_scale = 2.0f;

	public Material material;
	public Material material1;
	public Material material2;

	public Material[] mat = new Material[3];

	private bool mat1HasRefl, mat1HasRefr, mat2HasRefl, mat2HasRefr;
	public bool hasShore=true, hasShore1=false;
	public bool hasFog=true, hasFog1=true, hasFog2=true, distCan1=true, distCan2=true;
	public float cancellationDistance = 2000f;

	public bool followMainCamera = true;
	//this is hardcoded 
	private int max_LOD = 6;

#if !NATIVE
	public ComplexF[] h0;
	public ComplexF[] h02;
#if THREADS
	private int gh2;
#endif
#endif
	private ComplexF[] t_x;
	private ComplexF[] data;

	private Vector3[] baseHeight;

	private Mesh baseMesh;
	private GameObject child;
	private List<Mesh> btiles_LOD;
	private List<List<Mesh>> tiles_LOD;

	private int g_height;
	private int g_width;

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
	public float specPower = 1f;
	public float reflectivity = 1f;
	public float translucency = 1f;
	public float shoreDistance = 4f;
	public float shoreStrength = 1.5f;

	public float foamFactor = 1.1f;
	public Color surfaceColor = new Color (0.3f, 0.5f, 0.3f, 1.0f);
	public Color waterColor = new Color (0.3f, 0.4f, 0.3f);
	public Color fakeWaterColor = new Color (0.3f, 0.4f, 0.3f);

	public Shader oceanShader;
	public bool renderReflection = true;
	public bool renderRefraction = true;
	
	//Alpha for shaders that support it
	public float shaderAlpha = 0.83f;

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

	//preallocated lod buffers
	private Vector3[][] verticesLOD;
	private Vector3[][] normalsLOD;
	private Vector4[][] tangentsLOD;

	private Vector3 mv2;

#if UNITY_EDITOR
		private SceneView oldSceneView;
		private Transform oldTransform;
		private string oldTransform2;
#endif

	//SIMD
#if SIMD
	private Vector4f v4 = new Vector4f();
#endif

    void Awake() {
        Singleton = this;
		//Application.targetFrameRate = 120;

		mat[0] = material;
		mat[1] = material1;
		mat[2] = material2;
    }


    void Start () {

		canCheckBuoyancyNow = new byte[1];

		//experiment with this value on low end mobiles (default: -1. Read the documentation about it.)
		//QualitySettings.maxQueuedFrames = 1;

#if UNITY_EDITOR
		oldSceneView = SceneView.currentDrawingSceneView;
		EditorApplication.update += _update;
		if(player) {
			oldTransform = player.transform;
			oldTransform2 = player.name;
		}
#endif

		

		Initialize();


		// *** Use this to load a preset at runtime *** (You must have your presets at a folder on Application.dataPath or Application.persistentDataPath etc.)
		//In this case we load a preset with a fixed gaussian random table to have the same ocean loaded every time (or on every machine in case of multiplayer)
#if !UNITY_EDITOR && UNITY_ANDROID
		//presetLoader.loadPreset(this, "jar:file://" + Application.dataPath + "!/assets/"+"OceanPresets/ocean1.preset", true, true);
#endif
	}





	public void Initialize(bool runtimeLoad = false) {
#if !UNITY_WEBGL && !UNITY_WEBPLAYER && !(UNITY_WSA_8_1 || UNITY_WP_8_1 || UNITY_WINRT_8_1) && THREADS
		start = false;
#endif
		ticked = false;  start2= false;

		previousFogState = RenderSettings.fog;
		previousFogColor = RenderSettings.fogColor;
		previousFogDensity = RenderSettings.fogDensity;
		previousFogMode = RenderSettings.fogMode;
		previousFogNear = RenderSettings.fogStartDistance;
		previousFogFar = RenderSettings.fogEndDistance;

		if(lodSkipFrames>0) skipLods = true; else skipLods = false;

		setSpread();

		if(sun) sunLight = sun.GetComponent<Light>();

		//if the player is a boat, allow interactive foam drawing
		if(player.GetComponent<Camera>()) isBoat = false; else isBoat = true;

		bounds = new Bounds(new Vector3(size.x / 2f, 0f, size.z / 2f),new Vector3(size.x + size.x * 0.15f, 0, size.z + size.z * 0.15f));

		wh = width * height;
		wh1 = 1f / (float)wh;
#if !NATIVE
		hhalf = height / 2f;
		whalf = width / 2f;
#else
		floats = new float[5];
		vecs = new Vector3[2];
#endif

		sizeQX = 1f / size.x;
		sizeQZ = 1f / size.z;

		//Avoid division every frame, so do it only once on start up
		sizeInv = new Vector2(1f / size.x,  1f / size.z);
		
		SetupOffscreenRendering (runtimeLoad);

		// Init the water height matrix
		data = new ComplexF[width * height];

		// tangent
		t_x = new ComplexF[width * height];

		// Geometry size
		g_height = height + 1;	
		g_width = width + 1;

		gwgh = g_width*g_height-1;
#if !NATIVE
		offset = g_width * (g_height - 1);
		sizeXg_width = size.x/g_width;
#if THREADS
		gh2 = g_height / 2;
#endif
#endif

		scaleA = choppy_scale / wh;
		oldScaleA = choppy_scale;
		oldWindx = pWindx;
		oldWindy = pWindy;
		oldWaveDistanceFactor = waveDistanceFactor;
#if NATIVE
		oldSpeed = speed;
#endif
		//factor to reduce lod offsets when the camera is high.
		flodFact = 1f;
		if(player) flodFact = 1.1f - Mathf.Clamp01((player.position.y)/1428f);
		ffact = MyFloorInt(flodFact * 10.5f);
		oldffact = ffact;

		/*if(_gridMode == 0)*/ GenerateTiles();


		// Init wave spectra. One for vertex offset and another for normal map
#if !NATIVE
		h0 = new ComplexF[width * height];
		h02 = new ComplexF[width * height];
		InitWaveGenerator();
#endif
		matSetVars ();
		/*if(_gridMode == 0)*/ GenerateHeightmap ();
		if(!runtimeLoad) StartCoroutine(AddMist());
		preallocateBuffers();

		mv2 = new Vector3 (size.x, 0.0f, 0.0f);


		//These must be called at start !!!
#if !NATIVE
			calcComplex(Time.time, 0, height);
			Fourier.FFT2 (data, width, height, FourierDirection.Backward);
			Fourier2.FFT2 (t_x, width, height, FourierDirection.Backward);
			/*if(_gridMode == 0) */calcPhase3();
#else
			//set the number of threads for native multithreaded functions (android and Linux).
			//for all the other platforms set this to >1 to have parallelization. If set to 1, no parallelization will be used.
			uocean.setThreads(2);
			if(SystemInfo.processorCount == 1) uocean.setThreads(1);
			//--------------------------------------------------------------------------------------------------------------------------------------------
			uocean.UoceanInit(width, height, pWindx, pWindy, speed, waveScale, choppy_scale, size.x, size.y, size.z, waveDistanceFactor);
			getGaussianTable();//optional: get the gaussian random table in case we want to save it
			uocean._calcComplex(data, t_x, Time.time, 0, height);
			uocean._fft1(data);
			uocean._fft2(t_x);
			uocean._calcPhase3(data, t_x, vertices, baseHeight, normals, tangents, reflectionRefractionEnabled, canCheckBuoyancyNow, waveScale);
#endif
		
		//place player boat on current water level.
		if(player!= null && isBoat) {
			player.position = new Vector3(player.position.x, GetWaterHeightAtLocation(player.position.x, player.position.z), player.position.z);
		} 		

		//force the main camera to draw depth if we want shore lines to show (Not needed for deferred rendering paths.)
		if(forceDepth) Camera.main.depthTextureMode = DepthTextureMode.Depth;

	}


	void preallocateBuffers() {
		// Get base values for vertices and uv coordinates.
		//if(_gridMode == 0) {
			if (baseHeight == null) {
				baseHeight = baseMesh.vertices;
				vertices = new Vector3[baseHeight.Length];
				normals = new Vector3[baseHeight.Length];
				tangents = new Vector4[baseHeight.Length];
			}

			//preallocate lod buffers to avoid garbage generation!
			verticesLOD = new Vector3[max_LOD][];
			normalsLOD  = new Vector3[max_LOD][];
			tangentsLOD = new Vector4[max_LOD][];

			for (int L0D=0; L0D<max_LOD; L0D++) {
				int den = (int)System.Math.Pow (2f, L0D);
				int itemcount = (int)((height / den + 1) * (width / den + 1));
				tangentsLOD[L0D] = new Vector4[itemcount];
				verticesLOD[L0D] = new Vector3[itemcount];
				normalsLOD[L0D]  = new Vector3[itemcount];
			}
		//}

	}	

	//how the frame/threads job will be distributed.
	public void setSpread() {
#if !NATIVE
			fr1 =0; fr2 = 0;  fr2B = 1; fr3 = 2; fr4 = 3;
			if(everyXframe == 4) {fr2 = 1; fr2B = 2; fr3 = 2; fr4 = 3;  }
			if(everyXframe == 3) {fr2=0; fr2B = 1; fr3 = 1; fr4 = 2;  }
			if(everyXframe == 2) { fr2 = 0; fr2B = 0; fr3 = 1; fr4 = 1;  }
#else
			fr1 =0; fr2 = 1;  fr2B = 2; fr3 = 2; fr4 = 3;
			if(everyXframe == 4) {fr2 = 1; fr2B = 2; fr3 = 2; fr4 = 3;  }
			if(everyXframe == 3) {fr2=0; fr2B = 1; fr3 = 1; fr4 = 2;  }
			if(everyXframe == 2) { fr2 = 0; fr2B = 0; fr3 = 1; fr4 = 1;  }
#endif
		if(fixedUpdate ) spreadAlongFrames = false;
	}




	void FixedUpdate (){
		if(fixedUpdate) updNoThreads();
	}


	void Update () {

		if(!fixedUpdate) {
#if THREADS && !UNITY_WEBGL && !UNITY_WEBPLAYER && !(UNITY_WSA_8_1 || UNITY_WP_8_1 || UNITY_WINRT_8_1)
				if(spreadAlongFrames) updWithThreads(); else updNoThreads();
#else
				updNoThreads();
#endif
		}

        //create reflection/refraction render textures (moved here, because OnWillRenderObject caused sometimes the reflection camera to stuck)
        OnWillRenderObject2();
    }
	


#if !UNITY_WEBGL && !UNITY_WEBPLAYER && !(UNITY_WSA_8_1 || UNITY_WP_8_1 || UNITY_WINRT_8_1) && THREADS
	void updWithThreads() {

		int fint = Time.frameCount % everyXframe;
		

		if(fint==0) start=true;


		if(start) {
			float time = Time.time;
			/*if(_gridMode == 0)*/ calculateCenterOffset();

			if(fint == fr1 || !spreadAlongFrames) {
				calcPhase4();

				if(fint == fr1) updateOceanMaterial();
				if(dynamicWaves) humidity = GetHumidity(time);
				wind = humidity;
				SetWaves(wind);
				
			}

			if(fint == fr2 || !spreadAlongFrames) {
				canCheckBuoyancyNow[0] = 0;
#if !NATIVE
#if !NETFX_CORE
						th0 = new Thread( () => { calcComplex(time, 0, height/2); }); th0.Start();
						th0b = new Thread( () => { calcComplex(time, height/2, height); }); th0b.Start();
#else
						th0 = new Task( () => { calcComplex(time, 0, height/2); }); th0.Start();
						th0b = new Task( () => { calcComplex(time, height/2, height); }); th0b.Start();
#endif
#else
					uocean._calcComplex(data, t_x, time, 0, height);
#endif
			}

			if(fint == fr2B || !spreadAlongFrames) {
#if !NATIVE
					
#if !NETFX_CORE
						if(th0 != null) { if(th0.IsAlive) th0.Join(); } if(th0b != null) { if(th0b.IsAlive) th0b.Join();}
						th1 = new Thread( () => { Fourier.FFT2 (data, width, height, FourierDirection.Backward); } );
#else
						if(th0 != null && th0b!=null) Task.WaitAll(th0,th0b);
						th1 = new Task( () => { Fourier.FFT2 (data, width, height, FourierDirection.Backward); } );
#endif
					th1.Start();
#else
					uocean._fft1(data);
#endif
					
			}
		
			if(fint == fr3 || !spreadAlongFrames) {
#if !NATIVE
#if !NETFX_CORE
						th1b = new Thread( () => { Fourier2.FFT2 (t_x, width, height, FourierDirection.Backward); } );
#else
						th1b = new Task( () => { Fourier2.FFT2 (t_x, width, height, FourierDirection.Backward); } );
#endif
					th1b.Start();
#else
					uocean._fft2(t_x);
#endif
					
			}

			if(fint == fr4 || !spreadAlongFrames ) {
#if !NATIVE
#if !NETFX_CORE
						if(th1b != null) {if(th1b.IsAlive) th1b.Join(); } if(th1 != null) {if(th1.IsAlive) th1.Join(); }
						th2 = new Thread( calcPhase3 ); th2.Start();
#else
						if(th1b != null && th1!=null) Task.WaitAll(th1b,th1);
						th2 = new Task( calcPhase3 ); th2.Start();
#endif
					
#else
					//2nd stage of tile update
					if (skipLods) updateTiles(1, max_LOD);
					uocean._calcPhase3(data, t_x, vertices, baseHeight, normals, tangents, reflectionRefractionEnabled, canCheckBuoyancyNow, waveScale);
#endif
			}

		}
	}
#endif


	void updNoThreads() {

		int fint = Time.frameCount % everyXframe;
		if(fint==0) start2=true;

		if(start2) {
			float time = Time.time;
			/*if(_gridMode == 0)*/ calculateCenterOffset();

			if(fint == fr1 || !spreadAlongFrames) {
				calcPhase4N();

				if(fint == fr1) updateOceanMaterial();

				if(dynamicWaves) humidity = GetHumidity(time);
				wind = humidity;
				SetWaves(wind);
			}

			if(fint == fr2 || !spreadAlongFrames) {
				canCheckBuoyancyNow[0] = 0;
#if !NATIVE
					calcComplex(time, 0, height);
#else
					uocean._calcComplex(data, t_x, time, 0, height);
#endif
			}

			if(fint == fr2B || !spreadAlongFrames) {
#if !NATIVE
					Fourier.FFT2 (data, width, height, FourierDirection.Backward);
#else
					uocean._fft1(data);
#endif
			}

			if(fint == fr3 || !spreadAlongFrames) {
#if !NATIVE
					Fourier.FFT2 (t_x, width, height, FourierDirection.Backward);
#else
					uocean._fft2(t_x);
#endif
			}

			if(fint == fr4 || !spreadAlongFrames) {
#if !NATIVE
					calcPhase3();
#else
					//2nd stage of tile update
					if (skipLods) updateTiles(1, max_LOD); 
					uocean._calcPhase3(data, t_x, vertices, baseHeight, normals, tangents, reflectionRefractionEnabled, canCheckBuoyancyNow, waveScale);
					canCheckBuoyancyNow[0] = 1;
#endif

			}
		}
	}

	void calculateCenterOffset() {
		if (followMainCamera && player) {
			centerOffset.x = MyFloorInt(player.position.x * sizeInv.x) *  size.x;
			centerOffset.z = MyFloorInt(player.position.z * sizeInv.y) *  size.z;
			centerOffset.y = transform.position.y;
			if(transform.position != centerOffset) {
				 ticked = true;
				 transform.position = centerOffset; 
				//make sure that the LOD0 tiles get updated immediately when the offset changes.
				updateTiles(0, 1); ticked2 = true;
			 }
			//calculate the height offset factor.
			if(player) {
				if(farLodOffset!=0) {
					flodFact = 1f - Mathf.Clamp01((player.position.y)*0.0007f);
					//if the camera raises, lower the far lods and update them.
					ffact = MyFloorInt(flodFact*10.5f);
					if(ffact != oldffact) { oldffact = ffact; ticked = true; updateTiles(1, max_LOD);  }
				 }
			 }
		}
	}

#if !NATIVE
	void calcComplex(float time, int ha, int hb) {
		ComplexF coeffA = new ComplexF(0,0);
		ComplexF tmp = new ComplexF(0,0);
		for (int y = ha; y<hb; y++) {
			for (int x = 0; x<width; x++) {
				int idx = width * y + x;
				float yc = y < hhalf ? y : -height + y;
				float xc = x < whalf ? x : -width + x;
				
				float vec_kx = TWO_PI * xc * sizeQX;
				float vec_ky = TWO_PI * yc * sizeQZ;

				float sqrtMagnitude = (float)System.Math.Sqrt((vec_kx * vec_kx) + (vec_ky * vec_ky));
				
				float iwkt = (float)System.Math.Sqrt(9.81f * sqrtMagnitude)  * time * speed;

				coeffA.Re = (float)System.Math.Cos(iwkt); coeffA.Im = (float)System.Math.Sin(iwkt);

				ComplexF coeffB;
				coeffB.Re = coeffA.Re; coeffB.Im = -coeffA.Im;

				int ny = y > 0 ? height - y : 0;
				int nx = x > 0 ? width - x : 0;

				data [idx] = h0 [idx] * coeffA + h0[width * ny + nx].GetConjugate() * coeffB;
				tmp.Im = vec_kx;
				t_x [idx] = data [idx] * tmp - data [idx] * vec_ky;

				// Choppy wave calculations
				if (x + y > 0)
					data [idx] += data [idx] * vec_kx / sqrtMagnitude;
			}
		}
	}

	void calcPhase3() {
		float scaleB = waveScale * wh1;
		float scaleBinv = 1.0f / scaleB;
#if !SIMD
		float magnitude = 1 , invm;
		float mag2 = scaleBinv*scaleBinv;
#else
		Vector4f factor;
#endif

		for (int i=0; i<wh; i++) {
			int iw = i + i / width;
			vertices [iw] = baseHeight [iw];
			vertices [iw].x += data [i].Im * scaleA;
			vertices [iw].y = data [i].Re * scaleB;

			normals[iw].x = t_x [i].Re;
			normals[iw].z = t_x [i].Im;
			normals[iw].y = scaleBinv;
#if !SIMD
				//normalize
				magnitude = (float)System.Math.Sqrt(normals[iw].x *normals[iw].x + mag2 + normals[iw].z * normals[iw].z);
				if(magnitude>0){ invm = 1f/magnitude; normals[iw].x *= invm; normals[iw].y *= invm; normals[iw].z *= invm; }
#else
				v4.X = normals[iw].x; v4.Y = scaleBinv; v4.Z = normals[iw].z;
				//Normalizef(ref v4);
				factor = v4 * v4; factor = factor.HorizontalAdd(factor); factor = factor.HorizontalAdd(factor); factor = factor.InvSqrt(); v4 *= factor;
				normals[iw].x = v4.X; normals[iw].y = v4.Y; normals[iw].z = v4.Z;
#endif

			if (((i + 1) % width)==0) {
				int iwi=iw+1;
				int iwidth=i+1-width;
				vertices [iwi] = baseHeight [iwi];
				vertices [iwi].x += data [iwidth].Im * scaleA;
				vertices [iwi].y = data [iwidth].Re * scaleB;

				normals[iwi].x = t_x [iwidth].Re;
				normals[iwi].z = t_x [iwidth].Im;
				normals[iwi].y = scaleBinv;
#if !SIMD
					//normalize
					magnitude = (float)System.Math.Sqrt(normals[iwi].x *normals[iwi].x + mag2 + normals[iwi].z * normals[iwi].z);
					if(magnitude>0){ invm = 1f/magnitude; normals[iwi].x *= invm; normals[iwi].y *= invm; normals[iwi].z *= invm; }
#else
					v4.X = normals[iwi].x; v4.Y = scaleBinv; v4.Z = normals[iwi].z;
					//Normalizef(ref v4);
					factor = v4 * v4; factor = factor.HorizontalAdd(factor); factor = factor.HorizontalAdd(factor); factor = factor.InvSqrt(); v4 *= factor;
					normals[iwi].x = v4.X; normals[iwi].y = v4.Y; normals[iwi].z = v4.Z;
#endif
			}
		}

		for (int i=0; i<g_width; i++) {
			int io=i+offset;
			int mod=i % width;
			vertices [io] = baseHeight [io];
			vertices [io].x += data [mod].Im * scaleA;
			vertices [io].y = data [mod].Re * scaleB;

			normals[io].x = t_x [mod].Re;
			normals[io].z = t_x [mod].Im;
			normals[io].y = scaleBinv;

#if !SIMD
				//normalize
				magnitude = (float)System.Math.Sqrt(normals[io].x *normals[io].x + mag2 + normals[io].z * normals[io].z);
				if(magnitude>0){ invm = 1f/magnitude; normals[io].x *= invm; normals[io].y *= invm; normals[io].z *= invm; }
#else
				v4.X = normals[io].x; v4.Y = scaleBinv; v4.Z = normals[io].z;
				//Normalizef(ref v4);
				factor = v4 * v4; factor = factor.HorizontalAdd(factor); factor = factor.HorizontalAdd(factor); factor = factor.InvSqrt(); v4 *= factor;
				normals[io].x = v4.X; normals[io].y = v4.Y; normals[io].z = v4.Z;
#endif
		}
	    
		canCheckBuoyancyNow[0] = 1;

		Vector3 tmp;

		for (int i=0; i<gwgh; i++) {	

			if (((i + 1) % g_width) == 0) {
				tmp = (vertices[i - width + 1] + mv2 - vertices [i]);
			} else {
				tmp = (vertices [i + 1] - vertices [i]);
			}
#if !SIMD
				magnitude = (float)System.Math.Sqrt(tmp.x *tmp. x + tmp.y * tmp.y + tmp.z * tmp.z);
				if(magnitude>0){ invm = 1f/magnitude; tmp.x *= invm; tmp.y *= invm; tmp.z *= invm; }
				tangents[i].x = tmp.x; tangents[i].y = tmp.y; tangents[i].z = tmp.z;
#else
				v4.X = tmp.x; v4.Y = tmp.y; v4.Z = tmp.z;
				//Normalizef(ref v4);
				factor = v4 * v4; factor = factor.HorizontalAdd(factor); factor = factor.HorizontalAdd(factor); factor = factor.InvSqrt(); v4 *= factor;
				tangents[i].x = v4.X; tangents[i].y = v4.Y; tangents[i].z = v4.Z;				
#endif

			//Need to preserve w in refraction/reflection mode
			if (!reflectionRefractionEnabled) tangents [i].w = 1.0f;
		}

	}

	void calcPhase4a(int a, int b, float deltaTime, Vector3 playerPosition, Vector3 currentPosition) {
		for (int y = a; y < b; y++) {
			for (int x = 0; x < g_width; x++) {
				int item=x + g_width * y;
				if (x + 1 >= g_width) {	tangents [item].w = tangents [g_width * y].w; continue;	}
				if (y + 1 >= g_height) { tangents [item].w = tangents [x].w; continue; }
				
				float right = vertices[(x + 1) + g_width * y].x - vertices[item].x;
				float foam = right/sizeXg_width;
					
				if (foam < 0.0f) tangents [item].w = 1f;
				else if (foam < 0.5f) tangents [item].w += 3.0f * deltaTime;
				else tangents [item].w -= foamDuration * deltaTime;
					
				if (isBoat) {
					if(ifoamStrength>0) {
						Vector3 player2Vertex = (playerPosition - vertices[item] - currentPosition) * ifoamWidth;
						// foam around boat
						if (player2Vertex.x >= size.x) player2Vertex.x -= size.x;
						if (player2Vertex.x<= -size.x) player2Vertex.x += size.x;
						if (player2Vertex.z >= size.z) player2Vertex.z -= size.z;
						if (player2Vertex.z<= -size.z) player2Vertex.z += size.z;
						player2Vertex.y = 0;
						if (player2Vertex.sqrMagnitude < wakeDistance * wakeDistance) tangents[item].w += ifoamStrength * deltaTime;
					}
				}
					
				tangents [item].w = Mathf.Clamp (tangents[item].w, 0.0f, 2.0f);
			}
		}
	}

#endif


#if !UNITY_WEBGL && !UNITY_WEBPLAYER && THREADS
	void calcPhase4() {
		//Vector3 playerRelPos =  player.position - transform.position;
#if SIMD
		Vector4f fac;
#endif

		//In reflection mode, use tangent w for foam strength
		if (reflectionRefractionEnabled) {
			float deltaTime = Time.deltaTime;  Vector3 playerPosition =  player.position;  Vector3 currentPosition = transform.position;
#if !NATIVE
#if !NETFX_CORE
					th3 = new Thread( () => { calcPhase4a(0, gh2, deltaTime, playerPosition, currentPosition);	});
#else
					th3 = new Task( () => { calcPhase4a(0, gh2, deltaTime, playerPosition, currentPosition);	});
#endif
				th3.Start();
#if !NETFX_CORE
					th3b = new Thread( () => { calcPhase4a(gh2, g_height, deltaTime, playerPosition, currentPosition);	});
#else
					th3b = new Task( () => { calcPhase4a(gh2, g_height, deltaTime, playerPosition, currentPosition);	});
#endif
				th3b.Start();
				//not needed
				//th3b.Join(); th3.Join();
#else
				floats[0] = deltaTime; floats[1] = 	ifoamStrength; floats[2] = wakeDistance; floats[3] = ifoamWidth; floats[4] = foamDuration;
				vecs[0] = playerPosition; vecs[1] = currentPosition;
				uocean._calcPhase4b(vertices, tangents, floats, isBoat, vecs);
#endif
		}

#if !NATIVE
#if !NETFX_CORE
				if(th2 != null) { if(th2.IsAlive) th2.Join();}
#else
				if(th2 != null) th2.Wait();
#endif
#endif

#if !SIMD
			tangents [gwgh] = Vector4.Normalize(vertices [gwgh] + mv2 - vertices [1]);
#else
			Vector3 t = vertices [gwgh] + mv2 - vertices [1];
			v4.X = t.x; v4.Y = t.y; v4.Z = t.z;
			fac = v4 * v4; fac = fac.HorizontalAdd(fac); fac = fac.HorizontalAdd(fac); fac = fac.InvSqrt(); v4 *= fac;
			tangents[gwgh].x = v4.X; tangents[gwgh].y = v4.Y; tangents[gwgh].z = v4.Z;
#endif
#if NATIVE
			if (skipLods) updateTiles(0, 1); else updateTiles(0, max_LOD);
#else
			updateTiles(0, max_LOD);
#endif
	}
#endif

	void calcPhase4N() {
		//Vector3 playerRelPos =  player.position - transform.position;
#if SIMD
		Vector4f fac;
#endif

		//In reflection mode, use tangent w for foam strength
		if (reflectionRefractionEnabled) {

#if !NATIVE
				calcPhase4a(0, g_height, Time.deltaTime, player.position, transform.position);
#else
				floats[0] = Time.deltaTime; floats[1] = ifoamStrength; floats[2] = wakeDistance; floats[3] = ifoamWidth; floats[4] = foamDuration;
				vecs[0] = player.position; vecs[1] = transform.position;
				uocean._calcPhase4b(vertices, tangents, floats, isBoat, vecs);
#endif
		}

#if !SIMD
			tangents [gwgh] = Vector4.Normalize(vertices [gwgh] + mv2 - vertices [1]);
#else
			Vector3 t = vertices [gwgh] + mv2 - vertices [1];
			v4.X = t.x; v4.Y = t.y; v4.Z = t.z;
			fac = v4 * v4; fac = fac.HorizontalAdd(fac); fac = fac.HorizontalAdd(fac); fac = fac.InvSqrt(); v4 *= fac;
			tangents[gwgh].x = v4.X; tangents[gwgh].y = v4.Y; tangents[gwgh].z = v4.Z;
#endif
		
#if NATIVE
			if (skipLods) updateTiles(0, 1); else updateTiles(0, max_LOD);
#else
			updateTiles(0, max_LOD);
#endif
	}



	//update the meshes with the final calculated mesh data
	void updateTiles(int a, int b) {

		if(skipLods) {
			lodSkip++;
			if(lodSkip >= lodSkipFrames+1) lodSkip=0;
		}

		for (int L0D=a; L0D<b; L0D++) {
			//if(L0D>
			//this will skip one update of the tiles higher then Lod0
			if(L0D>0 && lodSkip==0 && !ticked && skipLods) { break; }
			//this will skip one update of the LOD0 tiles because they got updated earlier when they should.
			if(ticked2 && L0D==0) { ticked2=false; continue; }

#if !NATIVE
				int den = MyIntPow (2, L0D);
				int idx = 0;

				for (int y=0; y<g_height; y+=den) {
					for (int x=0; x<g_width; x+=den) {
						int idx2 = g_width * y + x;
						verticesLOD[L0D] [idx] = vertices [idx2];
						//lower the far lods to eliminate gaps in the horizon when having big waves
						if(L0D>0) {
							if(farLodOffset!=0) {
								verticesLOD[L0D] [idx].y += flodoffset[L0D] * flodFact;
							}
						}
						tangentsLOD[L0D] [idx] = tangents [idx2];
						normalsLOD[L0D] [idx++] = normals [idx2];
					}			
				}
#else
				uocean._updateTilesA(verticesLOD[L0D], vertices, tangentsLOD[L0D], tangents, normalsLOD[L0D], normals, L0D, farLodOffset, flodoffset, flodFact);
#endif

			btiles_LOD[L0D].vertices = verticesLOD[L0D];
			btiles_LOD[L0D].normals = normalsLOD[L0D];
			btiles_LOD[L0D].tangents = tangentsLOD[L0D];
		}

		if(ticked) ticked = false;
	}
	
		
	void GenerateTiles() {

		int chDist, nmaxLod=0; // Chebychev distance
		
		for (int y=0; y<tiles; y++) {
			for (int x=0; x<tiles; x++) {
				chDist = System.Math.Max (System.Math.Abs (tiles / 2 - y), System.Math.Abs (tiles / 2 - x));
				chDist = chDist > 0 ? chDist - 1 : 0;
				if(nmaxLod<chDist) nmaxLod = chDist;
			}
		}
		max_LOD = nmaxLod+1;

		flodoffset = new float[max_LOD+1];
		float ffact = farLodOffset/max_LOD;
		for(int i=0; i<max_LOD+1; i++) {
			flodoffset[i] = i*ffact;
		}

		btiles_LOD = new List<Mesh>();
		tiles_LOD = new List<List<Mesh>>();

		for (int L0D=0; L0D<max_LOD; L0D++) {
            Mesh mh = new Mesh();
            #if UNITY_2017_3_OR_NEWER
            if(width>128) mh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            #endif
            btiles_LOD.Add(mh);
			tiles_LOD.Add (new List<Mesh>());
		}

		GameObject tile;

		int ntl = LayerMask.NameToLayer ("Water");

		for (int y=0; y<tiles; y++) {
			for (int x=0; x<tiles; x++) {
				chDist = System.Math.Max (System.Math.Abs (tiles / 2 - y), System.Math.Abs (tiles / 2 - x));
				chDist = chDist > 0 ? chDist - 1 : 0;
				if(nmaxLod<chDist) nmaxLod = chDist;
				float cy = y - Mathf.Floor(tiles * 0.5f);
				float cx = x - Mathf.Floor(tiles * 0.5f);
				tile = new GameObject ("Lod_"+chDist.ToString()+":"+y.ToString()+"x"+x.ToString());
                
                Vector3 pos=tile.transform.position;
				pos.x = cx * size.x;
				pos.y = transform.position.y;
				pos.z = cy * size.z;

				tile.transform.position=pos;
				tile.AddComponent <MeshFilter>();
				tile.AddComponent <MeshRenderer>();
                Renderer renderer = tile.GetComponent<Renderer>();

				tile.GetComponent<MeshFilter>().mesh = btiles_LOD[chDist];
				//tile.isStatic = true;

				//shader/material lod (needs improvement)
				if(useShaderLods && numberLods>1) {
					if(numberLods==2) {
						if(chDist <= sTilesLod) { if(material) renderer.material = material; }
						if(chDist > sTilesLod) { if(material1) renderer.material = material1; }
					}else if(numberLods==3){
						if(chDist <= sTilesLod ) { if(material) renderer.material = material; }
						if(chDist == sTilesLod+1) { if(material1) renderer.material = material1; }
						if(chDist > sTilesLod+1) { if(material2) renderer.material = material2; }
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

				tiles_LOD[chDist].Add( tile.GetComponent<MeshFilter>().mesh);
			} 
		}

		//enable/disable the fixed disc
		initDisc();
	}

	public void initDisc() {

		for(int i=1; i<4; i++) {
			Transform disct2 = transform.Find("sea_disc"+i.ToString());
			GameObject disc2 = null;
			if(disct2) { disc2 = disct2.gameObject; disc2.SetActive(false); }
		}
		if(!fixedTiles) return;
		string d = "sea_disc"+(discSize+1).ToString();

		Transform disct = transform.Find(d);
		GameObject disc = null;
		if(disct) disc = disct.gameObject;

		if(disc){
			disc.layer = LayerMask.NameToLayer ("Water");
			if(fixedTiles) {
				disc.SetActive(true);
				disct.parent=null;
				disc.transform.position = new Vector3(transform.position.x, transform.position.y + flodoffset[max_LOD]-1f, transform.position.z);
				if(tiles % 2 != 0) disc.transform.Translate(new Vector3(size.x/2, 0, size.x/2));
				float ff = size.x/512f;
				disc.transform.localScale = new Vector3(tiles * ff, 1f, tiles * ff);
				disct.parent = transform;
				Renderer renderer = disc.GetComponent<Renderer>();
				if(useShaderLods && numberLods>1) {
					if(numberLods==2) {
						if(material1) renderer.material = material1;
					}else if(numberLods==3){
						if(material2)  renderer.material = material2;
					}
				} else {
					renderer.material = material;
				}
			} else {
				disc.SetActive(false);
			}
		}
	}

	void GenerateHeightmap () {

		Mesh mesh = new Mesh ();
        #if UNITY_2017_3_OR_NEWER
        if (width > 128) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        #endif
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
		//Mesh meshLOD = null;

		for (int L0D=0; L0D<max_LOD; L0D++) {

			Vector3[] verticesLOD = new Vector3[(int)(height / System.Math.Pow (2, L0D) + 1) * (int)(width / System.Math.Pow (2, L0D) + 1)];
			Vector2[] uvLOD = new Vector2[(int)(height / System.Math.Pow (2, L0D) + 1) * (int)(width / System.Math.Pow (2, L0D) + 1)];
			int idx = 0;
 
			for (y=0; y<g_height; y+=(int)System.Math.Pow(2,L0D)) {
				for (x=0; x<g_width; x+=(int)System.Math.Pow(2,L0D)) {
					verticesLOD [idx] = vertices [g_width * y + x];
					uvLOD [idx++] = uv [g_width * y + x];
				}			
			}

			btiles_LOD[L0D].vertices = verticesLOD;;
			btiles_LOD[L0D].uv = uvLOD;
			btiles_LOD[L0D].bounds = bounds;
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

			btiles_LOD[L0D].triangles = triangles;

		}
	
		baseMesh = mesh;
		vertices = null; tangents = null; uv = null;
	}

	//Call AssignFolowTarget(Camera.main.transform); so the ocean follows the main Camera.
	public void AssignFolowTarget(Transform tr) {
		player = tr;
		//if the player is a boat, allow interactive foam drawing
		if(player.GetComponent<Camera>()) isBoat = false; else isBoat = true;
	}

	void updateOceanMaterial() {
		if(material != null){
			if(sun != null){
		        SunDir = sun.transform.forward;
				if(SunDir != oldSunDir) {
					if(useShaderLods) { for(int i=0; i<numberLods; i++) { if(mat[i] != null) mat[i].SetVector ("_SunDir", SunDir); } } else { if(material != null) material.SetVector ("_SunDir", SunDir);}
					oldSunDir = SunDir;
				   }
				if(sunLight.color != oldSunColor) {
					if(useShaderLods) { for(int i=0; i<numberLods; i++) { if(mat[i] != null) mat[i].SetColor("_SunColor", sunLight.color); } } else { if(material != null) material.SetColor("_SunColor", sunLight.color); }
					oldSunColor = sunLight.color; 
				}
			}

		}

		bool fchange = false;
#if NATIVE
		bool gchange = false;
		if(oldSpeed != speed) {oldSpeed = speed; gchange = true; }
#endif

		if(oldScaleA != choppy_scale) {
			scaleA = choppy_scale * wh1; oldScaleA = choppy_scale;
#if NATIVE
			gchange = true;
#endif
		}

#if NATIVE
		if(gchange) { uocean.updVars( pWindx, pWindy, speed, waveScale, choppy_scale,waveDistanceFactor, false); }
#endif

		if(oldWindx != pWindx) { oldWindx = pWindx; fchange = true; }
		if(oldWindy != pWindy) { oldWindy = pWindy; fchange = true; }
		if(oldWaveDistanceFactor != waveDistanceFactor) { oldWaveDistanceFactor = waveDistanceFactor; fchange = true; }

		//***TODO*** make these changes affect the ocean smoothly and not rebuild the h0 buffer
		if(fchange) {
#if !NATIVE
				InitWaveGenerator(true);
#else
				uocean.updVars( pWindx, pWindy, speed, waveScale, choppy_scale, waveDistanceFactor, true);
#endif
		}
	}

	public void matSetVars() {
		material.shader = oceanShader;

		for(int i=0; i<3; i++) {
			if(mat[i]!= null) {
				mat[i].SetColor("_WaterColor", waterColor); 
				mat[i].SetColor("_SurfaceColor", surfaceColor);
				mat[i].SetColor("_FakeUnderwaterColor", fakeWaterColor); 
				mat[i].SetFloat("_FoamFactor", foamFactor);
				mat[i].SetFloat ("_FoamSize", foamUV);
				mat[i].SetFloat ("_Size", 0.015625f * bumpUV);
				mat[i].SetFloat("_WaterLod1Alpha", shaderAlpha);
				mat[i].SetFloat("_Specularity", specularity);
				mat[i].SetFloat("_SpecPower", specPower);
				mat[i].SetFloat("_ShoreDistance", shoreDistance);
				mat[i].SetFloat("_ShoreStrength", shoreStrength);
				mat[i].SetFloat("_Translucency", translucency);
				mat[i].SetFloat("_DistanceCancellation", cancellationDistance);
				mat[i].renderQueue = renderQueue;
				shader_LOD(!shaderLod, material, lowShaderLod);
			}
		}
		//if the device does not support shadows disable shore foam, because the depth buffer is not rendered.
		//Alternatively you could use the Deferred legacy camera renderpath, but again not all mobiles support it ...
		if(SystemInfo.supportsShadows) {
			switchKeyword(mat[0], "SHORE_ON","SHORE_OFF", hasShore); switchKeyword(mat[1], "SHORE_ON","SHORE_OFF", hasShore1);
		} else {
			switchKeyword(mat[0], "SHORE_ON","SHORE_OFF", false); switchKeyword(mat[1], "SHORE_ON","SHORE_OFF", false);
		}

		switchKeyword(mat[0], "FOGON","FOGOFF", hasFog); switchKeyword(mat[1], "FOGON","FOGOFF", hasFog1); switchKeyword(mat[2], "FOGON","FOGOFF", hasFog2);
		switchKeyword(mat[1], "DCON","DCOFF", distCan1); switchKeyword(mat[2], "DCON","DCOFF", distCan2);
	}


	/*
    Prepares the scene for offscreen rendering; spawns a camera we'll use for for
    temporary renderbuffers as well as the offscreen renderbuffers (one for
    reflection and one for refraction).
    */
	void SetupOffscreenRendering (bool runtimeLoad = false) {

		matSetVars();

		mat1HasRefl=false; mat1HasRefr=false; mat2HasRefl=false; mat2HasRefr=false;

		if(material1 != null) {
			if(material1.HasProperty("_Reflection")) { mat1HasRefl=true; }
			if(material1.HasProperty("_Refraction")) { mat1HasRefr=true; }
		}
		if(material2 != null) {
			if(material2.HasProperty("_Reflection")) { mat2HasRefl=true; }
			if(material2.HasProperty("_Refraction")) { mat2HasRefr=true;  }
		}

		//Hack to make this object considered by the renderer - first make a plane
		//covering the watertiles so we get a decent bounding box, then
		//scale all the vertices to 0 to make it invisible.
		if(!runtimeLoad) gameObject.AddComponent (typeof(MeshRenderer));
        GetComponent<Renderer>().material.renderQueue = renderQueue;
		if(runtimeLoad) return;
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
	void OnWillRenderObject2 ()
	{
		if (renderReflection || renderRefraction) {
			int rint = Time.frameCount % reflrefrXframe;
			//Since reflection and refraction are not easy for the eye to catch their changes,
			//we can update them every x frames to gain performance.
			if(rint == 0) RenderReflectionAndRefraction ();
		}
	}


	public void RenderReflectionAndRefraction() {
		int cullingMask = ~(1 << 4) & renderLayers.value;
        #if UNITY_2017_OR_NEWER
		Camera cam = XRDevice.isPresent ?  Camera.main :  Camera.current;
        #else
        Camera cam = Camera.current;
        #endif
        if ( !cam ) return;

		Camera reflectionCamera, refractionCamera;
		CreateWaterObjects( cam, out reflectionCamera, out refractionCamera );
		
		// find out the reflection plane: position and normal in world space
		Vector3 pos = transform.position;
		Vector3 normal =  transform.up;

		UpdateCameraModes( cam, reflectionCamera );
		UpdateCameraModes( cam, refractionCamera );

		//a hack for now
		if(reflectivity<1f) {
			RenderSettings.fogMode = FogMode.Linear;
			RenderSettings.fog = true;
			RenderSettings.fogColor = surfaceColor*0.5f;
			RenderSettings.fogEndDistance = 3000f;
			RenderSettings.fogStartDistance = -(1f-reflectivity)*20000f;
		}
		
		// Render reflection if needed
		if(renderReflection) {
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
			Vector4 clipPlane = CameraSpacePlane( reflectionCamera, pos, normal, 1f );
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
            if(material!=null) material.SetTexture( "_Reflection", m_ReflectionTexture );
			if(mat1HasRefl) { if(material1!=null)  material1.SetTexture( "_Reflection", m_ReflectionTexture ); }
			if(mat2HasRefl) { if(material2!=null)  material2.SetTexture( "_Reflection", m_ReflectionTexture ); }

		}
		
		// Render refraction
		if(renderRefraction) {
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
			if(material!=null) material.SetTexture( "_Refraction", m_RefractionTexture );

			if(mat1HasRefr) { if(material1!=null) material1.SetTexture( "_Refraction", m_RefractionTexture ); }
			if(mat2HasRefr) { if(material2!=null) material2.SetTexture( "_Refraction", m_RefractionTexture ); }
		}

		if(reflectivity<1f) {
			RenderSettings.fogMode = previousFogMode;
			RenderSettings.fog = previousFogState;
			RenderSettings.fogColor = previousFogColor;
			RenderSettings.fogDensity = previousFogDensity;
			RenderSettings.fogStartDistance = previousFogNear;
			RenderSettings.fogEndDistance = previousFogFar;
		}
	}

	//clear allocated memory!!!
	void OnDestroy() {

		zeroObjects();

#if UNITY_EDITOR
		EditorApplication.update -= _update;
#endif
	}


	public void zeroObjects(bool destroy = false) {
		
	//if(_gridMode == 0) {
			if(destroy) {

				OnDisable();

				foreach(Mesh m in btiles_LOD) { DestroyImmediate(m); }

				for (int L0D=0; L0D<max_LOD; L0D++) {
					foreach(Mesh m in tiles_LOD[L0D]) { DestroyImmediate(m); }
				}

				foreach (Transform child in transform) {
					if(child.name.Contains("Lod_")) { DestroyImmediate(child.gameObject); }
				}

				DestroyImmediate(baseMesh); baseMesh = null;
				baseHeight = null;
			}
		
			vertices = null; normals = null; tangents = null;
			verticesLOD = null; tangentsLOD = null; normalsLOD = null;

			for (int L0D=0; L0D<max_LOD; L0D++) { tiles_LOD[L0D].Clear(); tiles_LOD[L0D] = null; }
			btiles_LOD.Clear(); tiles_LOD.Clear(); btiles_LOD = null; tiles_LOD = null;
		//}

		t_x = null;
		data = null;
		gaussRandom1 = null;
		gaussRandom2 = null;

#if NATIVE
			uocean.UoceanClear(destroy);
#else
			Fourier.ClearLookupTables();
			Fourier2.ClearLookupTables();
			h0 = null;
			h02 = null;
#endif

		if(destroy) GC.Collect();
	}


	void OnEnable() {
		//if target == null assign the main Camera
		if(!player) { player = Camera.main.transform; followMainCamera = true; }
		//with no light with enabled shadows, if you have shore foam enabled you will get depth buffer artifacts. In that case use Deferred Legacy render path or add a dummy light with shadows enabled.
		if(!sun) { 
			if(useShaderLods) { for(int i=0; i<numberLods; i++) { if(mat[i] != null) mat[i].SetVector ("_SunDir", Vector4.zero); } } else { if(material != null) material.SetVector ("_SunDir", Vector4.zero);}
			if(useShaderLods) { for(int i=0; i<numberLods; i++) { if(mat[i] != null) mat[i].SetColor("_SunColor", Color.white); } } else { if(material != null) material.SetColor("_SunColor", Color.white); }
		}
	}

	// Cleanup all the objects we possibly have created
	void OnDisable() {
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

#if !NATIVE
			Fourier.ClearLookupTables();
			Fourier2.ClearLookupTables();
#endif
	}

	private void UpdateCameraModes( Camera src, Camera dest ) {
		if( dest == null )
			return;
		// set water camera to clear the same way as current camera
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;		
		if( src.clearFlags == CameraClearFlags.Skybox )	{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if( !sky || !sky.material )	{
				mysky.enabled = false;
			}else {
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
	private void CreateWaterObjects( Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera ) {	
		reflectionCamera = null;
		refractionCamera = null;
		
		if(this.renderReflection){
			// Reflection render texture
			if( !m_ReflectionTexture || m_OldReflectionTextureSize != renderTexWidth ) {
				if( m_ReflectionTexture ) DestroyImmediate( m_ReflectionTexture );
				m_ReflectionTexture = new RenderTexture( renderTexWidth, renderTexHeight, 16 );
				m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
				m_ReflectionTexture.isPowerOfTwo = true;
				m_ReflectionTexture.hideFlags = HideFlags.DontSave;
				m_OldReflectionTextureSize = renderTexWidth;
			}
			
			// Camera for reflection
			m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
			// catch both not-in-dictionary and in-dictionary-but-deleted-GO
			if (!reflectionCamera) {
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
		
		if(this.renderRefraction){
			// Refraction render texture
			if( !m_RefractionTexture || m_OldRefractionTextureSize != renderTexWidth ){
				if( m_RefractionTexture ) DestroyImmediate( m_RefractionTexture );
				m_RefractionTexture = new RenderTexture( renderTexWidth, renderTexHeight, 16 );
				m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
				m_RefractionTexture.isPowerOfTwo = true;
				m_RefractionTexture.hideFlags = HideFlags.DontSave;
				m_OldRefractionTextureSize = renderTexWidth;
			}
			
			// Camera for refraction
			m_RefractionCameras.TryGetValue(currentCamera, out refractionCamera);
			// catch both not-in-dictionary and in-dictionary-but-deleted-GO
			if (!refractionCamera) {
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
	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
		Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}
	
	// Calculates reflection matrix around the given plane
	private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane) {
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


	public void shader_LOD(bool isActive, Material mat , int lod = 1) {
		if(!isActive){
			if(lod==0) return;
			if(lod==1 || lod==2 || lod==3 || lod==6 || lod==7) {
			if(lod!=6 && lod!=7) renderReflection = false;
			renderRefraction = false;
			if(mat!=null) {  if(lod!=6 && lod!=7) { mat.SetTexture ("_Reflection", null); } mat.SetTexture ("_Refraction", null); }
			killReflRefr();
			}
			if(oceanShader!=null) oceanShader.maximumLOD = lod;
		}else{
			OnDisable(); 
			if(oceanShader!=null)  oceanShader.maximumLOD = defaultLOD;
			//disable refraction and reflection for shaderlods < 4
			if(defaultLOD<4) {
				renderReflection = false;
				renderRefraction = false;
				if(mat!=null) {  mat.SetTexture ("_Reflection", null); mat.SetTexture ("_Refraction", null); }
				killReflRefr();
			}
		}
    }

	public void killReflRefr(int lod = 0) {
		if(material1!= null) {
			if(mat1HasRefr) material1.SetTexture ("_Refraction", null);
			if(mat1HasRefl) material1.SetTexture ("_Reflection", null);
		}
		if(material2!= null) {
			if(mat2HasRefr) material2.SetTexture ("_Refraction", null);
			if(mat2HasRefl) material2.SetTexture ("_Reflection", null);
		}
	}

	public void matSetLod(Material mat, int lod) {
		if(mat != null) mat.shader.maximumLOD = lod;
	}

	public void EnableReflection(bool isActive) {
	    renderReflection = isActive;
		if(!isActive){
			if(material!=null) material.SetTexture ("_Reflection", null);
			killReflRefr();
		}else{
			OnDisable();
		}
    }

	public void EnableRefraction(bool isActive) {
	    renderRefraction = isActive;
		if(!isActive){
			if(material!=null)  material.SetTexture ("_Refraction", null);
			killReflRefr();
		}else{
			OnDisable();
		}
    }


	void Mist (bool isActive) {
		mistEnabled = isActive;	
	}

	void switchKeyword (Material _mat, string keyword1, string keyword2, bool on){
		if(_mat) {
			if(on) { _mat.EnableKeyword(keyword1);  _mat.DisableKeyword(keyword2); }
			else { _mat.EnableKeyword(keyword2);  _mat.DisableKeyword(keyword1); }
		 }
	}


	public void updMaterials() {

		foreach (Transform tile in transform){
			Renderer renderer = tile.GetComponent<Renderer>();

			if(useShaderLods && numberLods>1) {
				if(numberLods==2) {
					if(tile.name.Contains("_0") && tile.name.Contains("Lod")) renderer.material = material;
					if(!tile.name.Contains("_0") && tile.name.Contains("Lod"))  renderer.material = material1;
				}else if(numberLods==3){
					if(tile.name.Contains("_0") && tile.name.Contains("Lod")) renderer.material = material;
					if(tile.name.Contains("_1") && tile.name.Contains("Lod")) renderer.material = material1;
					if(!tile.name.Contains("_0") && !tile.name.Contains("_1") && tile.name.Contains("Lod")) renderer.material = material2;
				}
			} else {
				renderer.material = material;
			}
		}
	}

	//not working correct for now
	private float GetHumidity(float time) {
		int intTime = (int)(time * timeFreq);
		int intPrevTime = (int)(prevTime * timeFreq);
		
		if (intTime != intPrevTime){
			prevValue = nextValue;
			//float d = UnityEngine.Random.value;
			//if(d>0.5f) nextValue = d; else nextValue = -d;
			nextValue = UnityEngine.Random.value;
		}
		prevTime = time;
		float frac = time * timeFreq - intTime;
		
		//return Mathf.SmoothStep(prevValue, nextValue, frac);
		return MySmoothstep(prevValue, nextValue, frac);
	}

	//should use a pooling system
	IEnumerator AddMist () {
		while(true){
			if(player != null && mistEnabled && isBoat){
				Vector3 pos = new Vector3(player.transform.position.x + UnityEngine.Random.Range(-30, 30), player.transform.position.y + 5, player.transform.position.z + UnityEngine.Random.Range(-30, 30));
				if(wind >= 0.12f){
                    if (mistClouds != null && mist != null)
                    {
                        GameObject mistParticles = Instantiate(mist, pos, new Quaternion(0, 0, 0, 0)) as GameObject;
                        mistParticles.transform.parent = transform;
                        GameObject clouds = Instantiate(mistClouds, pos, new Quaternion(0, 0, 0, 0)) as GameObject;
                        clouds.transform.parent = transform;
                    }
				}else if(wind > 0.07f){
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
		waveScale = Lerp(0, scale, wind);
    }

    static float MySmoothstep(float a, float b, float t) {
        t = Mathf.Clamp01(t);
        return a + (t*t*(3-2*t))*(b - a);
    }

	static int MyFloorInt(float g) {
		if(g>=0)return (int)g; else return (int)g-1;
	}

	static int MyCeilInt(float g) {
		if(g>=0)return (int)g+1; else return (int)g;
	}

	static float Lerp (float from, float to, float value) {
		if (value < 0.0f) return from;
		else if (value > 1.0f) return to;
		return (to - from) * value + from;
	}

	static int MyIntPow(int a, int b) {
		int y = 1;

		while(true) {
			if ((b & 1) != 0) y = a*y;
			b = b >> 1;
			if (b == 0) return y;
			a *= a;
		}    
	}
	
	public int GetIndexAtLocation(float x, float y) {
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;
        y = y * sizeQZ;
		y = (y - MyFloorInt(y)) * height;

		return width * MyFloorInt(y) + MyFloorInt(x);
    }

	public int GetIndexChoppyAtLocation(float x, float y) {
		if(scaleA == 0) return GetIndexAtLocation(x , y);
		float x1 = x;
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y  * sizeQZ;
		y = (y - MyFloorInt(y)) * height;

		int wm = width * MyFloorInt(y);
		int idx = wm + MyFloorInt(x);
		float res1 = data[idx].Im * scaleA;

		x1 -= res1;
        x1 = x1 * sizeQX;
		x1 = (x1 - MyFloorInt(x1)) * width;

		return wm + MyFloorInt(x1);
    }

	//faster but less accurate then version2
	//When using choppy waves get the true height: GetWaterHeightAtLocation(x - GetChoppyAtLocation(x, y) , y);
	public float GetWaterHeightAtLocation(float x, float y) {
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y * sizeQZ;
		y = (y - MyFloorInt(y)) * height;

		int idx = width * MyFloorInt(y) + MyFloorInt(x);
		hIndex = idx;

        return data[idx].Re * waveScale * wh1;
    }

	//faster but less accurate then version2
	public float GetChoppyAtLocation(float x, float y) {
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y  * sizeQZ;
		y = (y - MyFloorInt(y)) * height;

		int idx = width * MyFloorInt(y) + MyFloorInt(x);
        return data[idx].Im * scaleA;
    }
	
	//unify the 2 above functions for faster result. faster but less accurate then version2
	public float GetHeightChoppyAtLocation(float x, float y) {
		if(scaleA == 0) return GetWaterHeightAtLocation(x , y);
		float x1 = x;
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y  * sizeQZ;
		y = (y - MyFloorInt(y)) * height;

		int wm = width * MyFloorInt(y);
		int idx = wm + MyFloorInt(x);
		float res1 = data[idx].Im * scaleA;

		x1 -= res1;
        x1 = x1 * sizeQX;
		x1 = (x1 - MyFloorInt(x1)) * width;

		idx = wm + MyFloorInt(x1);
		hIndex = idx;
        return data[idx].Re * waveScale * wh1;
    }

	//should be called directly after GetWaterHeightAtLocation otherwise use GetChoppyAtLocation
	public float GetChoppyAtLocationFast() {
        return data[hIndex].Im * scaleA;
    }


	//more accurate but slower
	public static  int  fy;
	public static float h1, h2, yy;

	//When using choppy waves get the true height: GetWaterHeightAtLocation2(x - GetChoppyAtLocation2(x, y) , y);
	public float GetWaterHeightAtLocation2 (float x, float y) {
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y * sizeQZ;
		y = (y - MyFloorInt(y)) * height;
		yy = y;

		//do quad interp
		int fx = MyFloorInt(x);
		fy = MyFloorInt(y);
		int cx = MyCeilInt(x)%width;
		int cy = MyCeilInt(y)%height;
   
		//find data points for all four points
		float fact = waveScale * wh1;
		int f1 = width * fy;
		int f2 = width * cy;

		float FFd = data[f1 + fx].Re * fact;
		float CFd = data[f1 + cx].Re * fact;
		float CCd = data[f2 + cx].Re * fact;
		float FCd = data[f2 + fx].Re * fact;
   
		//interp across x's
		float xs = x - fx;
		h1 = Lerp(FFd, CFd, xs);
		h2 = Lerp(FCd, CCd, xs);

		//interp across y
		return Lerp(h1, h2, y - fy);
	}
 
	//more accurate but slower
	public float GetChoppyAtLocation2 (float x, float y) {
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y * sizeQZ;
		y = (y - MyFloorInt(y)) * height;
 
		//do quad interp
		int fx = MyFloorInt(x);
		int fy = MyFloorInt(y);
		int cx = MyCeilInt(x)%width;
		int cy = MyCeilInt(y)%height;
   
		//find data points for all four points
		int f1 = width * fy;
		int f2 = width * cy;

		float FFd = data[f1 + fx].Im * scaleA;
		float CFd = data[f1 + cx].Im * scaleA;
		float CCd = data[f2 + cx].Im * scaleA;
		float FCd = data[f2 + fx].Im * scaleA;
   
		//interp across x's
		float xs = x - fx;
		float h1 = Lerp(FFd, CFd, xs);
		float h2 = Lerp(FCd, CCd, xs);
   
		//interp across y
		return Lerp(h1, h2, y - fy);     
	}

	//unify the 2 above functions for faster result.
	public float GetHeightChoppyAtLocation2 (float x, float y) {
		if(scaleA == 0) return GetWaterHeightAtLocation2(x , y);
		float x1 = x;
        x = x * sizeQX;
		x = (x - MyFloorInt(x)) * width;

        y = y * sizeQZ;
		y = (y - MyFloorInt(y)) * height;
 
		//do quad interp
		int fx = MyFloorInt(x);
		int fy = MyFloorInt(y);
		int cx = MyCeilInt(x)%width;
		int cy = MyCeilInt(y)%height;
   
		//find data points for all four points
		int f1 = width * fy;
		int f2 = width * cy;
		float FFd = data[f1 + fx].Im * scaleA;
		float CFd = data[f1 + cx].Im * scaleA;
		float CCd = data[f2 + cx].Im * scaleA;
		float FCd = data[f2 + fx].Im * scaleA;
   
		//interp across x's
		float xs = x - fx;
		float h1 = Lerp(FFd, CFd, xs);
		float h2 = Lerp(FCd, CCd, xs);
   
		//interp across y
		float res1 = Lerp(h1, h2, y - fy);  

		//-------------------------------
		//caculate only the height part now
		float fact = waveScale * wh1;

		x1 -= res1;
	    x1 = x1 * sizeQX;
		x1 = (x1 - MyFloorInt(x1)) * width;

		fx = MyFloorInt(x1);
		cx = MyCeilInt(x1)%width;

		FFd = data[f1 + fx].Re * fact;
		CFd = data[f1 + cx].Re * fact;
		CCd = data[f2 + cx].Re * fact;
		FCd = data[f2 + fx].Re * fact;
   
		//interp across x's
		xs = x1 - fx;
		h1 = Lerp(FFd, CFd, xs);
		h2 = Lerp(FCd, CCd, xs);

		//interp across y
		return Lerp(h1, h2, y - fy);
	}
		
	//should be called directly after GetWaterHeightAtLocation2 otherwise use GetChoppyAtLocation2
	public float GetChoppyAtLocation2Fast () {
		return Lerp(h1, h2, yy - fy);     
	}


#if !NATIVE
	public void InitWaveGenerator(bool skip = false, bool useMyRandom = false) {
		// Wind restricted to one direction, reduces calculations
		Vector2 windDirection = new Vector2 (windx, windy);

		int len = width * height;

        if (h0 != null && h0.Length != len) h0 = null;
        if (h02 != null && h02.Length != len) h02 = null;

        if (h0 == null) h0 = new ComplexF[len];
		if(h02 == null) h02 = new ComplexF[len];

		if(useMyRandom && (gaussRandom1 == null || gaussRandom2 == null)) {
			useMyRandom = false;
			Debug.Log("Fixed Gaussian Rand table is null!");
		}

        if(gaussRandom1 != null && gaussRandom1.Length != len) gaussRandom1 = null;
        if (gaussRandom2 != null && gaussRandom2.Length != len) gaussRandom2 = null;

        if (gaussRandom1 == null || (gaussRandom1 != null && gaussRandom1.Length == 0) ) gaussRandom1 = new float[len]; 
		if (gaussRandom2 == null || (gaussRandom2 != null && gaussRandom2.Length == 0)) gaussRandom2 = new float[len];
		

		// Initialize wave generator	
		for (int y=0; y<height; y++) {
			for (int x=0; x<width; x++) {
				float yc = y < height / 2f ? y : -height + y;
				float xc = x < width / 2f ? x : -width + x;
				float tp = TWO_PI * waveDistanceFactor;
				Vector2 vec_k = new Vector2 (tp * xc / size.x, tp* yc / size.z);
				int idx = width * y + x;

				if(useMyRandom) {
					if(!skip) h02[idx] = new ComplexF (gaussRandom1[idx], gaussRandom2[idx]);
				} else {
					if(!skip) {
						float g1 = GaussianRnd();
						float g2 = GaussianRnd();
						gaussRandom1[idx] = g1;
						gaussRandom2[idx] = g2;
						h02[idx] = new ComplexF (g1, g2);
					}
				}

				h0 [idx] = h02[idx] * 0.707f * (float)Math.Sqrt (P_spectrum (vec_k, windDirection));

			}
		}

	}


	float GaussianRnd () {
		float x1 = UnityEngine.Random.value;
		float x2 = UnityEngine.Random.value;
	
		if (x1 == 0.0f) x1 = 0.01f;
	
		return (float)(Math.Sqrt (-2.0 * Math.Log (x1)) * Math.Cos (TWO_PI * x2));
	}

    // Phillips spectrum
	float P_spectrum (Vector2 vec_k, Vector2 wind) {
		float A = vec_k.x > 0.0f ? 1.0f : 0.05f; // Set wind to blow only in one direction - otherwise we get turmoiling water
	
		float L = wind.sqrMagnitude / 9.81f;
		float k2 = vec_k.sqrMagnitude;
		// Avoid division by zero
		if (vec_k.sqrMagnitude == 0.0f || wind.magnitude == 0.0f) {
			return 0.0f;
		}
		float vcsq=vec_k.magnitude;	
		return (float)(A * Math.Exp (-1.0f / (k2 * L * L) - Math.Pow (vcsq * 0.1, 2.0)) / (k2 * k2) * Math.Pow (Vector2.Dot (vec_k / vcsq, wind / wind.magnitude), 2.0));// * wind_x * wind_y;
	}
#endif

#if NATIVE
    public void getGaussianTable()
    {
        int len = width * height;

        if (gaussRandom1 != null && gaussRandom1.Length != len) gaussRandom1 = null;
        if (gaussRandom2 != null && gaussRandom2.Length != len) gaussRandom2 = null;

        if (gaussRandom1 == null || gaussRandom1.Length == 0) gaussRandom1 = new float[len];
        if (gaussRandom2 == null || gaussRandom2.Length == 0) gaussRandom2 = new float[len];
        uocean._getFixedRandomTable(gaussRandom1, gaussRandom2);
    }

    public void InitNative()
    {
        uocean.UInit(width, height, pWindx, pWindy, speed, waveScale, choppy_scale, size.x, size.y, size.z, waveDistanceFactor);
        //getGaussianTable();//optional: get the gaussian random table in case we want to save it
    }
#endif


    //Only for the unity editor. Makes the ocean follow the editor camera.
#if UNITY_EDITOR
    void _update () {
		 if(oldSceneView != SceneView.currentDrawingSceneView) {
			if( EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof( SceneView ) ) {

                #if UNITY_2017_OR_NEWER
		        Camera cam = XRDevice.isPresent ?  Camera.main :  Camera.current;
                #else
                Camera cam = Camera.current;
                #endif

                if (cam) {
					if(oldTransform2 != cam.name) {
						oldSceneView = SceneView.currentDrawingSceneView;
						player = cam.transform;
						//Debug.Log("------> "+player.name);
						oldTransform2 = cam.name;
						isBoat = false;
					}
				}
			} else {
				if(oldTransform) player = oldTransform;
				oldTransform2 = player.name;
				oldSceneView = SceneView.currentDrawingSceneView;
				//Debug.Log(player.name);
				if(player.GetComponent<Camera>()) isBoat=false; else isBoat = true;
			}
		 }
	}
#endif

}
