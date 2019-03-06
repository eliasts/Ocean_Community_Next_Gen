// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/Ocean" {
	Properties {
	    _SurfaceColor ("SurfaceColor", Color) = (1,1,1,1)
	    _WaterColor ("WaterColor", Color) = (1,1,1,1)

		_Specularity ("Specularity", Range(0.01,8)) = 0.3
		_SpecPower("Specularity Power", Range(0,1)) = 1

		[HideInInspector] _SunColor ("SunColor", Color) = (1,1,0.901,1)

		_Refraction ("Refraction (RGB)", 2D) = "white" {}
		_Reflection ("Reflection (RGB)", 2D) = "white" {}
		_Bump ("Bump (RGB)", 2D) = "bump" {}
		_Foam("Foam (RGB)", 2D) = "white" {}
		_FoamBump ("Foam B(RGB)", 2D) = "bump" {}
		_FoamFactor("Foam Factor", Range(0,3)) = 1.8
		
		_Size ("UVSize", Float) = 0.015625//this is the best value (1/64) to have the same uv scales of normal and foam maps on all ocean sizes
		_FoamSize ("FoamUVSize", Float) = 2//tiling of the foam texture
		[HideInInspector] _SunDir ("SunDir", Vector) = (0.3, -0.6, -1, 0)

		_FakeUnderwaterColor ("Water Color LOD1", Color) = (0.196, 0.262, 0.196, 1)
		_WaterLod1Alpha ("Water Transparency", Range(0,1)) = 0.95

		[NoScaleOffset] _FoamGradient ("Foam gradient ", 2D) = "white" {}
		_ShoreDistance("Shore Distance", Range(0,20)) = 4
		_ShoreStrength("Shore Strength", Range(1,4)) = 1.5
		_Translucency("Translucency Factor", Range(0,6)) = 2.5
	}

	//water bump/foam bump/double foam/reflection/refraction//shore foam
	SubShader {
	    Tags { "RenderType" = "Opaque" "Queue"="Geometry"}
		LOD 8

    	Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0

			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
    			half4  projTexCoord : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half4  objSpaceNormal : TEXCOORD3;
    			half3  lightDir : TEXCOORD4;
				float4 buv : TEXCOORD5;
				half3 normViewDir : TEXCOORD6;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};
			
			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			half _Translucency;
			#ifdef FOGON
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif

			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///64;//float2(_Size.x, _Size.z);

    			o.pos = UnityObjectToClipPos (v.vertex);

				o.bumpTexCoord.z = v.tangent.w * _FoamFactor;

  				half4 projSource = half4(v.vertex.x, 0.0, v.vertex.z, 1.0);
    			half4 tmpProj = UnityObjectToClipPos( projSource);
    			//o.projTexCoord = tmpProj;

				o.projTexCoord.xy = 0.5 * tmpProj.xy * half2(1, _ProjectionParams.x) / tmpProj.w + half2(0.5, 0.5);

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			o.objSpaceNormal.xyz = v.normal;
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));



				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x *0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w );

				//World UV's
				//o.worldPos = mul(_Object2World, v.vertex).xyz;
				//o.bumpuv.xyzw = o.worldPos.xzxz  * _WaveTiling*0.005  + frac(_Time.xxxx * _WaveDirection);

				//float3 worldPos = mul(_Object2World, v.vertex).xyz;
				//o.buv = float4(worldPos.x + _CosTime.x * 0.2, worldPos.z + _SinTime.x *0.3 ,worldPos.x + _CosTime.y * 0.04, worldPos.z + o.bumpTexCoord.w )*0.05;

				o.normViewDir = normalize(viewDir);

				//translucency calculation
				o.objSpaceNormal.w = pow ( max (0, dot ( o.normViewDir, o.lightDir ) ), 1 ) * 0.5 * _Translucency;



				#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif

				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
                #endif

				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Refraction;
			sampler2D _Reflection;
			sampler2D _Bump;
			sampler2D _Foam;
			sampler2D _FoamBump;

			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif

			//sampler2D _Fresnel;
			half _FoamSize;
			
			half4 _SurfaceColor;
			half4 _WaterColor;
			half _Specularity;
			half _SpecPower;
			half4 _SunColor;
			half4 _FakeUnderwaterColor;


			half4 frag (v2f i) : COLOR {

				//foam
				half _foam =  tex2D(_Foam, -i.buv.xy  *_FoamSize).r;
				half foam = clamp( _foam  * tex2D(_Foam, i.buv.zy * _FoamSize).r -0.15, 0.0, 1.0)  * i.bumpTexCoord.z;
				//-----------------------------------------------------------------------------------------------------------------------------------------------------------

				//bumps			
				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2 )+( tex2D(_Bump, i.buv.zw) * 2 ) - 2 + (  tex2D(_FoamBump, i.bumpTexCoord.xy*_FoamSize)*4   - 1)*foam;
				half3 tangentNormal = normalize(tangentNormal0 );

				half2 bumpSampleOffset = (i.objSpaceNormal.xz  + tangentNormal.xy) * 0.05  + i.projTexCoord.xy;

				//-----------------------------------------------------------------------------------------------------------------------------------------------------------
				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//#endif

				//-----------------------------------------------------------------------------------------------------------------------------------------------------------
				half3 floatVec = normalize(i.normViewDir - normalize(i.lightDir));
				//specular
				#ifdef SHORE_ON
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif

				//-----------------------------------------------------------------------------------------------------------------------------------------------------------


				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif
				 //-----------------------------------------------------------------------------------------------------------------------------------------------------------
				 //translucency
				half3 wc = _WaterColor.rgb * i.objSpaceNormal.w  * _SunColor.rgb;//* floatVec.z 

				half4 result = half4(wc.x , wc.y , wc.z, 1);
				//-----------------------------------------------------------------------------------------------------------------------------------------------------------
				//reflection refraction
				half3 reflection = tex2D( _Reflection,  bumpSampleOffset) * _SurfaceColor.rgb ;
				half3 refraction = tex2D( _Refraction,  bumpSampleOffset ) * _WaterColor.rgb ;//*_FakeUnderwaterColor

				//-----------------------------------------------------------------------------------------------------------------------------------------------------------
				//half4 result = half4(0 , 0 , 0, 1);
				//method2
				result.rgb += lerp(refraction, reflection, fresnelTerm)*_SunColor.rgb + clamp(foam, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;
				
				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}

			ENDCG
		}

		



    }

	//water bump/double foam/reflection/alpha/shore foam
	SubShader {
	    Tags  { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 7
    	Pass {
			Blend One OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0

			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
    			half4  projTexCoord : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  objSpaceNormal : TEXCOORD3;
    			half3  lightDir : TEXCOORD4;
				float4 buv : TEXCOORD5;
				half3 normViewDir : TEXCOORD6;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			#ifdef FOGON
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif

			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///64;//float2(_Size.x, _Size.z);

    			o.pos = UnityObjectToClipPos (v.vertex);

				o.bumpTexCoord.z = v.tangent.w * _FoamFactor;
 
  				half4 projSource = half4(v.vertex.x, 0.0, v.vertex.z, 1.0);
    			half4 tmpProj = UnityObjectToClipPos( projSource);
    			//o.projTexCoord = tmpProj;

				o.projTexCoord.xy = 0.5 * tmpProj.xy * half2(1, _ProjectionParams.x) / tmpProj.w + half2(0.5, 0.5);

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			o.objSpaceNormal = v.normal;
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w);

				o.normViewDir = normalize(viewDir);

				#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif
  
				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif
				            
				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Refraction;
			sampler2D _Reflection;
			sampler2D _Bump;
			sampler2D _Foam;
			sampler2D _FoamBump;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half _FoamSize;
			half4 _SurfaceColor;
			half4 _WaterColor;
			half _Specularity;
			half _SpecPower;
			half4 _SunColor;
			half _WaterLod1Alpha;
			half4 _FakeUnderwaterColor;

			half4 frag (v2f i) : COLOR {
				half _foam =  tex2D(_Foam, -i.buv.xy  *_FoamSize).r;
				half foam = clamp( _foam * tex2D(_Foam, i.buv.zy * _FoamSize).r -0.15, 0.0, 1.0)  * i.bumpTexCoord.z;
								
				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2 )+( tex2D(_Bump, i.buv.zw) * 2 ) - 2 + (  tex2D(_FoamBump, i.bumpTexCoord.xy*_FoamSize)*4   - 1)*foam; 
				half3 tangentNormal = normalize(tangentNormal0 );

				half4 result = half4(0, 0, 0, 1);

				half2 bumpSampleOffset = (i.objSpaceNormal.xz  + tangentNormal.xy) * 0.05  + i.projTexCoord.xy;// + projTexCoord.xy
	
				half3 reflection = tex2D( _Reflection,  bumpSampleOffset) * _SurfaceColor *_FakeUnderwaterColor.a ;

				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//#endif

				half3 floatVec = normalize(i.normViewDir - normalize(i.lightDir));

				#ifdef SHORE_ON
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif

				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif

				//method2
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, reflection, fresnelTerm)*_SunColor.rgb + clamp(foam, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;
				result.a = _WaterLod1Alpha;


				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}

			ENDCG
		}
    }


	//water bump/foam/reflection/alpha/shore foam
	SubShader {
	    Tags  { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 6
    	Pass {
			Blend One OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0

			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
    			half4  projTexCoord : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  objSpaceNormal : TEXCOORD3;
    			half3  lightDir : TEXCOORD4;
				float4 buv : TEXCOORD5;
				half3 normViewDir : TEXCOORD6;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			#ifdef FOGON
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif

			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///64;//float2(_Size.x, _Size.z);

    			o.pos = UnityObjectToClipPos (v.vertex);

				o.bumpTexCoord.z = v.tangent.w * _FoamFactor;
 
  				half4 projSource = half4(v.vertex.x, 0.0, v.vertex.z, 1.0);
    			half4 tmpProj = UnityObjectToClipPos( projSource);
    			//o.projTexCoord = tmpProj;

				o.projTexCoord.xy = 0.5 * tmpProj.xy * half2(1, _ProjectionParams.x) / tmpProj.w + half2(0.5, 0.5);

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			o.objSpaceNormal = v.normal;
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w);

				o.normViewDir = normalize(viewDir);

  				#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif
				    
				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif
				      
				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Refraction;
			sampler2D _Reflection;
			sampler2D _Bump;
			sampler2D _Foam;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half _FoamSize;
			half4 _SurfaceColor;
			half4 _WaterColor;
			half _Specularity;
			half _SpecPower;
			half4 _SunColor;
			half _WaterLod1Alpha;
			half4 _FakeUnderwaterColor;

			half4 frag (v2f i) : COLOR {

				half _foam = tex2D(_Foam, -i.buv.xy *_FoamSize).r;
				half foam = clamp(_foam  - 0.5, 0.0, 1.0) * i.bumpTexCoord.z;
								
				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) + (tex2D(_Bump, i.buv.zw) * 2.0) - 2;
				half3 tangentNormal = normalize(tangentNormal0 );

				half4 result = half4(0, 0, 0, 1);

				half2 bumpSampleOffset = (i.objSpaceNormal.xz  + tangentNormal.xy) * 0.05  + i.projTexCoord.xy;// + projTexCoord.xy
	
				half3 reflection = tex2D( _Reflection,  bumpSampleOffset) * _SurfaceColor *_FakeUnderwaterColor.a ;


				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//#endif

				half3 floatVec = normalize(i.normViewDir - normalize(i.lightDir));

				#ifdef SHORE_ON
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif

				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif

				//method2
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, reflection, fresnelTerm)*_SunColor.rgb + clamp(foam, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;
				result.a = _WaterLod1Alpha;

				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}

			ENDCG
		}
    }

	//water bump/foam bump/double foam/reflection/refraction
	SubShader {
	    Tags {"RenderType" = "Opaque" "Queue"="Geometry"}
		LOD 5
    	Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0

			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
    			half4  projTexCoord : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  objSpaceNormal : TEXCOORD3;
    			half3  lightDir : TEXCOORD4;
				float4 buv : TEXCOORD5;
				half3 normViewDir : TEXCOORD6;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			#ifdef FOGON
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif

			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///64;//float2(_Size.x, _Size.z);

    			o.pos = UnityObjectToClipPos (v.vertex);

				o.bumpTexCoord.z = v.tangent.w * _FoamFactor;

  				half4 projSource = half4(v.vertex.x, v.vertex.y, v.vertex.z, 1.0);
    			half4 tmpProj = UnityObjectToClipPos( projSource);
    			//o.projTexCoord = tmpProj;

				o.projTexCoord.xy = 0.5 * tmpProj.xy * half2(1, _ProjectionParams.x) / tmpProj.w + half2(0.5, 0.5);

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			o.objSpaceNormal = v.normal;
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w);
				//World UV's
				//o.worldPos = mul(_Object2World, v.vertex).xyz;	
				//o.bumpuv.xyzw = o.worldPos.xzxz  * _WaveTiling*0.005  + frac(_Time.xxxx * _WaveDirection);

				o.normViewDir = normalize(viewDir);

	  			#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif
							
				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif

				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Refraction;
			sampler2D _Reflection;
			sampler2D _Bump;
			sampler2D _Foam;
			sampler2D _FoamBump;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half _FoamSize;
			half4 _SurfaceColor;
			half4 _WaterColor;
			half _Specularity;
			half _SpecPower;
			half4 _SunColor;


			half4 frag (v2f i) : COLOR {

				half _foam =  tex2D(_Foam, -i.buv.xy  *_FoamSize).r;
				half foam = clamp( _foam * tex2D(_Foam, i.buv.zy * _FoamSize).r -0.15, 0.0, 1.0)  * i.bumpTexCoord.z;
								
				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2 )+( tex2D(_Bump, i.buv.zw) * 2 ) - 2 + (  tex2D(_FoamBump, i.bumpTexCoord.xy*_FoamSize)*4   - 1)*foam;
				half3 tangentNormal = normalize(tangentNormal0 );

				half4 result = half4(0, 0, 0, 1);

				half2 bumpSampleOffset = (i.objSpaceNormal.xz  + tangentNormal.xy) * 0.05  + i.projTexCoord.xy;
	
				half3 reflection = tex2D( _Reflection,  bumpSampleOffset) * _SurfaceColor  ;
				half3 refraction = tex2D( _Refraction,  bumpSampleOffset ) * _WaterColor ;

				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//#endif

				half3 floatVec = normalize(i.normViewDir - normalize(i.lightDir));

				#ifdef SHORE_ON
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif

				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif

				//simple
				//result.rgb = lerp(refraction, reflection, fresnelTerm)+ clamp(foam, 0.0, 1.0) + specular;

				//method1
				//result.rgb = lerp(refraction, reflection, fresnelTerm) + clamp(foam, 0.0, 1.0)*_SunColor.b + specular;
				//result.rgb *= _SunColor.rgb;

				//method2
				result.rgb = lerp(refraction, reflection, fresnelTerm)*_SunColor.rgb + clamp(foam, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;


				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}

			ENDCG
		}
    }
		
	//water bump/foam/reflection/refraction
	SubShader {
	    Tags { "RenderType" = "Opaque" "Queue"="Geometry"}
		LOD 4
    	Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0

			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
    			half4  projTexCoord : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  objSpaceNormal : TEXCOORD3;
    			half3  lightDir : TEXCOORD4;
				float4 buv : TEXCOORD5;
				half3 normViewDir : TEXCOORD6;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			#ifdef FOGON
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif

			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///64;//float2(_Size.x, _Size.z);

    			o.pos = UnityObjectToClipPos (v.vertex);

				o.bumpTexCoord.z = v.tangent.w * _FoamFactor;
 
  				half4 projSource = half4(v.vertex.x, 0.0, v.vertex.z, 1.0);
    			half4 tmpProj = UnityObjectToClipPos( projSource);
    			//o.projTexCoord = tmpProj;

				o.projTexCoord.xy = 0.5 * tmpProj.xy * half2(1, _ProjectionParams.x) / tmpProj.w + half2(0.5, 0.5);

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			o.objSpaceNormal = v.normal;
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w);

				o.normViewDir = normalize(viewDir);

  	  			#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif
				 
				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif

				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Refraction;
			sampler2D _Reflection;
			sampler2D _Bump;
			sampler2D _Foam;
			half _FoamSize;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half4 _SurfaceColor;
			half4 _WaterColor;
			half _Specularity;
			half _SpecPower;
			half4 _SunColor;

			half4 frag (v2f i) : COLOR {

				half _foam = tex2D(_Foam, -i.buv.xy *_FoamSize).r;
				half foam = clamp(_foam  - 0.5, 0.0, 1.0) * i.bumpTexCoord.z;
								
				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) + (tex2D(_Bump, i.buv.zw) * 2.0) - 2;
				half3 tangentNormal = normalize(tangentNormal0 );

				half4 result = half4(0, 0, 0, 1);

				half2 bumpSampleOffset = (i.objSpaceNormal.xz  + tangentNormal.xy) * 0.05  + i.projTexCoord.xy;// + projTexCoord.xy
	
				half3 reflection = tex2D( _Reflection,  bumpSampleOffset) * _SurfaceColor  ;
				half3 refraction = tex2D( _Refraction,  bumpSampleOffset ) * _WaterColor ;

				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//#endif

				half3 floatVec = normalize(i.normViewDir - normalize(i.lightDir));

				#ifdef SHORE_ON
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif

				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif

				//simple
				//result.rgb = lerp(refraction, reflection, fresnelTerm)+ clamp(foam, 0.0, 1.0) + specular;

				//method1
				//result.rgb = lerp(refraction, reflection, fresnelTerm) + clamp(foam, 0.0, 1.0)*_SunColor.b + specular;
				//result.rgb *= _SunColor.rgb;

				//method2
				result.rgb = lerp(refraction, reflection, fresnelTerm)*_SunColor.rgb + clamp(foam, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;


				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}

			ENDCG
		}
    }
		

 
 		//water bump/foam
	    SubShader {
        Tags { "RenderType" = "Opaque" "Queue"="Geometry"}
        LOD 3
    	Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0
			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
				half3 floatVec : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  lightDir : TEXCOORD3;
				half4 buv : TEXCOORD4;
				half3 normViewDir : TEXCOORD5;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			half4 _FakeUnderwaterColor;
			#ifdef FOGON
      		uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif
			      
			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///float2(_Size.x, _Size.z)*5;
    			o.pos = UnityObjectToClipPos (v.vertex);
    			o.bumpTexCoord.z = v.tangent.w * _FoamFactor;

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w);

				o.normViewDir = normalize(viewDir);

				o.floatVec = normalize(o.normViewDir - normalize(o.lightDir));

    	  		#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif

				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif

				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Bump;
			sampler2D _Foam;
			half _FoamSize;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half4 _WaterColor;//Lod1;
			half4 _SurfaceColor;
			half _Specularity;
			half _SpecPower;
            half4 _SunColor;

			half4 frag (v2f i) : COLOR {

				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) + (tex2D(_Bump, i.buv.zw) * 2.0) - 2;
				half3 tangentNormal = normalize(tangentNormal0);

				half4 result = half4(0, 0, 0, 1);
                
				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//#endif

				half _foam = tex2D(_Foam, -i.buv.xy *_FoamSize).r;
				half foam = clamp(_foam  - 0.5, 0.0, 1.0) * i.bumpTexCoord.z;

				#ifdef SHORE_ON
				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif

				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif
                
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, _SunColor.rgb*_SurfaceColor*0.85, fresnelTerm*0.65) + clamp(foam.r, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;


				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}
			ENDCG
			

		}
    }
 
	//water bump
     SubShader {
        Tags { "RenderType" = "Opaque" "Queue"="Geometry"}
        LOD 2
    	Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0
			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
				half3 floatVec : TEXCOORD0;
    			float2  bumpTexCoord : TEXCOORD1;
    			//half3  viewDir : TEXCOORD2;
    			half3  lightDir : TEXCOORD3;
				half2 buv : TEXCOORD4;
				half3 normViewDir : TEXCOORD5;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half4 _SunDir;
			half4 _FakeUnderwaterColor;
			#ifdef FOGON
       		uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif
			     
			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///float2(_Size.x, _Size.z)*5;
    			o.pos = UnityObjectToClipPos (v.vertex);
    
    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.buv = float2(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3);

				o.normViewDir = normalize(viewDir);

				o.floatVec = normalize(o.normViewDir - normalize(o.lightDir));
  
				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif
				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Bump;
			half4 _WaterColor;
			half4 _SurfaceColor;
			half _Specularity;
			half _SpecPower;
            half4 _SunColor;

			half4 frag (v2f i) : COLOR {

				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) -1;
				half3 tangentNormal = normalize(tangentNormal0);

				half4 result = half4(0, 0, 0, 1);
                
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//half fresnelLookup = dot(tangentNormal,i. normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0-0.06)*pow(1.0 - fresnelLookup, 4.0);

				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
                
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, _SunColor.rgb*_SurfaceColor*0.85, fresnelTerm*0.65)  + specular*_SunColor.rgb;


				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 
				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}
			ENDCG
			

		}
    }


	//water bump/foam/alpha
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 1
    	Pass {
		    Blend One OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF

			#pragma target 2.0
			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
				half3 floatVec : TEXCOORD0;
    			float4  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  lightDir : TEXCOORD3;
				float4 buv : TEXCOORD4;
				half3 normViewDir : TEXCOORD5;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD7;
				#endif
			};

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			half4 _FakeUnderwaterColor;
			#ifdef FOGON
   			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif
			         
			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///float2(_Size.x, _Size.z)*5;
    			o.pos = UnityObjectToClipPos (v.vertex);
    			o.bumpTexCoord.z = v.tangent.w * _FoamFactor;

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				o.buv = float4(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3, o.bumpTexCoord.x + _CosTime.y * 0.04, o.bumpTexCoord.y + o.bumpTexCoord.w);

				o.normViewDir = normalize(viewDir);

				o.floatVec = normalize(o.normViewDir - normalize(o.lightDir));

	    	  	#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif
							
				#ifdef FOGON
				//manual fog
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif

				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Bump;
			sampler2D _Foam;
			half _FoamSize;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half4 _WaterColor;//Lod1;
			half4 _SurfaceColor;
            half _WaterLod1Alpha;
			half _Specularity;
			half _SpecPower;
            half4 _SunColor;

			half4 frag (v2f i) : COLOR {

				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) + (tex2D(_Bump, i.buv.zw) * 2.0) - 2;
				half3 tangentNormal = normalize(tangentNormal0);

				half4 result = half4(0, 0, 0, 1);
                
				//fresnel
				//#ifdef SHORE_ON
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				//#else
				//half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);
				//half fresnelTerm = UNITY_SAMPLE_1CHANNEL( _Fresnel, float2(fresnelLookup,fresnelLookup) );
				//#endif

				half _foam = tex2D(_Foam, -i.buv.xy *_FoamSize).r;
				half foam = clamp(_foam  - 0.5, 0.0, 1.0) * i.bumpTexCoord.z;

				#ifdef SHORE_ON
				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif
   
				//SHORELINES
				#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
                    half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                    foam += foamGradient * intensityFactor * _foam;
				#endif
				             
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, _SunColor.rgb*_SurfaceColor*0.85, fresnelTerm*0.65) + clamp(foam.r, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;
                result.a = _WaterLod1Alpha;


				//fog
				//UNITY_APPLY_FOG(i.fogCoord, result); 

				#ifdef FOGON
				//manual fog (linear) (reduces instructions on d3d9)
				float ff = saturate(i.dist);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
				#endif

    			return result;
			}
			ENDCG

		}
    }

		   
}
