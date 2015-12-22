using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
[CustomEditor(typeof(Boyancy))]
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
		Boyancy boyancy = target as Boyancy;

		presetPath = Application.dataPath+"/Ocean/Editor/_OceanPresets";

		rrig = boyancy.GetComponent<Rigidbody>();
		if(rrig== null) Debug.Log("Object requires a Rigidbody");

		presetPath = Application.dataPath+"/Ocean/Editor/_OceanPresets";

		col = boyancy.GetComponent<BoxCollider>();
		if(col == null) 	Debug.Log("Object requires a Box Collider");

	}


	public override void OnInspectorGUI() {

		Boyancy boyancy = target as Boyancy;
		DrawSeparator();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Render Queue", GUILayout.MaxWidth(130));
		boyancy.renderQueue = EditorGUILayout.IntField(boyancy.renderQueue);
		GUILayout.Space(175);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Material Render Queue","Set the object's material render queue to something that suits you. Useful for not showing shore lines under boat.","OK");
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("More Accurate", GUILayout.MaxWidth(130));
		boyancy.moreAccurate = EditorGUILayout.Toggle(boyancy.moreAccurate);
		GUILayout.Space(175);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("More accurate Calculation","If a more accurate function of the Water height function should be used.\n\nIt is however 2.5x times slower.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Use FixedUpdate", GUILayout.MaxWidth(130));
		boyancy.useFixedUpdate = EditorGUILayout.Toggle(boyancy.useFixedUpdate);
		GUILayout.Space(175);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Use FixedUpdate()","If this object should be simulated in the FixedUpdate function. Can be better timed but it is more accurate if unchecked.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Buoyancy");
		GUILayout.Space(-100);
		boyancy.magnitude = EditorGUILayout.Slider(boyancy.magnitude, 0, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Buoyancy magnitude","The amount of the buoyant forces applied to this vessel.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Y offset");
		GUILayout.Space(-100);
		boyancy.ypos = EditorGUILayout.Slider(boyancy.ypos, -30f, 30f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Y offset","How many units the boat will float above the calculated position.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Center of Mass");
		GUILayout.Space(-100);
		boyancy.CenterOfMassOffset = EditorGUILayout.Slider(boyancy.CenterOfMassOffset, -20f, 20f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Center of Mass offset","Offsets the height of the center of mass of the rigidbody.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Damp Coeff");
		GUILayout.Space(-100);
		boyancy.dampCoeff = EditorGUILayout.Slider(boyancy.dampCoeff, 0f, 2f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Damp Coefficient","The damp coefficient of the buoyancy.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slices X");
		GUILayout.Space(-100);
		boyancy.SlicesX = (int)EditorGUILayout.Slider(boyancy.SlicesX, 2, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slices X dimension","The slicing of the bounds of the box collider in the X dimension.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slices Z");
		GUILayout.Space(-100);
		boyancy.SlicesZ = (int)EditorGUILayout.Slider(boyancy.SlicesZ, 2, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slices Z dimension","The slicing of the bounds of the box collider in the Z dimension.","OK");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Interpolation");
		GUILayout.Space(-100);
		boyancy.interpolation = (int)EditorGUILayout.Slider(boyancy.interpolation, 0, 20);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Buoyancy Interpolation","How many cycles will be used to average/interpolate the final buoynacy.\n\nKeep this as small as you can since it adds overhead.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);
		DrawSeparator();
		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Choppyness influence", GUILayout.MaxWidth(130));
		boyancy.ChoppynessAffectsPosition = EditorGUILayout.Toggle(boyancy.ChoppynessAffectsPosition);
		GUILayout.Space(175);
		EditorGUILayout.LabelField("ifIsVisible", GUILayout.MaxWidth(60));
		boyancy.cvisible = EditorGUILayout.Toggle(boyancy.cvisible);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Choppyness affects boat","If the choppyness of the waves affect the boat's position and rotation.\n\nThis will be skipped if the boat has reached a high speed.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Choppyness factor", GUILayout.MinWidth(100));
		GUILayout.Space(-80);
		boyancy.ChoppynessFactor = EditorGUILayout.Slider(boyancy.ChoppynessFactor, 0, 10f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Choppyness Factor","The amount of choppyness that will influence the boat.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Wind affects boat", GUILayout.MaxWidth(130));
		boyancy.WindAffectsPosition = EditorGUILayout.Toggle(boyancy.WindAffectsPosition);
		GUILayout.Space(175);
		EditorGUILayout.LabelField("ifIsVisible", GUILayout.MaxWidth(60));
		boyancy.wvisible = EditorGUILayout.Toggle(boyancy.wvisible);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Wind affects boat","If the Ocean wind affect the boat's position and rotation.\n\nThis will be skipped if the boat has reached a high speed.\n\n"+
			"If ifIsVisible is checked the calculations will take place only if the objects renderer is visible.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Wind factor", GUILayout.MinWidth(100));
		GUILayout.Space(-80);
		boyancy.WindFactor = EditorGUILayout.Slider(boyancy.WindFactor, 0, 5f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Wind Factor","The amount of wind that will influence the boat.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slope sliding",  GUILayout.MaxWidth(130));
		boyancy.xAngleAddsSliding = EditorGUILayout.Toggle(boyancy.xAngleAddsSliding);
		GUILayout.Space(175);
		EditorGUILayout.LabelField("ifIsVisible", GUILayout.MaxWidth(60));
		boyancy.svisible = EditorGUILayout.Toggle(boyancy.svisible);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slope sliding","If the boat faces stormy waves, sliding forces will be applied to it.\n\n"+
			"If ifIsVisible is checked the calculations will take place only if the objects renderer is visible.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Slide factor", GUILayout.MinWidth(100));
		GUILayout.Space(-80);
		boyancy.slideFactor = EditorGUILayout.Slider(boyancy.slideFactor, 0, 10f);
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Slope sliding Factor","The amount of sliding force applied to the boat.","OK");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(10);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Visible Renderer", GUILayout.MinWidth(130));
		GUILayout.Space(-150);
		boyancy._renderer = (Renderer)EditorGUILayout.ObjectField(boyancy._renderer, typeof(Renderer), true, GUILayout.MinWidth(120));
		if(GUILayout.Button("?",GUILayout.MaxWidth(20))) {
			EditorUtility.DisplayDialog("Renderer","The object's renderer to make visibilty checks against it.","OK");
		}
		EditorGUILayout.EndHorizontal();
		
		DrawSeparator();
		
		GUILayout.Space(8);

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Save preset")) {
			savePreset(boyancy);
		}	
		if(GUILayout.Button("Load preset")) {
			loadPreset(boyancy);
		}

		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(8);	
		
		DrawSeparator();	
		
		if (GUI.changed) {
			EditorUtility.SetDirty(boyancy);
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
	public static void savePreset(Boyancy boyancy) {
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
						swr.Write(col.size.x * boyancy.transform.localScale.x);//float
						swr.Write(col.size.y * boyancy.transform.localScale.y);//float
						swr.Write(col.size.z * boyancy.transform.localScale.z);//float
					}

					//buoyancy parameters
					swr.Write(boyancy.magnitude);//float
					swr.Write(boyancy.ypos);//float
					swr.Write(boyancy.CenterOfMassOffset);//float
					swr.Write(boyancy.dampCoeff);//float
					swr.Write(boyancy.SlicesX);//int
					swr.Write(boyancy.SlicesZ);//int
					swr.Write(boyancy.interpolation);//int
					swr.Write(boyancy.ChoppynessAffectsPosition);//bool
					swr.Write(boyancy.cvisible);//bool
					swr.Write(boyancy.ChoppynessFactor);//float
					swr.Write(boyancy.WindAffectsPosition);//bool
					swr.Write(boyancy.wvisible);//bool
					swr.Write(boyancy.WindFactor);//float
					swr.Write(boyancy.xAngleAddsSliding);//bool
					swr.Write(boyancy.svisible);//bool
					swr.Write(boyancy.slideFactor);//float
					swr.Write(boyancy.moreAccurate);//bool
					swr.Write(boyancy.useFixedUpdate);//bool

					var bc = boyancy.GetComponent<BoatController>();

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

					swr.Write(boyancy.renderQueue);//int
				}

			}
		}
	}

	//loads a buoyancy preset (and boat controller settings if available)
	public static bool loadPreset(Boyancy boyancy) {

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
									col.size = new Vector3(sx/boyancy.transform.localScale.x, sy/boyancy.transform.localScale.y, sz/boyancy.transform.localScale.z);

								} else { Debug.Log("No Box Collider found"); }
							}

							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.magnitude = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.ypos = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.CenterOfMassOffset = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.dampCoeff = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.SlicesX = br.ReadInt32();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.SlicesZ = br.ReadInt32();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.interpolation = br.ReadInt32();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.ChoppynessAffectsPosition = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.cvisible = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.ChoppynessFactor = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.WindAffectsPosition = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.wvisible = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.WindFactor = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.xAngleAddsSliding = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.svisible = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.slideFactor = br.ReadSingle();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.moreAccurate = br.ReadBoolean();
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.useFixedUpdate = br.ReadBoolean();
							bool hasBoatController = false;
							if(br.BaseStream.Position != br.BaseStream.Length) {
								hasBoatController = br.ReadBoolean();
								if(hasBoatController) {
									float res=0;
									var bc = boyancy.GetComponent<BoatController>();
									res=br.ReadSingle(); if(bc) bc.m_FinalSpeed = res;
									res=br.ReadSingle(); if(bc) bc.m_accelerationTorqueFactor = res;
									res=br.ReadSingle(); if(bc) bc.m_InertiaFactor = res;
									res=br.ReadSingle(); if(bc) bc.m_turningFactor = res;
									res=br.ReadSingle(); if(bc) bc.m_turningTorqueFactor= res;
								}
							}
							
							if(br.BaseStream.Position != br.BaseStream.Length) boyancy.renderQueue = br.ReadInt32();

							//try to asign a renderer for visibility checks if there is none assigned in the boyancy inspector.
							if(boyancy._renderer == null) {
								boyancy._renderer = boyancy.GetComponent<Renderer>();
								if(!boyancy._renderer) {
									boyancy._renderer = boyancy.GetComponentInChildren<Renderer>();
								}
							}

							EditorUtility.SetDirty(boyancy);
							if(hasBoatController){
								var bc = boyancy.GetComponent<BoatController>();
								EditorUtility.SetDirty(bc);
							}

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
