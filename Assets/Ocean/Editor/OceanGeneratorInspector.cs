using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
[CustomEditor(typeof(Ocean))]
public class OceanGeneratorInspector : Editor {

	static public Texture2D blankTexture {
		get {
			return EditorGUIUtility.whiteTexture;
		}
	}

	//private Texture2D logo;
	private Texture2D back;
	public static string title;

	public static int oldEveryXframe;

	public static Color oldWaterColor;
	public static Color oldsurfaceColor;
	public static Color oldFakeWaterColor;
	public static float oldFoamFactor;
	public static float oldSpecularity;
	public static bool oldShaderLod;
	public static bool oldrefl, prevrefl;
	public static bool oldrefr, prevrefr, oldSpread, oldFixed;
	public static float oldFoamUV, oldBumpUV;
	public static Vector3 oldSunDir;
	public static int oldRefrReflXframe;
	public static string presetPath;
	public static int currentPreset, oldpreset, oldmodeset;

	private string[] defShader = {"default lod","1","2","3","4"};
	private string[] skiplods = {"off","1","2","3","4"};
	private string[] mode = {"Mobile Setting","Desktop Setting"};
	public static string[] presets, presetpaths;

	public static int editormode = 0;


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
				if(!dp[i].Contains(".meta")&& dp[i].Contains(".preset")) {k++;}
			}

			presets=null; presetpaths=null;
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
		oldmodeset = ocean._mode;

		presetPath = Application.dataPath+"/Ocean/Editor/_OceanPresets";

		checkPdir(ocean);

		//Load dynamic resources
		string pluginPath = FilePathToAssetPath( GetPluginPath() );


		string backPath = Path.Combine(pluginPath, "Editor");
		backPath = Path.Combine(backPath, "Background.png");

		back = AssetDatabase.LoadAssetAtPath<Texture2D>(backPath);

		if (null == back) Debug.LogError("null == back");
	}


	public override void OnInspectorGUI() {

		Ocean ocean = target as Ocean;


		GUILayout.Space(8);
		EditorGUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Ocean Classic")) { editormode=0;}
		if (GUILayout.Button("Ocean Grid")) {editormode=1;}
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
			ocean._mode = EditorGUILayout.Popup(ocean._mode, mode);

			GUILayout.Space(4);
			GUI.backgroundColor = new Color(.7f, .8f, 1f, 1f);
			currentPreset = EditorGUILayout.Popup(currentPreset, presets);
			if(oldpreset != currentPreset ) {
				if(currentPreset==0) { currentPreset = oldpreset; return; }
				if(ocean.loadPreset(presetpaths[currentPreset-1])) {oldpreset = currentPreset; ocean._name = presets[currentPreset]; } else {currentPreset = oldpreset; checkPdir(ocean);}
			}
			GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
			EditorGUILayout.EndVertical();

			GUILayout.Space(8);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Target"); 
			GUILayout.Space(-100);
			ocean.player = (Transform)EditorGUILayout.ObjectField(ocean.player, typeof(Transform), true, GUILayout.MinWidth(150));
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
			EditorGUILayout.LabelField("Sun: ",GUILayout.MaxWidth(50));
			ocean.sun = (Transform)EditorGUILayout.ObjectField(ocean.sun, typeof(Transform), true, GUILayout.MinWidth(140));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Load Sun",GUILayout.MaxWidth(59));
			ocean.loadSun = EditorGUILayout.Toggle(ocean.loadSun,GUILayout.MaxWidth(79));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Far LOD Y-offset");
			EditorGUILayout.BeginHorizontal();
			ocean.farLodOffset = (float)EditorGUILayout.Slider(ocean.farLodOffset, -30f, 0);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Far lod tiles offset","This value will offset the far lod tiles (after the 2nd) to a lower level to eliminate noticable gaps between lod borders.\n\n"+
				"Each extra lod (after the 2nd) will get lowered by 1/3 of the entered value.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();


			//GUILayout.Space(4);

			GUILayout.Space(4);
			DrawSeparator();

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Waves settings");
			GUILayout.Space(14);
			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("Scale");
			ocean.scale = (float)EditorGUILayout.Slider(ocean.scale, 0, 100);

			EditorGUILayout.LabelField("Choppy scale");
			ocean.choppy_scale = (float)EditorGUILayout.Slider(ocean.choppy_scale, 0, 100);

			EditorGUILayout.LabelField("Waves speed");
			ocean.speed = (float)EditorGUILayout.Slider(ocean.speed, 0.1f, 3f);

			EditorGUILayout.LabelField("Waves Offset animation speed");
			ocean.waveSpeed = (float)EditorGUILayout.Slider(ocean.waveSpeed, 0.01f, 4f);

			EditorGUILayout.LabelField("Wake distance");
			ocean.wakeDistance = (float)EditorGUILayout.Slider(ocean.wakeDistance, 1f, 15f);
			GUILayout.Space(6);
			DrawSeparator();

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Mist");
			GUILayout.Space(10);
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

			DrawSeparator();

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Wind");
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Dynamic waves");

			ocean.dynamicWaves = EditorGUILayout.Toggle(ocean.dynamicWaves);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Wind power");
			ocean.humidity = (float)EditorGUILayout.Slider(ocean.humidity, 0.01f, 1f);

			EditorGUILayout.LabelField("Wind direction");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("X");
			GUILayout.Space(-100);
			ocean.pWindx = EditorGUILayout.FloatField(ocean.pWindx);
			EditorGUILayout.LabelField("Y");
			GUILayout.Space(-100);
			ocean.pWindy = EditorGUILayout.FloatField(ocean.pWindy);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Foam Strength");
			ocean.foamFactor = (float)EditorGUILayout.Slider(ocean.foamFactor, 0f, 3.0f);


			GUILayout.Space(24);
			if(!ocean.fixedUpdate) {
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spread along frames");
				ocean.spreadAlongFrames = EditorGUILayout.Toggle(ocean.spreadAlongFrames);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Enable spread along frames.","This will enable/disable the work offload of the wave calculations by spreading the calculations among x frames declared by the slider.","OK");
				}
			EditorGUILayout.EndHorizontal();
			} else {
				EditorGUILayout.LabelField(""); 
				GUILayout.Space(2);
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Default shader lod");
			GUILayout.Space(-22);
			ocean.defaultLOD = EditorGUILayout.Popup(ocean.defaultLOD, defShader,GUILayout.MaxWidth(40));
			if(ocean.defaultLOD==0) ocean.defaultLOD = 1;
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Default shader Quality.","Decalre the shader lod(quality) that is getting loaded by default when the simulation starts.\n\n"+
				"4: high quality. (reflection/reflection/wave bump/foam bump/foam)\n\n3: medium quality. (wave bump/foam)\n\n2: low quality. (wave bump)\n\n1: medium quality-transparent variation. (wave bump/foam bump/foam)\n\n"+
				"For older mobile devices the medium quality shader is recommended (3)","OK");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Shader Low lod");
			ocean.shaderLod = EditorGUILayout.Toggle(ocean.shaderLod);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Enable/disable low quality shader.","This will enable/disable the low quality shader lod using the number decalard by the slider (No. of shader lods).\n\n"+
				"You can switch to the desired lower quality shader lod by code, using the shader_LOD function in the Ocean.cs script.","OK");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Enable shader lods");
			ocean.useShaderLods = EditorGUILayout.Toggle(ocean.useShaderLods);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Enable/disable of shader lods on tiles.","This will enable/disable shader lod on tiles with higher lod.\n\n"+
				"You can set the number of lods that will be used with the slider: No. of shader lods.","OK");		
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(8);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Use Fixed Update");
			ocean.fixedUpdate = EditorGUILayout.Toggle(ocean.fixedUpdate);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Use FixedUpdate()","Enable/disable the use of the FixedUpdate() function for calculation of the waves.\n\n"+
				"This gives higher frame rates but disables the spread along frames functionality. It is a good alternative when you get jerky waves with vsync on and almost doubles the frame rate compared to regular Update().","OK");		
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(14);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Load preset")) {
				string preset = EditorUtility.OpenFilePanel("Load Ocean preset",presetPath,"preset");

				if (preset != null) {
					if (preset.Length > 0) {
						ocean.loadPreset(preset);
						title = Path.GetFileName(preset).Replace(".preset", ""); ocean._name = title;
						updcurPreset();
						EditorUtility.SetDirty(ocean);
					}
				}
			}

			if (GUILayout.Button("Save preset")) {
				savePreset(ocean);
				checkPdir(ocean);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();




			//=======================================================================================================================


			GUILayout.FlexibleSpace();
			GUILayout.Space(20);
			GUILayout.FlexibleSpace();

			EditorGUILayout.BeginVertical();
			//GUILayout.Space(20);
			

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Tiles settings");
			GUILayout.Space(14);
			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("Tiles count");
			ocean.tiles = (int)EditorGUILayout.Slider(ocean.tiles, 1, 15);
			EditorGUILayout.LabelField("Tiles size");
			ocean.size = EditorGUILayout.Vector3Field("", ocean.size);

			EditorGUILayout.LabelField("Tiles poly count");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Width");
			GUILayout.Space(-90);
			ocean.width = EditorGUILayout.IntField(ocean.width);
			EditorGUILayout.LabelField("Height");
			GUILayout.Space(-90);
			ocean.height = EditorGUILayout.IntField(ocean.height);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Fixed tiles", GUILayout.MaxWidth(65));
			ocean.fixedTiles = EditorGUILayout.Toggle(ocean.fixedTiles);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Fixed tiles distance");
			ocean.fTilesDistance = (int)EditorGUILayout.Slider(ocean.fTilesDistance, 1, 5);

			EditorGUILayout.LabelField("Fixed tiles lod");
			ocean.fTilesLod = (int)EditorGUILayout.Slider(ocean.fTilesLod, 0, 5);

			////------------------------------------------
			GUILayout.Space(30);

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Reflection & Refraction");
			GUILayout.Space(14);
			EditorGUILayout.EndVertical();
			GUILayout.Space(6);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reflection",GUILayout.MaxWidth(59));
			if (!ocean.shaderLod) ocean.renderReflection = EditorGUILayout.Toggle(ocean.renderReflection,GUILayout.MaxWidth(79));
			EditorGUILayout.LabelField("Refraction",GUILayout.MaxWidth(64));
			if (!ocean.shaderLod) ocean.renderRefraction = EditorGUILayout.Toggle(ocean.renderRefraction,GUILayout.MaxWidth(79));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Every x frames:",GUILayout.MinWidth(120));
			ocean.reflrefrXframe = EditorGUILayout.IntField(ocean.reflrefrXframe,GUILayout.MaxWidth(39));
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Render Reflection/Refraction every x frames.","This will render the reflection and refraction camera every x frames you declare.\n\n"+
				"Since reflection and refraction are not easy for the eye to catch their changes, we can update them every x frames to gain performance.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.LabelField("Render textures size");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Width");
			GUILayout.Space(-90);
			ocean.renderTexWidth = EditorGUILayout.IntField(ocean.renderTexWidth);
			EditorGUILayout.LabelField("Height");
			GUILayout.Space(-90);
			ocean.renderTexHeight = EditorGUILayout.IntField(ocean.renderTexHeight);
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(8);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reflection clip plane offset");
			ocean.m_ClipPlaneOffset = EditorGUILayout.FloatField(ocean.m_ClipPlaneOffset, GUILayout.MaxWidth(79));
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(8);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Render layers");
			GUILayout.Space(-40);
			int mask = LayerMaskField(ocean.renderLayers);

			if (ocean.renderLayers != mask) {
				ocean.renderLayers = mask;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(4);
				
			EditorGUILayout.LabelField("Specularity");
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			ocean.specularity = (float)EditorGUILayout.Slider(ocean.specularity, 0.01f, 1.0f);
			EditorGUILayout.EndHorizontal();

			////------------------------------------------
			GUILayout.Space(4);

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Interactive Foam");
			GUILayout.Space(18);
			EditorGUILayout.EndVertical();

		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Interactive foam strength");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			ocean.ifoamStrength = (float)EditorGUILayout.Slider(ocean.ifoamStrength, 0, 50);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Interactive foam strength/duaration.","The strength/duration of the foam produced by the boat that interacts with the ocean.","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Interactive foam width");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			ocean.ifoamWidth = (float)EditorGUILayout.Slider(ocean.ifoamWidth, 2, 0.1f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(-1);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Interactive foam width.","The width of the boat's trail foam. (Lower values produce higher width).","OK");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();




			////------------------------------------------
			GUILayout.Space(10);

			EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Water color");
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();

			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Water color",GUILayout.MaxWidth(90));
			ocean.waterColor = EditorGUILayout.ColorField(ocean.waterColor);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Surface color",GUILayout.MaxWidth(90));
			ocean.surfaceColor = EditorGUILayout.ColorField(ocean.surfaceColor);
			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Lods Refr color",GUILayout.MaxWidth(90));
			ocean.fakeWaterColor = EditorGUILayout.ColorField(ocean.fakeWaterColor);
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
			GUI.contentColor = new Color(1f, 1f, 1f, 1f);
			GUILayout.Space(6);

			GUILayout.Space(15);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Foam tiling",GUILayout.MaxWidth(70));
			ocean.foamUV = (int)EditorGUILayout.Slider(ocean.foamUV, 1, 8);
			EditorGUILayout.EndHorizontal();
		
			GUILayout.Space(15);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Bump tiling",GUILayout.MaxWidth(70));
			ocean.bumpUV = (int)EditorGUILayout.Slider(ocean.bumpUV, 1, 8);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			DrawSeparator();

			if (ocean.spreadAlongFrames && !ocean.fixedUpdate) {
				EditorGUILayout.LabelField("Calc waves every x frames:");
				EditorGUILayout.BeginHorizontal();

				ocean.everyXframe = (int)EditorGUILayout.Slider(ocean.everyXframe, 2, 8);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Calculate waves every x frames.","This will spread the calculation of the waves along the frames you decalre.\n\n"+
					"When you use Vsyc for frame limitting the recommended values are 2-4. Otherwise you will notice jerky wave movement.\n\nYou might want to consider using the fixedUpdate flag where"+
					" the waves are getting updated in the FixedUpdate function. This will give better framerates when vsync is used. When fixedUpdate is used the spread along frames function gets disabled.","OK");
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

			if(ocean.useShaderLods || ocean.shaderLod) {
				EditorGUILayout.LabelField("No. of shader lods");
				EditorGUILayout.BeginHorizontal();
				ocean.numberLods = (int)EditorGUILayout.Slider(ocean.numberLods, 1, 3);
				if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
					EditorUtility.DisplayDialog("Number of shader lods","How many shader lods will be used for the mesh lods.\n\n The current number of the shader lods is used also as the low shader lod when switching between"+
					"the high and low quality shader for the main (lod0) material of the ocean.\n\n1: Only the main shader will be used on all tiles.\n\n2: The main shader and the 2nd lod level will be used.\n\n"+
					"3: The main shader and the 2nd and the 3rd lod levels will be used.","OK");
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

			DrawSeparator();

			GUILayout.Space(50);
			// EditorGUILayout.LabelField("Editor script by 'MindBlocks Studio'", GUILayout.MinWidth(170));

			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();


			GUILayout.FlexibleSpace();


			DrawSeparator();
		}



		if (ocean.waterColor != oldWaterColor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_WaterColor", ocean.waterColor);  }   oldWaterColor = ocean.waterColor; }
		if (ocean.surfaceColor != oldsurfaceColor) {  for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_SurfaceColor", ocean.surfaceColor); } oldsurfaceColor = ocean.surfaceColor; }
		if (ocean.fakeWaterColor != oldFakeWaterColor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_FakeUnderwaterColor", ocean.fakeWaterColor); } oldFakeWaterColor = ocean.fakeWaterColor; }

		if (ocean.foamFactor != oldFoamFactor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_FoamFactor", ocean.foamFactor); }  oldFoamFactor = ocean.foamFactor; }
		if (ocean.specularity != oldSpecularity) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_Specularity", ocean.specularity); } oldSpecularity = ocean.specularity; }

		if (ocean.foamUV != oldFoamUV) { for(int i=0; i<2; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_FoamSize", ocean.foamUV); } oldFoamUV = ocean.foamUV; }
		if (ocean.bumpUV != oldBumpUV) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_Size", 0.015625f * ocean.bumpUV); } oldBumpUV = ocean.bumpUV; }

		if (ocean.reflrefrXframe != oldRefrReflXframe) { if(ocean.reflrefrXframe<=0) ocean.reflrefrXframe = 1; oldRefrReflXframe = ocean.reflrefrXframe; }
		if (oldFixed != ocean.fixedUpdate) {
			if(ocean.fixedUpdate) { oldSpread = ocean.spreadAlongFrames;  } 
			if(!ocean.fixedUpdate) {ocean.spreadAlongFrames = oldSpread; }
			oldFixed = ocean.fixedUpdate;
		 }

		if(oldSunDir != ocean.sun.transform.forward) {
			ocean.SunDir = ocean.sun.transform.forward;
			oldSunDir = ocean.SunDir;
		}

		if (ocean.renderReflection != oldrefl) { ocean.EnableReflection(ocean.renderReflection); oldrefl = ocean.renderReflection; }
		if (ocean.renderRefraction != oldrefr) { ocean.EnableRefraction(ocean.renderRefraction); oldrefr = ocean.renderRefraction; }

		if (ocean.shaderLod != oldShaderLod) {
			if (ocean.shaderLod) {
				prevrefl = ocean.renderReflection;
				prevrefr = ocean.renderRefraction;
			}

			ocean.shader_LOD(!ocean.shaderLod, ocean.material, ocean.numberLods);
			oldShaderLod = ocean.shaderLod;

			if (!ocean.shaderLod) {
				ocean.EnableReflection(prevrefl); ocean.EnableRefraction(prevrefr);
				oldrefl = ocean.renderReflection; oldrefr = ocean.renderRefraction;
			}
		}

		//switch between mobile and desktop settings
		if(oldmodeset != ocean._mode) {
			oldmodeset = ocean._mode;
		}

		if (GUI.changed) {
			EditorUtility.SetDirty(ocean);
		}

	}

	public static void updcurPreset() {
		if(presets!= null) {
			if(presets.Length>0) {
				for(int i=0; i<presets.Length; i++) {
					if(presets[i] == title) { currentPreset = i; oldpreset = i; }
				}
			}
		}
	}

	public static void savePreset(Ocean ocean) {
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
					swr.Write(ocean.fTilesDistance);//int
					swr.Write(ocean.fTilesLod);//int
					swr.Write(ocean.scale);//float
					swr.Write(ocean.choppy_scale);//float
					swr.Write(ocean.speed);//float
					swr.Write(ocean.waveSpeed);//float
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
				}

			}
		}
	}

	public static int LayerMaskField(string label, int mask, params GUILayoutOption[] options) {
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

	public static int LayerMaskField(int mask, params GUILayoutOption[] options) {
		return LayerMaskField(null, mask, options);
	}

	public void DrawSeparator() {
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

	public void DrawVerticalSeparator() {
		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(Screen.width * 0.5f, rect.yMin + 10f, 4f, Screen.height-26), tex);
			GUI.DrawTexture(new Rect(Screen.width * 0.5f, rect.yMin + 10f, 1f, Screen.height-26), tex);
			GUI.DrawTexture(new Rect(Screen.width * 0.5f + 3f, rect.yMin + 10f, 1f, Screen.height-26), tex);
			GUI.color = Color.white;
		}
	}

	public void DrawBackground() {
		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = back;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, Screen.height-24), tex);
			GUI.color = Color.white;
		}
	}


}
#endif
