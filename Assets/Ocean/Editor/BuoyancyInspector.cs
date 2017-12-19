using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
[CustomEditor(typeof(Buoyancy))]
[System.Serializable]
public class BuoyancyInspector  : Editor{

	static public Texture2D blankTexture {
		get {
			return EditorGUIUtility.whiteTexture;
		}
	}

	public static Rigidbody rrig;
	public static BoxCollider col;

	public static string presetPath;

	


	void OnEnable () {
		Buoyancy boyancy = target as Buoyancy;

		var script = MonoScript.FromScriptableObject( this );
		presetPath = Path.GetDirectoryName( AssetDatabase.GetAssetPath( script ))+"/_OceanPresets";

		rrig = boyancy.GetComponent<Rigidbody>();
		if(rrig== null) Debug.Log("Object requires a Rigidbody");

		presetPath = Application.dataPath+"/Ocean/Editor/_OceanPresets";

		col = boyancy.GetComponent<BoxCollider>();
		if(col == null) 	Debug.Log("Object requires a Box Collider");

	}


	public override void OnInspectorGUI() {

		Buoyancy buoyancy = target as Buoyancy;
		DrawSeparator();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Render Queue", GUILayout.MaxWidth(130));
		buoyancy.renderQueue = EditorGUILayout.IntField(buoyancy.renderQueue);
		GUILayout.Space(175);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Material Render Queue","Set the object's material render queue to something that suits you. Useful for not showing shore lines under boat.","OK");
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("More Accurate", GUILayout.MaxWidth(130));
		buoyancy.moreAccurate = EditorGUILayout.Toggle(buoyancy.moreAccurate);
		GUILayout.Space(175);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("More accurate Calculation","If a more accurate function of the Water height function should be used.\n\nIt is however 2.5x times slower.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Use FixedUpdate", GUILayout.MaxWidth(130));
		buoyancy.useFixedUpdate = EditorGUILayout.Toggle(buoyancy.useFixedUpdate);
		GUILayout.Space(175);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Use FixedUpdate()","If this object should be simulated in the FixedUpdate function. Can be better timed but it is more accurate if unchecked.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Buoyancy");
		GUILayout.Space(-100);
		buoyancy.magnitude = EditorGUILayout.Slider(buoyancy.magnitude, 0, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Buoyancy magnitude","The amount of the buoyant forces applied to this vessel.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Y offset");
		GUILayout.Space(-100);
		buoyancy.ypos = EditorGUILayout.Slider(buoyancy.ypos, -30f, 30f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Y offset","How many units the boat will float above the calculated position.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Center of Mass");
		GUILayout.Space(-100);
		buoyancy.CenterOfMassOffset = EditorGUILayout.Slider(buoyancy.CenterOfMassOffset, -20f, 20f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Center of Mass offset","Offsets the height of the center of mass of the rigidbody.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Damp Coeff");
		GUILayout.Space(-100);
		buoyancy.dampCoeff = EditorGUILayout.Slider(buoyancy.dampCoeff, 0f, 2f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Damp Coefficient","The damp coefficient of the buoyancy.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slices X");
		GUILayout.Space(-100);
		buoyancy.SlicesX = (int)EditorGUILayout.Slider(buoyancy.SlicesX, 2, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slices X dimension","The slicing of the bounds of the box collider in the X dimension.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slices Z");
		GUILayout.Space(-100);
		buoyancy.SlicesZ = (int)EditorGUILayout.Slider(buoyancy.SlicesZ, 2, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slices Z dimension","The slicing of the bounds of the box collider in the Z dimension.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Interpolation");
		GUILayout.Space(-100);
		buoyancy.interpolation = (int)EditorGUILayout.Slider(buoyancy.interpolation, 0, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Buoyancy Interpolation","How many cycles will be used to average/interpolate the final buoynacy.\n\nKeep this as small as you can since it adds overhead.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);
		DrawSeparator();
		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Choppyness influence", GUILayout.MaxWidth(130));
		buoyancy.ChoppynessAffectsPosition = EditorGUILayout.Toggle(buoyancy.ChoppynessAffectsPosition);
		GUILayout.Space(175);
		EditorGUILayout.LabelField("ifIsVisible", GUILayout.MaxWidth(60));
		buoyancy.cvisible = EditorGUILayout.Toggle(buoyancy.cvisible);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Choppyness affects boat","If the choppyness of the waves affect the boat's position and rotation.\n\nThis will be skipped if the boat has reached a high speed.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Choppyness factor", GUILayout.MinWidth(100));
		GUILayout.Space(-80);
		buoyancy.ChoppynessFactor = EditorGUILayout.Slider(buoyancy.ChoppynessFactor, 0, 10f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Choppyness Factor","The amount of choppyness that will influence the boat.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Wind affects boat", GUILayout.MaxWidth(130));
		buoyancy.WindAffectsPosition = EditorGUILayout.Toggle(buoyancy.WindAffectsPosition);
		GUILayout.Space(175);
		EditorGUILayout.LabelField("ifIsVisible", GUILayout.MaxWidth(60));
		buoyancy.wvisible = EditorGUILayout.Toggle(buoyancy.wvisible);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Wind affects boat","If the Ocean wind affect the boat's position and rotation.\n\nThis will be skipped if the boat has reached a high speed.\n\n"+
			"If ifIsVisible is checked the calculations will take place only if the objects renderer is visible.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Wind factor", GUILayout.MinWidth(100));
		GUILayout.Space(-80);
		buoyancy.WindFactor = EditorGUILayout.Slider(buoyancy.WindFactor, 0, 5f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Wind Factor","The amount of wind that will influence the boat.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slope sliding",  GUILayout.MaxWidth(130));
		buoyancy.xAngleAddsSliding = EditorGUILayout.Toggle(buoyancy.xAngleAddsSliding);
		GUILayout.Space(175);
		EditorGUILayout.LabelField("ifIsVisible", GUILayout.MaxWidth(60));
		buoyancy.svisible = EditorGUILayout.Toggle(buoyancy.svisible);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slope sliding","If the boat faces stormy waves, sliding forces will be applied to it.\n\n"+
			"If ifIsVisible is checked the calculations will take place only if the objects renderer is visible.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slide factor", GUILayout.MinWidth(100));
		GUILayout.Space(-80);
		buoyancy.slideFactor = EditorGUILayout.Slider(buoyancy.slideFactor, 0, 10f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slope sliding Factor","The amount of sliding force applied to the boat.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(10);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Visible Renderer", GUILayout.MinWidth(130));
		GUILayout.Space(-150);
		buoyancy._renderer = (Renderer)EditorGUILayout.ObjectField(buoyancy._renderer, typeof(Renderer), true, GUILayout.MinWidth(120));
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Renderer","The object's renderer to make visibilty checks against it.","OK");
		}
		EditorGUILayout.EndHorizontal();
		
		DrawSeparator();
		
		GUILayout.Space(8);

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Save preset")) {
			savePreset(buoyancy);
		}	
		if(GUILayout.Button("Load preset")) {
			loadPreset(buoyancy);
		}

		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(8);	
		
		DrawSeparator();	
		
		if (GUI.changed) {
			EditorUtility.SetDirty(buoyancy);
			if(!EditorApplication.isPlaying) EditorSceneManager.MarkSceneDirty(buoyancy.gameObject.scene);
		}
														
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



	//saves a buoyancy preset
	public static void savePreset(Buoyancy buoyancy) {
		if (!Directory.Exists(presetPath)) Directory.CreateDirectory(presetPath);
		string preset = EditorUtility.SaveFilePanel("Save buoyancy preset", presetPath,"","buoyancy");

		if (preset != null) {
			if (preset.Length > 0) {
				using (BinaryWriter swr = new BinaryWriter(File.Open(preset, FileMode.Create))) {
					//rigidbody parameters
					if(rrig != null) {
						swr.Write((byte)1);//has Rigidbody
						swr.Write((float)rrig.mass);//float
						swr.Write((float)rrig.drag);//float
						swr.Write((float)rrig.angularDrag);//float
						swr.Write((bool)rrig.useGravity);//bool
					}

					//box collider parameters
					if(col != null) {
						swr.Write((byte)1);//has Boxcollider
						swr.Write(col.center.x);//float
						swr.Write(col.center.y);//float
						swr.Write(col.center.z);//float
						swr.Write(col.size.x * buoyancy.transform.localScale.x);//float
						swr.Write(col.size.y * buoyancy.transform.localScale.y);//float
						swr.Write(col.size.z * buoyancy.transform.localScale.z);//float
					}

					//buoyancy parameters
					swr.Write(buoyancy.magnitude);//float
					swr.Write(buoyancy.ypos);//float
					swr.Write(buoyancy.CenterOfMassOffset);//float
					swr.Write(buoyancy.dampCoeff);//float
					swr.Write(buoyancy.SlicesX);//int
					swr.Write(buoyancy.SlicesZ);//int
					swr.Write(buoyancy.interpolation);//int
					swr.Write(buoyancy.ChoppynessAffectsPosition);//bool
					swr.Write(buoyancy.cvisible);//bool
					swr.Write(buoyancy.ChoppynessFactor);//float
					swr.Write(buoyancy.WindAffectsPosition);//bool
					swr.Write(buoyancy.wvisible);//bool
					swr.Write(buoyancy.WindFactor);//float
					swr.Write(buoyancy.xAngleAddsSliding);//bool
					swr.Write(buoyancy.svisible);//bool
					swr.Write(buoyancy.slideFactor);//float
					swr.Write(buoyancy.moreAccurate);//bool
					swr.Write(buoyancy.useFixedUpdate);//bool

					var bc = buoyancy.GetComponent<BoatController>();

					//If the object has a boat controller attached write the properties.
					//This is useful, because if you change the buoynacy settings the speed
					//settings of the boat controller need tweaking again.
					if(bc!= null) {
						swr.Write(true);//bool
						swr.Write(bc.m_FinalSpeed);//float
						swr.Write(bc.m_accelerationTorqueFactor);//float
						swr.Write(bc.m_InertiaFactor);//float
						swr.Write(bc.m_turningFactor);//float
						swr.Write(bc.m_turningTorqueFactor);//float
					} else {
						swr.Write(false);//bool
					}

					swr.Write(buoyancy.renderQueue);//int
				}

			}
		}
	}

	//loads a buoyancy preset (and boat controller settings if available)
	public static bool loadPreset(Buoyancy buoyancy) {

		string preset = EditorUtility.OpenFilePanel("Load Ocean preset",presetPath,"buoyancy");
		if(!Application.isPlaying) {
			if (preset != null) {
				if (preset.Length > 0) {

					if(File.Exists(preset)) {
						using (BinaryReader br = new BinaryReader(File.Open(preset, FileMode.Open))){

							bool hasrigidbody = false, hascollider = false;

							if(br.BaseStream.Position != br.BaseStream.Length) hasrigidbody = br.ReadBoolean();

							if(hasrigidbody) {
								float mass=1, drag=1, andrag=1;
								bool usegrav=true;
								if(br.BaseStream.Position != br.BaseStream.Length) mass = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) drag = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) andrag = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) usegrav = br.ReadBoolean();
								if(rrig != null) {
									rrig.mass = mass; rrig.drag = drag; rrig.angularDrag = andrag; rrig.useGravity = usegrav;
								} else { Debug.Log("No rigid body found"); }
							}

							if(br.BaseStream.Position != br.BaseStream.Length) hascollider = br.ReadBoolean();

							if(hascollider) {
								float x=0, y=0, z=0, sx=1, sy=1, sz=1;
								if(br.BaseStream.Position != br.BaseStream.Length) x = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) y = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) z = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) sx = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) sy = br.ReadSingle();
								if(br.BaseStream.Position != br.BaseStream.Length) sz = br.ReadSingle();

								if(col != null) {
									
									col.center = new Vector3(x, y, z);
									col.size = new Vector3(sx/buoyancy.transform.localScale.x, sy/buoyancy.transform.localScale.y, sz/buoyancy.transform.localScale.z);

								} else { Debug.Log("No Box Collider found"); }
							}

							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.magnitude = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.ypos = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.CenterOfMassOffset = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.dampCoeff = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.SlicesX = br.ReadInt32();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.SlicesZ = br.ReadInt32();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.interpolation = br.ReadInt32();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.ChoppynessAffectsPosition = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.cvisible = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.ChoppynessFactor = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.WindAffectsPosition = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.wvisible = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.WindFactor = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.xAngleAddsSliding = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.svisible = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.slideFactor = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.moreAccurate = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.useFixedUpdate = br.ReadBoolean();
							bool hasBoatController = false;
							if(br.BaseStream.Position != br.BaseStream.Length) {
								hasBoatController = br.ReadBoolean();
								if(hasBoatController) {
									float res=0;
									var bc = buoyancy.GetComponent<BoatController>();
									res=br.ReadSingle(); if(bc) bc.m_FinalSpeed = res;
									res=br.ReadSingle(); if(bc) bc.m_accelerationTorqueFactor = res;
									res=br.ReadSingle(); if(bc) bc.m_InertiaFactor = res;
									res=br.ReadSingle(); if(bc) bc.m_turningFactor = res;
									res=br.ReadSingle(); if(bc) bc.m_turningTorqueFactor= res;
								}
							}
							
							if(br.BaseStream.Position != br.BaseStream.Length) buoyancy.renderQueue = br.ReadInt32();

							//try to asign a renderer for visibility checks if there is none assigned in the boyancy inspector.
							if(buoyancy._renderer == null) {
								buoyancy._renderer = buoyancy.GetComponent<Renderer>();
								if(!buoyancy._renderer) {
									buoyancy._renderer = buoyancy.GetComponentInChildren<Renderer>();
								}
							}

							EditorUtility.SetDirty(buoyancy);
							if(hasBoatController){
								var bc = buoyancy.GetComponent<BoatController>();
								EditorUtility.SetDirty(bc);
								if(!EditorApplication.isPlaying) EditorSceneManager.MarkSceneDirty(bc.gameObject.scene);
							}

							if(!EditorApplication.isPlaying) EditorSceneManager.MarkSceneDirty(buoyancy.gameObject.scene);
							return true;
							
						}
					} else {Debug.Log(preset+" does not exist..."); return false;}

				}
			} 
		} else { Debug.Log("Cannot load this on runtime"); }

		return false;
	}

}
#endif
