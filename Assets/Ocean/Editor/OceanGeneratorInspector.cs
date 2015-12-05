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
	public static bool oldrefr, prevrefr;


	private string[] defShader = {"default lod","1","2","3","4"};


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

	void OnEnable() {

		//Load dynamic resources
		string pluginPath = FilePathToAssetPath( GetPluginPath() );

		/*
		string logoPath = Path.Combine(pluginPath, "Editor");
		logoPath = Path.Combine(logoPath, "OceanBanner.png");
		logo = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);
		if (null == logo) Debug.LogError("null == logo");
		*/

		string backPath = Path.Combine(pluginPath, "Editor");
		backPath = Path.Combine(backPath, "Background.png");

		back = AssetDatabase.LoadAssetAtPath<Texture2D>(backPath);

		if (null == back) Debug.LogError("null == back");
	}


	public override void OnInspectorGUI() {

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		DrawBackground();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
		GUI.contentColor = new Color(1f, 1f, 1f, 1f);

		EditorGUIUtility.labelWidth = 80F;// LookLikeControls(80f);
		Ocean ocean = target as Ocean;

		DrawSeparator();

		GUILayout.Space(-15);

		EditorGUILayout.BeginHorizontal();
		//float bannerWidth = 256;
		GUILayout.Space(Screen.width * 0.65f);
		// EditorGUILayout.LabelField(new GUIContent( logo ), GUILayout.Width( bannerWidth), GUILayout.Height( bannerWidth * 0.40f ));
		EditorGUILayout.EndHorizontal();

		//GUILayout.Space(-20);

		DrawSeparator();

		DrawVerticalSeparator();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.BeginVertical();

		EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Targets");
		GUILayout.Space(12);
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("target"); 
		GUILayout.Space(-100);
		ocean.player = (Transform)EditorGUILayout.ObjectField(ocean.player, typeof(Transform), true, GUILayout.MinWidth(150));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Follow");
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

		EditorGUILayout.LabelField("Ocean shader");
		ocean.oceanShader = (Shader)EditorGUILayout.ObjectField(ocean.oceanShader, typeof(Shader), true);

		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Interactive foam strength");
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		ocean.ifoamStrength = (float)EditorGUILayout.Slider(ocean.ifoamStrength, 0, 50);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Interactive foam strength/duaration.","The strength/duration of the boat that interacts with the ocean.","OK");
		}
		
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-2);

		//GUILayout.Space(4);
		EditorGUILayout.LabelField("Far LOD Y-offset");
		EditorGUILayout.BeginHorizontal();
		ocean.farLodOffset = (float)EditorGUILayout.Slider(ocean.farLodOffset, -30f, 0);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Far lod tiles offset","This value will offset the far lod tiles (after the 2nd) to a lower level to eliminate noticable gaps between lod borders.\n\n"+
			"Each extra lod (after the 2nd) will get lowered by 1/3 of the entered value.","OK");
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-2);
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
		EditorGUILayout.LabelField("Enable Mist");
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


		GUILayout.Space(22);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Spread along frames");
		ocean.spreadAlongFrames = EditorGUILayout.Toggle(ocean.spreadAlongFrames);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Enable spread along frames.","This will enable/disable the work offload of the wave calculations by spreading the calculations among x frames declared by the slider.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Default shader lod");
		//ocean.defaultLOD = EditorGUILayout.IntField(ocean.defaultLOD);
		GUILayout.Space(-22);
		ocean.defaultLOD = EditorGUILayout.Popup(ocean.defaultLOD, defShader,GUILayout.MaxWidth(40));
		if(ocean.defaultLOD==0) ocean.defaultLOD = 1;
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Default shader Quality.","Decalre the shader lod(quality) that is getting loaded by default when the simulation starts.\n\n"+
			"4: high quality. (reflection/reflection/wave bump/foam bump/foam)\n\n3: medium quality. (wave bump/foam bump/foam)\n\n2: low quality. (wave bump)\n\n1: medium quality-transparent variation. (wave bump/foam bump/foam)","OK");
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

		GUILayout.Space(13);
		if (GUILayout.Button("Load preset")) {
			string preset = EditorUtility.OpenFilePanel("Load Ocean preset",Application.dataPath+"/Ocean/Editor/_OceanPresets","preset");

			if (preset != null) {
				if (preset.Length > 0) {
					ocean.loadPreset(preset);
					title = Path.GetFileName(preset).Replace(".preset", ""); ocean._name = title;
				}
			}
		}

		GUILayout.Space(8);
		EditorGUILayout.LabelField("Last loaded/saved: " + ocean._name, GUILayout.MinWidth(170));

		EditorGUILayout.EndVertical();




		//=======================================================================================================================





		GUILayout.FlexibleSpace();
		GUILayout.Space(20);
		GUILayout.FlexibleSpace();
		EditorGUILayout.BeginVertical();

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
		EditorGUILayout.LabelField("Fixed tiles");
		ocean.fixedTiles = EditorGUILayout.Toggle(ocean.fixedTiles);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Fixed tiles distance");
		ocean.fTilesDistance = (int)EditorGUILayout.Slider(ocean.fTilesDistance, 1, 5);

		EditorGUILayout.LabelField("Fixed tiles lod");
		ocean.fTilesLod = (int)EditorGUILayout.Slider(ocean.fTilesLod, 0, 5);

		////------------------------------------------
		GUILayout.Space(12);

		EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Reflection & Refraction");
		GUILayout.Space(16);
		EditorGUILayout.EndVertical();
		GUILayout.Space(10);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Render reflection");
		if (!ocean.shaderLod) ocean.renderReflection = EditorGUILayout.Toggle(ocean.renderReflection);
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(4);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Render refraction");
		if (!ocean.shaderLod) ocean.renderRefraction = EditorGUILayout.Toggle(ocean.renderRefraction);
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(6);
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

		EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Sun reflection");
		GUILayout.Space(18);
		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField("Sun transform");
		ocean.sun = (Transform)EditorGUILayout.ObjectField(ocean.sun, typeof(Transform), true);

		EditorGUILayout.LabelField("Sun direction");
		ocean.SunDir = EditorGUILayout.Vector3Field("", ocean.SunDir);

		////------------------------------------------
		GUILayout.Space(18);

		EditorGUI.DropShadowLabel(EditorGUILayout.BeginVertical(), "Water color");
		GUILayout.Space(16);
		EditorGUILayout.EndVertical();

		GUI.backgroundColor = Color.white;
		GUI.contentColor = Color.white;

		EditorGUILayout.LabelField("Water color");
		ocean.waterColor = EditorGUILayout.ColorField(ocean.waterColor);

		EditorGUILayout.LabelField("Water surface color");
		ocean.surfaceColor = EditorGUILayout.ColorField(ocean.surfaceColor);

		GUILayout.Space(5);
		EditorGUILayout.LabelField("Fake water color(lods)");
		ocean.fakeWaterColor = EditorGUILayout.ColorField(ocean.fakeWaterColor);
		GUI.backgroundColor = new Color(.5f, .8f, 1f, 1f);
		GUI.contentColor = new Color(1f, 1f, 1f, 1f);

		GUILayout.Space(10);
		DrawSeparator();

		if (ocean.spreadAlongFrames) {
			EditorGUILayout.LabelField("Calc waves every x frames:");
			EditorGUILayout.BeginHorizontal();

			ocean.everyXframe = (int)EditorGUILayout.Slider(ocean.everyXframe, 3, 8);
			if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
				EditorUtility.DisplayDialog("Calculate waves every x frames.","This will spread the calculation of the waves along the frames you decalre.\n\n"+
				"When you use Vsyc for frame limitting the recommended values are 3-5. Otherwise you will notice jerky wave movement.","OK");
			}
			if (oldEveryXframe != ocean.everyXframe) {
				oldEveryXframe = ocean.everyXframe;
				if (ocean.everyXframe > 3) { ocean.fr1 = 0; ocean.fr2 = 1; ocean.fr3 = 2; ocean.fr4 = 3; } else { ocean.fr1 = 0; ocean.fr2 = 1; ocean.fr3 = 2; ocean.fr4 = 2; }
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

		DrawSeparator();

		if (GUILayout.Button("Save preset")) {
			if (!Directory.Exists(Application.dataPath + "/Ocean/Editor/_OceanPresets")) Directory.CreateDirectory(Application.dataPath + "/Ocean/Editor/_OceanPresets");
			string preset = EditorUtility.SaveFilePanel("Save Ocean preset", Application.dataPath+"/Ocean/Editor/_OceanPresets","","preset");

			if (preset != null) {
				if (preset.Length > 0) {
					title = Path.GetFileName(preset).Replace(".preset", ""); ocean._name = title;
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
					}

				}
			}

		}
		GUILayout.Space(26);
		// EditorGUILayout.LabelField("Editor script by 'MindBlocks Studio'", GUILayout.MinWidth(170));

		EditorGUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		DrawSeparator();



		if (ocean.waterColor != oldWaterColor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_WaterColor", ocean.waterColor);  }   oldWaterColor = ocean.waterColor; }
		if (ocean.surfaceColor != oldsurfaceColor) {  for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_SurfaceColor", ocean.surfaceColor); } oldsurfaceColor = ocean.surfaceColor; }
		if (ocean.fakeWaterColor != oldFakeWaterColor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetColor("_FakeUnderwaterColor", ocean.fakeWaterColor); } oldFakeWaterColor = ocean.fakeWaterColor; }

		if (ocean.foamFactor != oldFoamFactor) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_FoamFactor", ocean.foamFactor); }  oldFoamFactor = ocean.foamFactor; }
		if (ocean.specularity != oldSpecularity) { for(int i=0; i<3; i++) { if(ocean.mat[i]!= null) ocean.mat[i].SetFloat("_Specularity", ocean.specularity); } oldSpecularity = ocean.specularity; }

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

		if (GUI.changed) {
			EditorUtility.SetDirty(ocean);
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
			GUI.DrawTexture(new Rect(Screen.width * 0.5f, rect.yMin + 10f, 4f, 695f), tex);
			GUI.DrawTexture(new Rect(Screen.width * 0.5f, rect.yMin + 10f, 1f, 695f), tex);
			GUI.DrawTexture(new Rect(Screen.width * 0.5f + 3f, rect.yMin + 10f, 1f, 695), tex);
			GUI.color = Color.white;
		}
	}

	public void DrawBackground() {
		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = back;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 812), tex);
			GUI.color = Color.white;
		}
	}


}
#endif
