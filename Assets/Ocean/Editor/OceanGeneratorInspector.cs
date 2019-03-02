using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[CustomEditor(typeof(Ocean))]
[System.Serializable]
public class OceanGeneratorInspector : Editor {

	static private Texture2D blankTexture {
		get {
			return EditorGUIUtility.whiteTexture;
		}
	}


	//private Texture2D logo;
	private Texture2D back;
	private static string title;

	private static int oldEveryXframe;

	private static Color oldWaterColor;
	private static Color oldsurfaceColor;
	private static Color oldFakeWaterColor;
	private static float oldFoamFactor;
	private static float oldSpecularity, oldSpecPower;
	private static bool oldShaderLod;
	private static bool oldrefl, prevrefl;
	private static bool oldrefr, prevrefr, oldSpread, oldFixed;
	private static float oldFoamUV, oldBumpUV, oldShaderAlpha;
	private static float oldTranslucency, oldShoreDistance, oldShoreStrength;
	private static Vector3 oldSunDir;
	private static int oldRefrReflXframe, oldeDefaultLod, oldRenderQueue, oldDiscSize;
	private static string presetPath;
	private static int currentPreset, oldpreset, oldmodeset;
	private static bool oldRenderRefraction, oldFixedTiles;
	private static Shader oldShader;
	private static bool hasShore, hasShore1,hasFog, hasFog1, hasFog2, distCan1, distCan2;
	private static float cancellationDistance;
	private static int ocW, oldocW, oldGridRes;

	private readonly string[] defShader = {"default lod","1 (alpha)","2","3","4","5","6 (alpha)","7 (alpha)","8(translucent)"};
	private readonly string[] skiplods = {"off","1","2","3","4"};
	private readonly string[] tileSize = {"8x8","16x16","32x32","64x64","128x128"
    #if UNITY_2017_3_OR_NEWER
    , "256x256"
    #endif
    };
	private readonly string[] mode = {"Random Gaussian Table","Fixed Gaussian Table"};
	private readonly string[] discSize = {"small", "medium", "large"};
	//private string[] projRes = {"low", "medium", "high"};
	private readonly string[] gridMode = {"tiles"};//,"proj grid"};

	private static string[] presets, presetpaths;

	private static int editormode = 0;

    private bool markedDirty = false;
    private int checkPlayCounter = 0;

    private Ocean _ocean;

	private string GetPluginPath() {
		MonoScript ms = MonoScript.FromScriptableObject( this );
		string scriptPath = AssetDatabase.GetAssetPath( ms );

		var directoryInfo = Directory.GetParent( scriptPath ).Parent;
		return directoryInfo != null ? directoryInfo.FullName : null;
	}


	private string FilePathToAssetPath(string filePath) {
		int indexOfAssets = filePath.LastIndexOf("Assets");

		return filePath.Substring(indexOfAssets);
	}


	void checkPdir(Ocean ocean) {
		if(Directory.Exists(presetPath)){
			string[] dp = Directory.GetFiles(presetPath);
			int k=0;

			for(int i=0; i<dp.Length; i++) {
				if(!dp[i].Contains(".meta") && dp[i].Contains(".preset"))  k++;
			}

			presets=null;
			presetpaths=null;
			System.Array.Resize(ref presets, k+2);
			System.Array.Resize(ref presetpaths, k+1);
			k=0;
			presets[0] = "Select Preset";

			for(int i=0; i<dp.Length; i++) {
				if(!dp[i].Contains(".meta") && dp[i].Contains(".preset")) {
					k++;
					presetpaths[k] = dp[i];
					presets[k+1] = Path.GetFileName(dp[i]).Replace(".preset","");
					if (presets[k+1] == ocean._name) {  currentPreset = k+1; oldpreset = currentPreset;}
				}
			}
			dp=null;
		}
	}



	void OnEnable() {
		Ocean ocean = target as Ocean;
		oldmodeset = ocean._gaussianMode;

        _ocean = ocean;

		var script = MonoScript.FromScriptableObject( this );
		presetPath = Path.GetDirectoryName( AssetDatabase.GetAssetPath( script ))+"/_OceanPresets";

		checkPdir(ocean);

		//Load dynamic resources
		string pluginPath = FilePathToAssetPath( GetPluginPath() );


		string backPath = Path.Combine(pluginPath, "Editor");
		backPath = Path.Combine(backPath, "Background.png");

		back = AssetDatabase.LoadAssetAtPath<Texture2D>(backPath);

		if (null == back) Debug.LogError("null == back");

		oldRenderRefraction = ocean.renderRefraction;
		oldDiscSize = ocean.discSize;
		oldFixedTiles = ocean.fixedTiles;
		oldGridRes = ocean._gridRes;

		checkOceanWidth(ocean);
	}






    public override void OnInspectorGUI() {

		Ocean ocean = target as Ocean;

        GUILayout.Space(8);
		EditorGUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Ocean Settings")) { editormode=0;}
		if (GUILayout.Button("Extra Settings")) {editormode=1;}
		if (GUILayout.Button("Objects")) {editormode=2;}
		EditorGUILayout.EndHorizontal();


		if(editormode==0) {
			GUILayout.FlexibleSpace();
			DrawBackground();

			GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
			GUI.contentColor = new Color(1f, 1f, 1f, 1f);


			GUILayout.Space(-8);
			EditorGUIUtility.labelWidth = 70F;// LookLikeControls(80f);
		

			DrawSeparator();

			DrawVerticalSeparator();
		
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			ocean._gaussianMode = EditorGUILayout.Popup(ocean._gaussianMode, mode, GUILayout.MaxWidth(160));
				
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Gaussian Random Table","If a Fixed table is used, then the library will use a saved gaussian random table.\n\n"+
				"To save the fixed random table, make your modifications and save the preset in PLAY MODE!.\n\n"+
				"After this you can use the preset loader with the appropriate flag to load tour preset with a fixed gaussian random table. (After the Initialize function. See Start() in Ocean.cs.)\n\n"+
				"This is useful when you want a predictable simulation behavior."
				,"OK");		
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(2);
			GUI.backgroundColor = new Color(.7f, .8f, 1f, 1f);

			currentPreset = EditorGUILayout.Popup(currentPreset, presets);

			if(oldpreset != currentPreset ) {
				if(currentPreset==0) { currentPreset = oldpreset; return; }
				if(presetLoader.loadPreset(ocean, presetpaths[currentPreset-1], EditorApplication.isPlaying)) {
					oldpreset = currentPreset;
					ocean._name = presets[currentPreset];
					checkOceanWidth(ocean);
					oldRenderRefraction = ocean.renderRefraction;
					makeDirty(ocean);
				} else {
					currentPreset = oldpreset;
					checkPdir(ocean);
				}

			}
			

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Load preset")) {
				string preset = EditorUtility.OpenFilePanel("Load Ocean preset",presetPath,"preset");

				if (preset != null) {
					if (preset.Length > 0) {
						presetLoader.loadPreset(ocean, preset,EditorApplication.isPlaying);
						title = Path.GetFileName(preset).Replace(".preset", ""); ocean._name = title;
						updcurPreset();
						checkOceanWidth(ocean);
						oldRenderRefraction=ocean.renderRefraction;
                        makeDirty(ocean);
                    }
				}
			}

