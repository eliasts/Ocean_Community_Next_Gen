
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class presetLoader : MonoBehaviour {

	public static bool loadPreset(Ocean o, string preset, bool runtime = false, bool useFixedGaussianRandTable = false) {
		#if !UNITY_WEBGL && !UNITY_WEBPLAYER && !(UNITY_WSA_8_1 ||  UNITY_WP_8_1 || UNITY_WINRT_8_1) || UNITY_EDITOR
			return readPreset(o, preset, runtime, useFixedGaussianRandTable);
		#else
			return false;
		#endif
	}

	public static bool readPreset(Ocean o, string preset, bool runtime = false, bool useFixedGaussianRandTable = false) {


		#if NATIVE
		uocean._setFixedRandomTable(false, 0, null, null);
		#endif

		//ios: path = Application.dataPath + "/Raw";
		//android: path = "jar:file://" + Application.dataPath + "!/assets/";

		#if !UNITY_EDITOR && UNITY_ANDROID
			WWW wt = new WWW(preset);
			while(!wt.isDone){}

			string filename = Path.GetFileName(preset);
			string destination = Application.persistentDataPath + "/" + filename;

			if(wt.error == null) {
				if (File.Exists(destination)) File.Delete(destination);
				File.WriteAllBytes(destination, wt.bytes);
				preset = destination;
			}else {
				Debug.Log("Could not find preset in Streaming Assets folder");
				return false;
			}
		#endif


		FileStream fs = new FileStream(preset, FileMode.Open, FileAccess.Read);
		if (fs == null) return false;

		BinaryReader br = new BinaryReader(fs);
		if (br == null) { fs.Close(); return false; }

		int ct = o.tiles;
		float cx = o.size.x;
		float cz = o.size.z;
		int cwh = o.width;
		int tt = ct;
		float ccx = cx;
		float ccy = 0;
		float ccz = cz;
		int ccw = cwh, cch = 0;
		bool ccfx = false;

		if (evalStream(br)) o.followMainCamera = br.ReadBoolean();
		if (evalStream(br)) o.ifoamStrength = br.ReadSingle();
		if (evalStream(br)) o.farLodOffset = br.ReadSingle();
		//these values cannot be updated at runtime!
		if((!Application.isPlaying
		#if UNITY_EDITOR
			&& !EditorApplication.isPlaying
			#endif
			) || !runtime ) {
			if (evalStream(br)) o.tiles = br.ReadInt32();
			if (evalStream(br)) o.size.x = br.ReadSingle();
			if (evalStream(br)) o.size.y = br.ReadSingle();
			if (evalStream(br)) o.size.z = br.ReadSingle();
			if (evalStream(br)) o.width = br.ReadInt32();
			if (evalStream(br)) o.height = br.ReadInt32();
			if (evalStream(br)) o.fixedTiles = br.ReadBoolean();
			if (evalStream(br)) br.ReadInt32();//fTilesDistance : removed
			if (evalStream(br)) br.ReadInt32(); //fTilesLod : removed
		} else {
			if (evalStream(br)) tt = br.ReadInt32();
			if (evalStream(br)) ccx = br.ReadSingle();
			if (evalStream(br)) ccy = br.ReadSingle();
			if (evalStream(br)) ccz = br.ReadSingle();
			if (evalStream(br)) ccw = br.ReadInt32();
			if (evalStream(br)) cch = br.ReadInt32();
			if (evalStream(br)) ccfx = br.ReadBoolean();
			if (evalStream(br)) br.ReadInt32();///
			if (evalStream(br)) br.ReadInt32();///
		}
		if (evalStream(br)) o.scale = br.ReadSingle();
		if (evalStream(br)) o.choppy_scale = br.ReadSingle();
		if (evalStream(br)) o.speed = br.ReadSingle();
		if (evalStream(br)) br.ReadSingle();//waveSpeed : removed
		if (evalStream(br)) o.wakeDistance = br.ReadSingle();
		if (evalStream(br)) o.renderReflection = br.ReadBoolean();
		if (evalStream(br)) o.renderRefraction = br.ReadBoolean();
		if (evalStream(br)) o.renderTexWidth = br.ReadInt32();
		if (evalStream(br)) o.renderTexHeight = br.ReadInt32();
		if (evalStream(br)) o.m_ClipPlaneOffset = br.ReadSingle();
		if (evalStream(br)) o.renderLayers = br.ReadInt32();
		if (evalStream(br)) o.specularity = br.ReadSingle();
		if (evalStream(br)) o.mistEnabled = br.ReadBoolean();
		if (evalStream(br)) o.dynamicWaves = br.ReadBoolean();
		if (evalStream(br)) o.humidity = br.ReadSingle();
		if (evalStream(br)) o.pWindx = br.ReadSingle();
		if (evalStream(br)) o.pWindy = br.ReadSingle();
		if (evalStream(br)) o.waterColor.r = br.ReadSingle();
		if (evalStream(br)) o.waterColor.g = br.ReadSingle();
		if (evalStream(br)) o.waterColor.b = br.ReadSingle();
		if (evalStream(br)) o.waterColor.a = br.ReadSingle();
		if (evalStream(br)) o.surfaceColor.r = br.ReadSingle();
		if (evalStream(br)) o.surfaceColor.g = br.ReadSingle();
		if (evalStream(br)) o.surfaceColor.b = br.ReadSingle();
		if (evalStream(br)) o.surfaceColor.a = br.ReadSingle();
		if (evalStream(br)) o.foamFactor = br.ReadSingle();
		if (evalStream(br)) o.spreadAlongFrames = br.ReadBoolean();
		if (evalStream(br)) o.shaderLod = br.ReadBoolean();
		if (evalStream(br)) o.everyXframe = br.ReadInt32();
		if (evalStream(br)) o.useShaderLods = br.ReadBoolean();
		if (evalStream(br)) o.numberLods = br.ReadInt32();
		if (evalStream(br)) o.fakeWaterColor.r = br.ReadSingle();
		if (evalStream(br)) o.fakeWaterColor.g = br.ReadSingle();
		if (evalStream(br)) o.fakeWaterColor.b = br.ReadSingle();
		if (evalStream(br)) o.fakeWaterColor.a = br.ReadSingle();
		if (evalStream(br)) o.defaultLOD = br.ReadInt32();
			
		if (evalStream(br)) { o.reflrefrXframe =  br.ReadInt32(); if(o.reflrefrXframe==0) o.reflrefrXframe = 1; }

		if (evalStream(br)) o.foamUV = br.ReadSingle();

		float x=1000, y=0, z=0;
		if (evalStream(br)) x = br.ReadSingle();
		if (evalStream(br)) y = br.ReadSingle();
		if (evalStream(br)) z = br.ReadSingle();

		if(o.loadSun) {
			if(o.sun != null & x<999) {
				o.sun.rotation = Quaternion.Euler (x, y, z);
				o.SunDir = o.sun.transform.forward;
			}
		}

		if (evalStream(br)) o.bumpUV = br.ReadSingle();
		if (evalStream(br)) o.ifoamWidth = br.ReadSingle();
		if (evalStream(br)) o.lodSkipFrames = br.ReadInt32();
		if (evalStream(br)) {
			string nm = br.ReadString();
			if(nm!= null) {Material mtr = (Material)Resources.Load("oceanMaterials/"+nm, typeof(Material)); if(mtr!= null) { o.material = mtr; o.mat[0] = o.material;  } }
		}
		if (evalStream(br)) {
			string nm1 = br.ReadString();
			if(nm1!= null) { Material mtr = (Material)Resources.Load("oceanMaterials/"+nm1, typeof(Material)); if(mtr!= null) { o.material1 = mtr; o.mat[1] = o.material1; } }
		}
		if (evalStream(br)) {
			string nm2 = br.ReadString();
			if(nm2!= null) {Material mtr = (Material)Resources.Load("oceanMaterials/"+nm2, typeof(Material)); if(mtr!= null) { o.material2 = mtr; o.mat[2] = o.material2; } }
		}

		#if !NATIVE
		if((!Application.isPlaying
		#if UNITY_EDITOR
			&& !EditorApplication.isPlaying
			#endif
			) || !runtime ) {
			if(ccw != o.width) {
				o.h0 = null; o.h02 = null;
				o.gaussRandom1 = null;
				o.gaussRandom2 = null;
				o.wh = o.width * o.height;
				o.wh1 = 1f / (float)o.wh;
				o.h0 = new ComplexF[o.wh];
				o.h02 = new ComplexF[o.wh];
				o.gaussRandom1 = new float[o.wh];
				o.gaussRandom2 = new float[o.wh];
			} 
		}
		#endif

		if((Application.isPlaying
		#if UNITY_EDITOR
			&& EditorApplication.isPlaying
			#endif
			) || runtime ) {
			if(tt!=ct || cx!=ccx || cz!=ccz || cwh!= ccw) {
				o.zeroObjects(true);
				o.tiles = tt; o.size.x = ccx; o.size.y = ccy; o.size.z = ccz; o.width = ccw; o.height = cch; o.fixedTiles = ccfx;
				#if !NATIVE
				o.h0 = null; o.h02 = null;
				o.gaussRandom1 = null;
				o.gaussRandom2 = null;
				o.wh = o.width * o.height;
				o.wh1 = 1f / (float)o.wh;
				o.h0 = new ComplexF[o.wh];
				o.h02 = new ComplexF[o.wh];
				o.gaussRandom1 = new float[o.wh];
				o.gaussRandom2 = new float[o.wh];
				#endif
				o.Initialize(true);
			}
		}

		o.updMaterials();

		if (evalStream(br)) o.shaderAlpha = br.ReadSingle();
		if (evalStream(br)) o.reflectivity = br.ReadSingle();
		if (evalStream(br)) o.translucency = br.ReadSingle();
		if (evalStream(br)) o.shoreDistance = br.ReadSingle();
		if (evalStream(br)) o.shoreStrength = br.ReadSingle();
		if (evalStream(br)) o.specPower = br.ReadSingle();

		if (evalStream(br)) {
			string nm = br.ReadString();
			if(nm!= null) {Shader shd = Shader.Find(nm);  if(shd && o.material) { o.material.shader = shd; o.oceanShader = shd; } }
		}

		if (evalStream(br)) o.hasShore = br.ReadBoolean();
		if (evalStream(br)) o.hasShore1 = br.ReadBoolean();
		if (evalStream(br)) o.hasFog = br.ReadBoolean();
		if (evalStream(br)) o.hasFog1 = br.ReadBoolean();
		if (evalStream(br)) o.hasFog2 = br.ReadBoolean();
		if (evalStream(br)) o.distCan1 = br.ReadBoolean();
		if (evalStream(br)) o.distCan2 = br.ReadBoolean();
		if (evalStream(br)) o.cancellationDistance = br.ReadSingle();
		if (evalStream(br)) o.foamDuration = br.ReadSingle();
		if (evalStream(br)) o.sTilesLod = br.ReadInt32();
		if (evalStream(br)) o.discSize = br.ReadInt32();
		if (evalStream(br)) o.lowShaderLod = br.ReadInt32();
		if (evalStream(br)) o.forceDepth = br.ReadBoolean();
		if (evalStream(br)) o.waveDistanceFactor = br.ReadSingle(); else o.waveDistanceFactor = 1f;


		if (evalStream(br)) {
			bool b = br.ReadBoolean();
			if (b) {
				o._gaussianMode = 1;
				useFixedGaussianRandTable = true;
			} else {
				o._gaussianMode = 0;
				useFixedGaussianRandTable = false;
			}
		}

		if (evalStream(br)) br.ReadBoolean();//reserved
		if (evalStream(br)) br.ReadBoolean();//reserved
		if (evalStream(br)) br.ReadBoolean();//reserved

		if (evalStream(br)) br.ReadInt32();//reserved
		if (evalStream(br)) br.ReadInt32();//reserved

		if (evalStream(br)) br.ReadSingle();//reserved
		if (evalStream(br)) br.ReadSingle();//reserved



		if (!evalStream(br)) {
			useFixedGaussianRandTable = false;
			o._gaussianMode = 0;
		}
		//in case we want a fixed random table
		if(useFixedGaussianRandTable || o._gaussianMode == 1) {

			int len = 0;
			if (evalStream(br)) len = br.ReadInt32();
			#if NATIVE
				o.gaussRandom1 = null;
				o.gaussRandom2 = null;
				o.gaussRandom1 = new float[len];
				o.gaussRandom2 = new float[len];
			#endif

			if(o.gaussRandom1 != null) {
				if(len > 0) {
					if(len == o.gaussRandom1.Length) {
						for(int i = 0; i < len; i++) {
							if (evalStream(br)) o.gaussRandom1[i] = br.ReadSingle();
							if (evalStream(br)) o.gaussRandom2[i] = br.ReadSingle();
						}
					} else {
						Debug.Log("<color=yellow>Fixed Gaussian Table does not match the length of the grid resolution</color>");
					}
				} else {
					Debug.Log("<color=yellow>No fixed table data present</color>");
					useFixedGaussianRandTable = false;
				}
			} else {
				Debug.Log("<color=yellow>Fixed Gaussian Table is null</color>");
			}
		}

		br.Close();
		fs.Close();

		if(Application.isPlaying) o.initDisc();

		o.setSpread();

		//UPDATE ALL VALUES (except tile settings) FOR STANDALONE RUNTIME !!!
		o.matSetVars();

		#if !NATIVE
			o.InitWaveGenerator(false, useFixedGaussianRandTable);
		#else
			uocean._setFixedRandomTable(useFixedGaussianRandTable, o.width * o.height, o.gaussRandom1, o.gaussRandom2);
			//Debug.Log("used fixed: " + useFixedGaussianRandTable);
			uocean.UInit(o.width, o.height, o.pWindx, o.pWindy, o.speed, o.waveScale, o.choppy_scale, o.size.x, o.size.y, o.size.z, o.waveDistanceFactor);
		#endif

		#if !UNITY_EDITOR && UNITY_ANDROID
			if (File.Exists(destination)) File.Delete(destination);
		#endif

		return true;

	}

	public static bool evalStream(BinaryReader br) {
		if (br.BaseStream.Position != br.BaseStream.Length) return true;
		return false;
	}
}