			if (GUILayout.Button("Save preset")) {
				savePreset(ocean);
				checkPdir(ocean);
			}
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
			EditorGUILayout.EndVertical();

			GUILayout.Space(4);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Target:"); 
			GUILayout.Space(-100);
			ocean.player = (Transform)EditorGUILayout.ObjectField(ocean.player, typeof(Transform), true, GUILayout.MinWidth(120));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Follow", GUILayout.MaxWidth(65));
			ocean.followMainCamera = EditorGUILayout.Toggle(ocean.followMainCamera);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Material L0");
			GUILayout.Space(-100);
			ocean.material = (Material)EditorGUILayout.ObjectField(ocean.material, typeof(Material), true, GUILayout.MinWidth(120));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Material L1");
			GUILayout.Space(-100);
			ocean.material1 = (Material)EditorGUILayout.ObjectField(ocean.material1, typeof(Material), true, GUILayout.MinWidth(120));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Material L2");
			GUILayout.Space(-100);
			ocean.material2 = (Material)EditorGUILayout.ObjectField(ocean.material2, typeof(Material), true, GUILayout.MinWidth(120));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Shader");
			GUILayout.Space(-100);
			ocean.oceanShader = (Shader)EditorGUILayout.ObjectField(ocean.oceanShader, typeof(Shader), true, GUILayout.MinWidth(120));
			EditorGUILayout.EndHorizontal();
			//GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Sun: ");
			GUILayout.Space(-100);
			ocean.sun = (Transform)EditorGUILayout.ObjectField(ocean.sun, typeof(Transform), true, GUILayout.MinWidth(120));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Load Sun",GUILayout.MaxWidth(59));
			ocean.loadSun = EditorGUILayout.Toggle(ocean.loadSun,GUILayout.MaxWidth(79));
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(4);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("RenderQueue:");
			GUILayout.Space(-70);
			ocean.renderQueue = EditorGUILayout.IntField(ocean.renderQueue, GUILayout.MaxWidth(65));
			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(4);

			GUILayout.Space(4);
			DrawHalfSeparator();

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Waves settings");
			GUILayout.Space(12);
			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("Scale");

			EditorGUILayout.BeginHorizontal();
			ocean.scale = EditorGUILayout.Slider(ocean.scale, 0, 200);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Wave scale.","Sets the vertical scale of the waves.\n\nCan be changed in runtime.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Choppy scale");
			
			EditorGUILayout.BeginHorizontal();
			ocean.choppy_scale = EditorGUILayout.Slider(ocean.choppy_scale, 0, 100);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Choppy scale.","Sets the choppiness of the waves.\n\nCan be changed in runtime.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Waves speed");

			EditorGUILayout.BeginHorizontal();
			ocean.speed = EditorGUILayout.Slider(ocean.speed, 0.1f, 3f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Wave speed.","Sets the speed of the wave movement.\n\nCan be changed in runtime.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Wave density");
			
			EditorGUILayout.BeginHorizontal();
			ocean.waveDistanceFactor = EditorGUILayout.Slider(ocean.waveDistanceFactor, 0.4f, 3f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Wave density.","Determines the distance between waves. Careful when using it!\n\n If you lower its value you have to lower 'wave scale' and 'choppy scale'\n\nCan be changed in runtime.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			DrawHalfSeparator();

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Mist");
			GUILayout.Space(12);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Enable Mist", GUILayout.MaxWidth(100));
			ocean.mistEnabled = EditorGUILayout.Toggle(ocean.mistEnabled);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Mist Low");
			GUILayout.Space(-170);
			ocean.mistLow = (GameObject)EditorGUILayout.ObjectField(ocean.mistLow, typeof(GameObject), true, GUILayout.MaxWidth(130));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Mist Hi");
			GUILayout.Space(-170);
			ocean.mist = (GameObject)EditorGUILayout.ObjectField(ocean.mist, typeof(GameObject), true, GUILayout.MaxWidth(130));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Mist Clouds");
			GUILayout.Space(-170);
			ocean.mistClouds = (GameObject)EditorGUILayout.ObjectField(ocean.mistClouds, typeof(GameObject), true, GUILayout.MaxWidth(130));
			EditorGUILayout.EndHorizontal();

			DrawHalfSeparator();
			GUILayout.Space(2);
			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Wind");
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Dynamic waves");

			ocean.dynamicWaves = EditorGUILayout.Toggle(ocean.dynamicWaves);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Wind power");
			ocean.humidity = EditorGUILayout.Slider(ocean.humidity, 0.01f, 1f);

			EditorGUILayout.LabelField("Wind direction");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("X");
			GUILayout.Space(-100);
			ocean.pWindx = EditorGUILayout.FloatField(ocean.pWindx);
			EditorGUILayout.LabelField("Y");
			GUILayout.Space(-100);
			ocean.pWindy = EditorGUILayout.FloatField(ocean.pWindy);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);
			GUI.contentColor = new Color(0.5f, 1f, 1f, 1f);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Foam Strength", GUILayout.MaxWidth(90));
			GUI.contentColor = new Color(1f, 1f, 1f, 1f);
			ocean.foamFactor = EditorGUILayout.Slider(ocean.foamFactor, 0f, 3.0f);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Foam Duration", GUILayout.MaxWidth(90));
			ocean.foamDuration = EditorGUILayout.Slider(ocean.foamDuration, 2f, 0.1f);
			EditorGUILayout.EndHorizontal();

			DrawHalfSeparator();

			GUILayout.Space(8);
			if(!ocean.fixedUpdate) {
			EditorGUILayout.BeginHorizontal();
				GUI.contentColor = new Color(0.7f, 1f, 0.7f, 1f);
				EditorGUILayout.LabelField("Spread along frames");
				GUI.contentColor = new Color(1f, 1f, 1f, 1f);
				ocean.spreadAlongFrames = EditorGUILayout.Toggle(ocean.spreadAlongFrames);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Enable spread along frames.","This will enable/disable the work offload of the wave calculations by spreading the calculations among x frames declared by the slider.","OK");
				}
			EditorGUILayout.EndHorizontal();
			} else {
				EditorGUILayout.LabelField(""); 
				GUILayout.Space(2);
			}

			if (ocean.spreadAlongFrames && !ocean.fixedUpdate) {
				EditorGUILayout.LabelField("Calc waves every x frames:");
				EditorGUILayout.BeginHorizontal();

				ocean.everyXframe = (int)EditorGUILayout.Slider(ocean.everyXframe, 2, 8);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Calculate waves every x frames.","This will spread the calculation of the waves along the frames you decalre.\n\n"+
                    "When you use Vsyc for frame limitting the recommended values are 2-4. Otherwise you will notice jerky wave movement. When using XR,  recommended value is 1.\n\n" +
                    "You might want to consider using the fixedUpdate flag where the waves are getting updated in the FixedUpdate function. This will give better framerates when vsync " +
                    "is used. When fixedUpdate is used the spread along frames function gets disabled.", "OK");
                }
				if (oldEveryXframe != ocean.everyXframe) {
					oldEveryXframe = ocean.everyXframe;
					ocean.setSpread();
				}

				EditorGUILayout.EndHorizontal();
			} else {
				EditorGUILayout.LabelField("");
				EditorGUILayout.LabelField("");
				GUILayout.Space(2);
			}

			DrawHalfSeparator();
			GUILayout.Space(7);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Default shader lod");
			GUILayout.Space(-22);
			ocean.defaultLOD = EditorGUILayout.Popup(ocean.defaultLOD, defShader,GUILayout.MaxWidth(90));
			if(ocean.defaultLOD==0) ocean.defaultLOD = 1;
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Default shader Quality.","Decalre the shader lod(quality) that is getting loaded by default when the simulation starts."+
				"\n\n8: high quality-translucent. (reflection/refraction/translucency/wave bump/foam bump/double foam)"+
				"\n\n7: high quality-transparent. (reflection/wave bump/foam bump/double foam/alpha)"+
				"\n\n6: high quality-transparent. (reflection/wave bump/foam/alpha)\n\n5: high quality. (reflection/refraction/wave bump/foam bump/double foam)\n\n"+
				"4: normal quality. (reflection/refraction/wave bump/foam)\n\n3: medium quality. (wave bump/foam)\n\n2: low quality. (wave bump)\n\n1: medium quality-transparent. (wave bump/foam bump/foam/alpha)\n\n"+
				"For older mobile devices the medium quality shader is recommended (3)\n\nAll shaders that have foam support shore lines if enabled.","OK");
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(7);
			GUI.contentColor = new Color(0.75f, 0.75f, 0.75f, 1f);
			EditorGUILayout.BeginHorizontal();
			ocean.shaderLod = EditorGUILayout.Toggle(ocean.shaderLod,GUILayout.MaxWidth(15));
			EditorGUILayout.LabelField("Shader Low lod",GUILayout.MinWidth(70));
			ocean.lowShaderLod = EditorGUILayout.Popup(ocean.lowShaderLod, defShader,GUILayout.MaxWidth(90));
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Enable/disable low quality shader.","This will enable/disable the selected low quality shader.\n\n"+
				"You can switch to the desired lower quality shader lod by code, using the shader_LOD function in the Ocean.cs script.","OK");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			ocean.useShaderLods = EditorGUILayout.Toggle(ocean.useShaderLods,GUILayout.MaxWidth(15));
			EditorGUILayout.LabelField("Enable shader lods");
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Enable/disable of shader lods on tiles.","This will enable/disable shader lod on tiles with higher lod.\n\n"+
				"You can set the number of lods that will be used with the slider: No. of shader lods.","OK");		
			}
			EditorGUILayout.EndHorizontal();

			if(ocean.useShaderLods) {
				EditorGUILayout.LabelField("No. of shader lods");
				EditorGUILayout.BeginHorizontal();
				ocean.numberLods = (int)EditorGUILayout.Slider(ocean.numberLods, 1, 3);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Number of shader lods","How many shader lods will be used for the mesh lods.\n\n(The alpha shader falls back to lod1 alpha shader lod.)\n\n1: Only the main shader will be used on all tiles.\n\n2: The main shader and the 2nd lod level will be used.\n\n"+
					"3: The main shader and the 2nd and the 3rd lod levels will be used.\n\nYou assign the lod shaders to the material L1 and material L2 respectively. (See example presets).","OK");
				}
				EditorGUILayout.EndHorizontal();
			} else {
				EditorGUILayout.LabelField("");
				EditorGUILayout.LabelField("");
				GUILayout.Space(2);
			}
			GUILayout.Space(7);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Lod skip frames");
			GUILayout.Space(-22);
			ocean.lodSkipFrames = EditorGUILayout.Popup(ocean.lodSkipFrames, skiplods,GUILayout.MaxWidth(50));
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Skip mesh lod updates.","How many frames should be skipped before updating the mesh lods.","OK");
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(4);
			GUI.contentColor = new Color(1f, 1f, 1f, 1f);
			DrawHalfSeparator();

			GUILayout.Space(4);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Use Fixed Update");
			ocean.fixedUpdate = EditorGUILayout.Toggle(ocean.fixedUpdate);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Use FixedUpdate()","Enable/disable the use of the FixedUpdate() function for calculation of the waves.\n\n"+
				"This gives higher frame rates but disables the spread along frames functionality. It is a good alternative when you get jerky waves with vsync on and almost doubles the frame rate compared to regular Update()."+
				"\n\nAvoid on mobile.","OK");		
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			DrawSeparator();
			EditorGUILayout.EndVertical();

			


			//=======================================================================================================================


			GUILayout.FlexibleSpace();
			GUILayout.Space(35);
			GUILayout.FlexibleSpace();

			EditorGUILayout.BeginVertical();//----------------------------------------------------------------------------------------
			
			EditorGUILayout.BeginHorizontal();
			if(!EditorApplication.isPlaying) EditorGUILayout.LabelField("Mesh Mode: "); else EditorGUILayout.LabelField("Mesh Mode:   "+gridMode[ocean._gridMode]);
			if(!EditorApplication.isPlaying) ocean._gridMode = EditorGUILayout.Popup(ocean._gridMode, gridMode, GUILayout.MaxWidth(90));
			EditorGUILayout.EndHorizontal();

			if(ocean._gridMode == 0) {
				EditorGUILayout.LabelField("Tiles count");
				ocean.tiles = (int)EditorGUILayout.Slider(ocean.tiles, 1, 15);
				EditorGUILayout.LabelField("Tiles size");
				ocean.size = EditorGUILayout.Vector3Field("", ocean.size);
			
				GUILayout.Space(6);
				EditorGUILayout.LabelField("Tiles polycount: "+(ocean.width*ocean.height).ToString());
				EditorGUILayout.BeginHorizontal();
				if(!EditorApplication.isPlaying)  EditorGUILayout.LabelField("Width x Height :"); else EditorGUILayout.LabelField("Width x Height : "+tileSize[ocW]);
				GUILayout.Space(-80);
				if(!EditorApplication.isPlaying) ocW = EditorGUILayout.Popup(ocW, tileSize, GUILayout.MaxWidth(70));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(3);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Fixed Disc", GUILayout.MaxWidth(65));
				ocean.fixedTiles = EditorGUILayout.Toggle(ocean.fixedTiles);
				if(ocean.fixedTiles) {
					ocean.discSize = EditorGUILayout.Popup(ocean.discSize, discSize,GUILayout.MaxWidth(70));
				}
				EditorGUILayout.EndHorizontal();
			}
			/*
			if(ocean._gridMode == 1) {
				GUILayout.Space(6);
				EditorGUILayout.BeginHorizontal();
				if(!EditorApplication.isPlaying)  EditorGUILayout.LabelField("Grid resolution :"); else EditorGUILayout.LabelField("Grid resolution : "+projRes[ocean._gridRes]);
				GUILayout.Space(-80);
				if(!EditorApplication.isPlaying) ocean._gridRes = EditorGUILayout.Popup(ocean._gridRes, projRes, GUILayout.MaxWidth(70));
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(6);
				GUILayout.Space(16);
			}*/

			GUILayout.Space(3);
			
			EditorGUILayout.LabelField("Tiles shader Lod start");
			EditorGUILayout.BeginHorizontal();
			ocean.sTilesLod = (int)EditorGUILayout.Slider(ocean.sTilesLod, 0, 3);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Tiles shader Lod start","At which tile lod the lods will start to get lower shader lods (if they are enabled).\n\n","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Far LOD Y-offset");
			EditorGUILayout.BeginHorizontal();
			ocean.farLodOffset = EditorGUILayout.Slider(ocean.farLodOffset, -50f, 0);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Far lod tiles offset","This value will offset the far lod tiles (after the 2nd) to a lower level to eliminate noticable gaps between lod borders.\n\n"+
				"Each extra lod (after the 2nd) will get lowered by 1/3 of the entered value.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			if(ocean._gridMode == 1) GUILayout.Space(10);
			DrawHalfSeparator(false);

			////------------------------------------------
			GUILayout.Space(4);
			if(ocean._gridMode == 1) GUILayout.Space(10);
			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Reflection & Refraction");
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reflection",GUILayout.MaxWidth(59));
			ocean.renderReflection = EditorGUILayout.Toggle(ocean.renderReflection,GUILayout.MaxWidth(79));
			EditorGUILayout.LabelField("Refraction",GUILayout.MaxWidth(64));
			ocean.renderRefraction = EditorGUILayout.Toggle(ocean.renderRefraction,GUILayout.MaxWidth(79));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Every x frames:",GUILayout.MinWidth(80));
			GUILayout.Space(50);
			ocean.reflrefrXframe = EditorGUILayout.IntField(ocean.reflrefrXframe,GUILayout.MaxWidth(39));
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Render Reflection/Refraction every x frames.","This will render the reflection and refraction camera every x frames you declare.\n\n"+
				"Since reflection and refraction are not easy for the eye to catch their changes, we can update them every x frames to gain performance.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(2);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("texture size  xy:",GUILayout.MinWidth(90));
			GUILayout.Space(-110);

			ocean.renderTexWidth = EditorGUILayout.IntField(ocean.renderTexWidth,GUILayout.MaxWidth(50));
			//GUILayout.Space(-30);
			ocean.renderTexHeight = EditorGUILayout.IntField(ocean.renderTexHeight,GUILayout.MaxWidth(50));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reflection clip plane offset");
			ocean.m_ClipPlaneOffset = EditorGUILayout.FloatField(ocean.m_ClipPlaneOffset, GUILayout.MaxWidth(50));
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(4);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Render layers");
			GUILayout.Space(-40);
			int mask = LayerMaskField(ocean.renderLayers);

			if (ocean.renderLayers != mask) {
				ocean.renderLayers = mask;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);
			//DrawHalfSeparator();

			GUILayout.Space(3);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reflectivity",GUILayout.MaxWidth(70));
			ocean.reflectivity = EditorGUILayout.Slider(ocean.reflectivity, 0f, 1f);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Specularity",GUILayout.MaxWidth(70));
			ocean.specularity = EditorGUILayout.Slider(ocean.specularity, 0.01f, 8.0f);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Spec power",GUILayout.MaxWidth(70));
			ocean.specPower = EditorGUILayout.Slider(ocean.specPower, 0f, 1.0f);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);
			//EditorGUILayout.LabelField("Shader Alpha");
			//ocean.shaderAlpha = EditorGUILayout.Slider(ocean.shaderAlpha, 0f, 1f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Alpha",GUILayout.MaxWidth(70));
			ocean.shaderAlpha = EditorGUILayout.Slider(ocean.shaderAlpha, 0f, 1f);
			EditorGUILayout.EndHorizontal();
			////------------------------------------------
			GUILayout.Space(2);

			DrawHalfSeparator(false);
			if(ocean._gridMode == 1) GUILayout.Space(10);
			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Interactive Foam");
			GUILayout.Space(18);
			EditorGUILayout.EndVertical();

		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Interactive foam strength");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			ocean.ifoamStrength = EditorGUILayout.Slider(ocean.ifoamStrength, 0, 50);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Interactive foam strength/duaration.","The strength/duration of the foam produced by the boat that interacts with the ocean.\n\nSet this to 0 to disable calculations for it.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Interactive foam width");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			ocean.ifoamWidth = EditorGUILayout.Slider(ocean.ifoamWidth, 2, 0.1f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Interactive foam width.","The width of the boat's trail foam. (Lower values produce higher width).","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Foam Wake distance");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			ocean.wakeDistance = EditorGUILayout.Slider(ocean.wakeDistance, 1f, 15f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Foam Wake distance.","A factor that determines the length of the foam trail of the player's vessel.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(2);
			DrawHalfSeparator(false);


			////------------------------------------------
			GUILayout.Space(3);
			if(ocean._gridMode == 1) GUILayout.Space(10);
			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Water color");
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();

			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
			GUILayout.Space(4);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Water color",GUILayout.MaxWidth(90));
			ocean.waterColor = EditorGUILayout.ColorField(ocean.waterColor);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Surface color",GUILayout.MaxWidth(90));
			ocean.surfaceColor = EditorGUILayout.ColorField(ocean.surfaceColor);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Lods Refr color",GUILayout.MaxWidth(90));
			ocean.fakeWaterColor = EditorGUILayout.ColorField(ocean.fakeWaterColor);
			EditorGUILayout.BeginVertical();
			GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Fake refraction color.","This color is used on shaders with Alpha support as a fake refraction color.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			
			GUI.contentColor = new Color(0.5f, 1f, 1f, 1f);
			GUILayout.Space(6);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Translucency",GUILayout.MaxWidth(90));
			GUI.contentColor = new Color(1f, 1f, 1f, 1f);
			ocean.translucency = EditorGUILayout.Slider(ocean.translucency, 0, 6f);
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Foam tiling",GUILayout.MaxWidth(90));
			ocean.foamUV = (int)EditorGUILayout.Slider(ocean.foamUV, 1, 16);
			EditorGUILayout.EndHorizontal();
		
			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Bump tiling",GUILayout.MaxWidth(90));
			ocean.bumpUV = (int)EditorGUILayout.Slider(ocean.bumpUV, 1, 16);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(15);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Shore Distance",GUILayout.MaxWidth(90));
			ocean.shoreDistance = EditorGUILayout.Slider(ocean.shoreDistance, 0, 20);
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Shore Strength",GUILayout.MaxWidth(90));
			ocean.shoreStrength = EditorGUILayout.Slider(ocean.shoreStrength, 1, 4);
			EditorGUILayout.EndHorizontal();
			GUI.contentColor = new Color(0.75f, 1f, 0.75f, 1f);
			GUILayout.Space(4);
			DrawHalfSeparator(false);
			GUILayout.Space(4);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Shore Foam L0 | L1       :");
			GUILayout.Space(-120);
			ocean.hasShore = EditorGUILayout.Toggle(ocean.hasShore,GUILayout.MaxWidth(15));
			ocean.hasShore1 = EditorGUILayout.Toggle(ocean.hasShore1, GUILayout.MaxWidth(15));

			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Enable Shore Foam","Enable/disable Shore line foam on shader lods.\n\n"+
				"It is recommended to use it on Lod0. Use on Lod1 if necessary.\n\nIt will enable shore foam only if the shader lod supports it.","OK");		
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(2);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Shader Fog  L0 | L1 | L2 :");
			GUILayout.Space(-120);
			ocean.hasFog = EditorGUILayout.Toggle(ocean.hasFog,GUILayout.MaxWidth(15));
			ocean.hasFog1 = EditorGUILayout.Toggle(ocean.hasFog1, GUILayout.MaxWidth(15));
			ocean.hasFog2 = EditorGUILayout.Toggle(ocean.hasFog2, GUILayout.MaxWidth(15));

			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Enable Fog","Enable/disable Fog on shader lods.\n\n"+
				"It is recommended to use it on all Lods. Use on spefic lods if desired.","OK");		
			}
			EditorGUILayout.EndHorizontal();
			

			if(ocean.hasFog1 || ocean.hasFog2) {
				EditorGUILayout.BeginHorizontal();
				GUI.contentColor = new Color(0.7f, 0.7f, 0.7f, 1f);
				EditorGUILayout.LabelField("Distance cancel.  L1 | L2 :");
				GUILayout.Space(-120);
				ocean.distCan1 = EditorGUILayout.Toggle(ocean.distCan1, GUILayout.MaxWidth(15));
				ocean.distCan2 = EditorGUILayout.Toggle(ocean.distCan2, GUILayout.MaxWidth(15));
				GUI.contentColor = new Color(1f, 1f, 1f, 1f);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Fog Distance Cancellation","This will cancel shader calculations after a certain distance and display only fog. Only for Lod1 and Lod2.\n\n"+
					"Specify the desired distance below. It should be higher then FogEnd.","OK");		
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUI.contentColor = new Color(0.7f, 0.7f, 0.7f, 1f);
				EditorGUILayout.LabelField("Cancellation distance:");
				GUILayout.Space(-120);
				GUI.contentColor = new Color(1f, 1f, 1f, 1f);
				ocean.cancellationDistance = EditorGUILayout.FloatField(ocean.cancellationDistance, GUILayout.MaxWidth(55));
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Fog Cancellation Distance","The distance at which the shader will display only fog. For Lod1 and Lod2.","OK");		
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUI.contentColor = new Color(1f, 0.5f, 0.5f, 1f);
				EditorGUILayout.LabelField("Force depth on camera:");
				GUILayout.Space(-120);
				GUI.contentColor = new Color(1f, 1f, 1f, 1f);
				ocean.forceDepth = EditorGUILayout.Toggle(ocean.forceDepth, GUILayout.MaxWidth(55));
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Force depth on camera","On the forward rendering on most platforms, depth is not drawn and so the shore foam is not working.\n\n"+
					"Use this to force the main camera to draw depth! (Not needed for deferred rendering paths.)","OK");		
				}
				EditorGUILayout.EndHorizontal();
			}

			

			//DrawHalfSeparator(false);




			EditorGUILayout.EndVertical();//----------------------------------------------------------------------------------------
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();


			GUILayout.FlexibleSpace();



            if(GUI.changed) markedDirty = true;

            checkPlayCounter++;
            if (checkPlayCounter > 2) { 
                OnPlaymodeStateChange();
                checkPlayCounter = 0;
            }

        }

		//UPDATE SHADER IN REALTIME

		if (ocean.waterColor != oldWaterColor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_WaterColor", ocean.waterColor);  }   oldWaterColor = ocean.waterColor; }
		if (ocean.surfaceColor != oldsurfaceColor) {  for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_SurfaceColor", ocean.surfaceColor); } oldsurfaceColor = ocean.surfaceColor; }
		if (ocean.fakeWaterColor != oldFakeWaterColor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_FakeUnderwaterColor", ocean.fakeWaterColor); } oldFakeWaterColor = ocean.fakeWaterColor; }

		if (ocean.foamFactor != oldFoamFactor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_FoamFactor", ocean.foamFactor); }  oldFoamFactor = ocean.foamFactor; }
		if (ocean.specularity != oldSpecularity) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_Specularity", ocean.specularity); } oldSpecularity = ocean.specularity; }
		if (ocean.specPower != oldSpecPower) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_SpecPower", ocean.specPower); } oldSpecPower = ocean.specPower; }

		if (ocean.translucency != oldTranslucency) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_Translucency", ocean.translucency); } oldTranslucency = ocean.translucency; }
		if (ocean.shoreDistance != oldShoreDistance) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_ShoreDistance", ocean.shoreDistance); } oldShoreDistance = ocean.shoreDistance; }
		if (ocean.shoreStrength !=  oldShoreStrength) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_ShoreStrength", ocean.shoreStrength); } oldShoreStrength = ocean.shoreStrength; }

		if (ocean.foamUV != oldFoamUV) { for(int i=0; i<2; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_FoamSize", ocean.foamUV); } oldFoamUV = ocean.foamUV; }
		if (ocean.bumpUV != oldBumpUV) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_Size", 0.015625f * ocean.bumpUV); } oldBumpUV = ocean.bumpUV; }
		if (ocean.shaderAlpha != oldShaderAlpha) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_WaterLod1Alpha", ocean.shaderAlpha); } oldShaderAlpha = ocean.shaderAlpha; }

		if (ocean.hasShore != hasShore) { if(ocean.mat[0]) switchKeyword(ocean.mat[0], "SHORE_ON","SHORE_OFF", ocean.hasShore); hasShore = ocean.hasShore; }
		if (ocean.hasShore1 != hasShore1) { if(ocean.mat[1]) switchKeyword(ocean.mat[1], "SHORE_ON","SHORE_OFF", ocean.hasShore1); hasShore1 = ocean.hasShore1; }

		if (ocean.hasFog != hasFog) { if(ocean.mat[0]) switchKeyword(ocean.mat[0], "FOGON","FOGOFF", ocean.hasFog); hasFog = ocean.hasFog; }
		if (ocean.hasFog1 != hasFog1) { if(ocean.mat[1]) switchKeyword(ocean.mat[1], "FOGON","FOGOFF", ocean.hasFog1); hasFog1 = ocean.hasFog1; }
		if (ocean.hasFog2 != hasFog2) { if(ocean.mat[2]) switchKeyword(ocean.mat[2], "FOGON","FOGOFF", ocean.hasFog2); hasFog2 = ocean.hasFog2; }

		if (ocean.distCan1 != distCan1) { if(ocean.mat[1]) switchKeyword(ocean.mat[1], "DCON","DCOFF", ocean.distCan1); distCan1 = ocean.distCan1; }
		if (ocean.distCan2 != distCan2) { if(ocean.mat[2]) switchKeyword(ocean.mat[2], "DCON","DCOFF", ocean.distCan2); distCan2 = ocean.distCan2; }

		if(ocean.cancellationDistance != cancellationDistance) { for(int i=1; i<3; i++) { if(ocean.mat[i]) ocean.mat[i].SetFloat("_DistanceCancellation", ocean.cancellationDistance); } cancellationDistance = ocean.cancellationDistance; }

		if(ocean.renderQueue != oldRenderQueue) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].renderQueue = ocean.renderQueue; } oldRenderQueue = ocean.renderQueue; }

		if (ocean.reflrefrXframe != oldRefrReflXframe) { if(ocean.reflrefrXframe<=0) ocean.reflrefrXframe = 1; oldRefrReflXframe = ocean.reflrefrXframe; }
		if (oldFixed != ocean.fixedUpdate) {
			if(ocean.fixedUpdate) { oldSpread = ocean.spreadAlongFrames;  } 
			if(!ocean.fixedUpdate) {ocean.spreadAlongFrames = oldSpread; }
			oldFixed = ocean.fixedUpdate;
		 }

		if(ocean.sun!= null) {
			if(oldSunDir != ocean.sun.transform.forward) {
				ocean.SunDir = ocean.sun.transform.forward;
				oldSunDir = ocean.SunDir;
			}
		}

		if(oldeDefaultLod != ocean.defaultLOD) {
			//hardcoded for now. If the shader has alpha disable refraction since it is not needed (and not supported by the shader.)
			if(ocean.defaultLOD == 6 || ocean.defaultLOD == 7 || ocean.defaultLOD == 1 ) {
				oldRenderRefraction = ocean.renderRefraction;
				ocean.renderRefraction = false;
				//Debug.Log("Shader cannot use Refraction");
			}

			if(ocean.defaultLOD != 6 || ocean.defaultLOD != 7 || ocean.defaultLOD != 1) {
				ocean.renderRefraction = oldRenderRefraction;
			}

			oldeDefaultLod = ocean.defaultLOD;
		}

		if (ocean.renderReflection != oldrefl) {
			ocean.EnableReflection(ocean.renderReflection);
			oldrefl = ocean.renderReflection;
		}

		if (ocean.renderRefraction != oldrefr) {
			if(ocean.defaultLOD!=6 && ocean.defaultLOD!=7) {
				ocean.EnableRefraction(ocean.renderRefraction);
			} else {
				ocean.renderRefraction=false;
			}
			oldrefr = ocean.renderRefraction;
		}

		if (ocean.shaderLod != oldShaderLod) {
			if (ocean.shaderLod) {
				prevrefl = ocean.renderReflection;
				prevrefr = ocean.renderRefraction;
			}

			if(ocean.lowShaderLod>0) ocean.shader_LOD(!ocean.shaderLod, ocean.material, ocean.lowShaderLod);
			oldShaderLod = ocean.shaderLod;

			if (!ocean.shaderLod) {
				ocean.EnableReflection(prevrefl); ocean.EnableRefraction(prevrefr);
				oldrefl = ocean.renderReflection; oldrefr = ocean.renderRefraction;
			}
		}

		if(ocean.oceanShader !=  oldShader) {
			if(ocean.material != null) {
				ocean.material.shader = ocean.oceanShader;
				oldShader = ocean.oceanShader;
			}
		}
		//switch between fixed and random gaussian tables
		if(oldmodeset != ocean._gaussianMode) {
			oldmodeset = ocean._gaussianMode;
			if(ocean._gaussianMode == 0) {
			#if NATIVE
				uocean._setFixedRandomTable(false, ocean.width * ocean.height, ocean.gaussRandom1, ocean.gaussRandom2);
			#else
				ocean.InitWaveGenerator(false, false);
			#endif
			} else {
			#if NATIVE
				uocean._setFixedRandomTable(true, ocean.width * ocean.height, ocean.gaussRandom1, ocean.gaussRandom2);
			#else
				ocean.InitWaveGenerator(false, true);
			#endif
			}
		}

		//switch fixed disc size
		if(oldDiscSize != ocean.discSize) {
			oldDiscSize = ocean.discSize;
			if(Application.isPlaying)  ocean.initDisc();
		}

		if(oldFixedTiles != ocean.fixedTiles) {
			oldFixedTiles = ocean.fixedTiles;
			if(Application.isPlaying)  ocean.initDisc();
		}

        if (ocW != oldocW)
        {
            switch (ocW)
            {
                case 0:
                    ocean.width = ocean.height = 8; break;
                case 1:
                    ; ocean.width = ocean.height = 16; break;
                case 2:
                    ocean.width = ocean.height = 32; break;
                case 3:
                    ocean.width = ocean.height = 64; break;
                case 4:
                    ocean.width = ocean.height = 128; break;
                #if UNITY_2017_3_OR_NEWER
                case 5:
                    ocean.width = ocean.height = 256; break;
                #endif
            }

            #if !NATIVE
                ocean.InitWaveGenerator();
            #endif

            #if NATIVE
                ocean.InitNative();
            #endif

            ocean._gaussianMode = 0;

            makeDirty(ocean);

            oldocW = ocW;
        }


	}

    private void OnDisable()
    {
        if (markedDirty) { 
            markedDirty = false;
            makeDirty(_ocean);
        }
    }

    void OnPlaymodeStateChange()
    {

        if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (markedDirty)
            {
                markedDirty = false;
                makeDirty(_ocean);
            }
        }
    }

    public static void makeDirty(Ocean ocean)
    {
        if(ocean == null) return;
        EditorUtility.SetDirty(ocean);
        if (!EditorApplication.isPlaying) EditorSceneManager.MarkSceneDirty(ocean.gameObject.scene);
    }

	private static void switchKeyword (Material mat, string keyword1, string keyword2, bool on){
		if(on) { mat.EnableKeyword(keyword1);  mat.DisableKeyword(keyword2); }
		 else { mat.EnableKeyword(keyword2);  mat.DisableKeyword(keyword1); }
	}

	private static void updcurPreset() {
		if(presets!= null) {
			if(presets.Length>0) {
				for(int i=0; i<presets.Length; i++) {
					if(presets[i] == title) { currentPreset = i; oldpreset = i; }
				}
			}
		}
	}

	private static void checkOceanWidth(Ocean ocean) {
		switch (ocean.width) {
			case 8:
			ocW = 0; ocean.width = ocean.height = 8; break;
			case 16:
			ocW = 1 ;ocean.width = ocean.height = 16; break;
			case 32:
			ocW = 2; ocean.width = ocean.height = 32; break;
			case 64:
			ocW = 3; ocean.width = ocean.height = 64; break;
			case 128:
			ocW = 4; ocean.width = ocean.height = 128; break;
            #if UNITY_2017_3_OR_NEWER
            case 256:
            ocW = 5; ocean.width = ocean.height = 256; break;
            #endif
        }
		oldocW = ocW;
	}

	private static void savePreset(Ocean ocean) {
		if (!Directory.Exists(presetPath)) Directory.CreateDirectory(presetPath);
		string preset = EditorUtility.SaveFilePanel("Save Ocean preset", presetPath,"","preset");

		if (preset != null) {
			if (preset.Length > 0) {
				title = Path.GetFileName(preset).Replace(".preset", ""); ocean._name = title;
				updcurPreset();
				using (BinaryWriter swr = new BinaryWriter(File.Open(preset, FileMode.Create))) {
					swr.Write(ocean.followMainCamera);//bool
					swr.Write(ocean.ifoamStrength);//float
					swr.Write(ocean.farLodOffset);//float
					swr.Write(ocean.tiles);//int
					swr.Write(ocean.size.x);//float
					swr.Write(ocean.size.y);//float
					swr.Write(ocean.size.z);//float
					swr.Write(ocean.width);//int
					swr.Write(ocean.height);//int
					swr.Write(ocean.fixedTiles);//bool
					swr.Write(0);//reserved
					swr.Write(0);//reserved
					swr.Write(ocean.scale);//float
					swr.Write(ocean.choppy_scale);//float
					swr.Write(ocean.speed);//float
					swr.Write(0f);//reserved
					swr.Write(ocean.wakeDistance);//float
					swr.Write(ocean.renderReflection);//bool
					swr.Write(ocean.renderRefraction);//bool
					swr.Write(ocean.renderTexWidth);//int
					swr.Write(ocean.renderTexHeight);//int
					swr.Write(ocean.m_ClipPlaneOffset);//float
					swr.Write(LayerMaskField(ocean.renderLayers));//int
					swr.Write(ocean.specularity);//float
					swr.Write(ocean.mistEnabled);//bool
					swr.Write(ocean.dynamicWaves);//bool
					swr.Write(ocean.humidity);//float
					swr.Write(ocean.pWindx);//float
					swr.Write(ocean.pWindy);//float
					swr.Write(ocean.waterColor.r);//float
					swr.Write(ocean.waterColor.g);//float
					swr.Write(ocean.waterColor.b);//float
					swr.Write(ocean.waterColor.a);//float
					swr.Write(ocean.surfaceColor.r);//float
					swr.Write(ocean.surfaceColor.g);//float
					swr.Write(ocean.surfaceColor.b);//float
					swr.Write(ocean.surfaceColor.a);//float
					swr.Write(ocean.foamFactor);//float
					swr.Write(ocean.spreadAlongFrames);//bool
					swr.Write(ocean.shaderLod);//bool
					swr.Write(ocean.everyXframe);//int
					swr.Write(ocean.useShaderLods);//bool
					swr.Write(ocean.numberLods);//int
					swr.Write(ocean.fakeWaterColor.r);//float
					swr.Write(ocean.fakeWaterColor.g);//float
					swr.Write(ocean.fakeWaterColor.b);//float
					swr.Write(ocean.fakeWaterColor.a);//float
					swr.Write(ocean.defaultLOD);//int

					swr.Write(ocean.reflrefrXframe);//int
					swr.Write(ocean.foamUV);//float
					swr.Write(ocean.sun.transform.localRotation.eulerAngles.x);//float
					swr.Write(ocean.sun.transform.localRotation.eulerAngles.y);//float
					swr.Write(ocean.sun.transform.localRotation.eulerAngles.z);//float
					swr.Write(ocean.bumpUV);//float
					swr.Write(ocean.ifoamWidth);//float
					swr.Write(ocean.lodSkipFrames);//int
					if(ocean.material) swr.Write(ocean.material.name);else swr.Write("");//string
					if(ocean.material1) swr.Write(ocean.material1.name);else swr.Write("");//string
					if(ocean.material2) swr.Write(ocean.material2.name);else swr.Write("");//string
					swr.Write(ocean.shaderAlpha);//float
					swr.Write(ocean.reflectivity);//float
					swr.Write(ocean.translucency);//float
					swr.Write(ocean.shoreDistance);//float
					swr.Write(ocean.shoreStrength);//float
					swr.Write(ocean.specPower);//float
					if(ocean.oceanShader) swr.Write(ocean.oceanShader.name);else swr.Write("");//string
					swr.Write(ocean.hasShore);//bool
					swr.Write(ocean.hasShore1);//bool
					swr.Write(ocean.hasFog);//bool
					swr.Write(ocean.hasFog1);//bool
					swr.Write(ocean.hasFog2);//bool
					swr.Write(ocean.distCan1);//bool
					swr.Write(ocean.distCan2);//bool
					swr.Write(ocean.cancellationDistance);//float
					swr.Write(ocean.foamDuration);//float
					swr.Write(ocean.sTilesLod);//int
					swr.Write(ocean.discSize);//int
					swr.Write(ocean.lowShaderLod);//int
					swr.Write(ocean.forceDepth);//bool
					swr.Write(ocean.waveDistanceFactor);//float

					swr.Write(ocean._gaussianMode == 1);//bool

					swr.Write(false);//reserved
					swr.Write(false);//reserved
					swr.Write(false);//reserved
					swr.Write(0);//reserved
					swr.Write(0);//reserved
					swr.Write(0f);//reserved
					swr.Write(0f);//reserved

					//if we should write a fixed gaussian random table
					if(ocean._gaussianMode == 1) {
						int len = ocean.width * ocean.height;
						if(ocean.gaussRandom1 != null && ocean.gaussRandom1.Length > 0 && len == ocean.gaussRandom1.Length) {
							swr.Write(len);
							for(int i = 0; i < len; i++) {
								swr.Write(ocean.gaussRandom1[i]);
								swr.Write(ocean.gaussRandom2[i]);
							}
						} else {
							swr.Write(0);
							Debug.Log("<color=yellow>Fixed Gaussian Table not saved. Save the preset in Play mode.</color>");
						}
					}
				}

			}
		}
	}

	private static int LayerMaskField(string label, int mask, params GUILayoutOption[] options) {
		List<string> layers = new List<string>();
		List<int> layerNumbers = new List<int>();

		string selectedLayers = "";

		for (int i = 0; i < 32; ++i) {
			string layerName = LayerMask.LayerToName(i);

			if (!string.IsNullOrEmpty(layerName)) {
				if (mask == (mask | (1 << i))) {
					if (string.IsNullOrEmpty(selectedLayers)) {
						selectedLayers = layerName;
					} else {
						selectedLayers = "Mixed";
					}
				}
			}
		}

		if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand) {
			if (mask == 0) {
				layers.Add("Nothing");
			} else if (mask == -1) {
				layers.Add("Everything");
			} else {
				layers.Add(selectedLayers);
			}
			layerNumbers.Add(-1);
		}

		layers.Add((mask == 0 ? "[+] " : "      ") + "Nothing");
		layerNumbers.Add(-2);

		layers.Add((mask == -1 ? "[+] " : "      ") + "Everything");
		layerNumbers.Add(-3);

		for (int i = 0; i < 32; ++i) {
			string layerName = LayerMask.LayerToName(i);

			if (layerName != "") {
				if (mask == (mask | (1 << i))) {
					layers.Add("[+] " + layerName);
				} else {
					layers.Add("      " + layerName);
				}
				layerNumbers.Add(i);
			}
		}

		bool preChange = GUI.changed;

		GUI.changed = false;

		int newSelected = 0;

		if (Event.current.type == EventType.MouseDown) {
			newSelected = -1;
		}

		if (string.IsNullOrEmpty(label)) {
			newSelected = EditorGUILayout.Popup(newSelected, layers.ToArray(), EditorStyles.layerMaskField, options);
		} else {
			newSelected = EditorGUILayout.Popup(label, newSelected, layers.ToArray(), EditorStyles.layerMaskField, options);
		}

		if (GUI.changed && newSelected >= 0) {
			if (newSelected == 0) {
				mask = 0;
			} else if (newSelected == 1) {
				mask = -1;
			} else {
				if (mask == (mask | (1 << layerNumbers[newSelected]))) {
					mask &= ~(1 << layerNumbers[newSelected]);
				} else {
					mask = mask | (1 << layerNumbers[newSelected]);
				}
			}
		} else {
			GUI.changed = preChange;
		}
		return mask;
	}

	private static int LayerMaskField(int mask, params GUILayoutOption[] options) {
		return LayerMaskField(null, mask, options);
	}

	private void DrawSeparator() {
		EditorGUILayout.BeginVertical();
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
		EditorGUILayout.EndVertical();
	}

private void DrawHalfSeparator(bool left = true) {
		EditorGUILayout.BeginVertical();
		GUILayout.Space(10f);

		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			float sw = 0f;
			float sw5 = Screen.width * 0.5f;
			if(!left) sw = sw5+3;
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(sw, rect.yMin + 6f, sw5, 2f), tex);
			GUI.DrawTexture(new Rect(sw, rect.yMin + 6f, sw5, 1f), tex);
			GUI.DrawTexture(new Rect(sw, rect.yMin + 8f, sw5, 1f), tex);
			GUI.color = Color.white;
		}
		EditorGUILayout.EndVertical();
	}


	private void DrawVerticalSeparator() {
		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			float sw5 = Screen.width * 0.5f;
			GUI.DrawTexture(new Rect(sw5, rect.yMin + 10f, 4f, 950), tex);
			GUI.DrawTexture(new Rect(sw5, rect.yMin + 10f, 1f, 950), tex);
			GUI.DrawTexture(new Rect(sw5 + 3f, rect.yMin + 10f, 1f, 950), tex);
			GUI.color = Color.white;
		}
	}

	private void DrawBackground() {
		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = back;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 950), tex);
			GUI.color = Color.white;
		}
	}

}
#endif
