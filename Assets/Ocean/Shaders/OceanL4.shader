// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/OceanL4" {
	Properties {
	    _SurfaceColor ("SurfaceColor", Color) = (1,1,1,1)
	    _WaterColor ("WaterColor", Color) = (1,1,1,1)

		_Specularity ("Specularity", Range(0.01,1)) = 0.3
		_SpecPower("Specularity Power", Range(0,1)) = 1

		[HideInInspector] _SunColor ("SunColor", Color) = (1,1,0.901,1)

		_Refraction ("Refraction (RGB)", 2D) = "white" {}
		_Reflection ("Reflection (RGB)", 2D) = "white" {}
		_Bump ("Bump (RGB)", 2D) = "bump" {}
		_Size ("UVSize", Float) = 0.015625//this is the best value (1/64) to have the same uv scales of normal and foam maps on all ocean sizes
		[HideInInspector] _SunDir ("SunDir", Vector) = (0.3, -0.6, -1, 0)

		//_FakeUnderwaterColor ("Water Color LOD1", Color) = (0.196, 0.262, 0.196, 1)
		_DistanceCancellation ("Distance Cancellation", Float) = 2000
	}

	//water bump/refelection/refraction
	SubShader {
	    Tags { "RenderType" = "Opaque" "Queue"="Geometry"}
		LOD 4
    	Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile FOGON FOGOFF
			#pragma multi_compile DCON	DCOFF

			#pragma target 2.0

			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
    			half4  projTexCoord : TEXCOORD0;
    			float3  bumpTexCoord : TEXCOORD1;
    			//half3  viewDir : TEXCOORD2;
    			half3  objSpaceNormal : TEXCOORD2;
    			half3  lightDir : TEXCOORD3;
				float2 buv : TEXCOORD4;
				half3 normViewDir : TEXCOORD5;
				//UNITY_FOG_COORDS(7)
				#ifdef FOGON
				half dist : TEXCOORD6;
				#ifdef DCON
				half distCancellation : TEXCOORD7;
				#endif
				#endif
			};

			half _Size;
			half4 _SunDir;
			#ifdef FOGON
 			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#ifdef DCON
			half _DistanceCancellation;
			#endif
			#endif

			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///64;//float2(_Size.x, _Size.z);

    			o.pos = UnityObjectToClipPos (v.vertex);

				o.bumpTexCoord.z = v.tangent.w;
 
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

				o.buv = float2(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3);

				o.normViewDir = normalize(viewDir);
                
				#ifdef FOGON
				//manual fog
				half fogDif = 1.0/(unity_FogEnd.x - unity_FogStart.x);
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) * fogDif;
				#ifdef DCON
				o.distCancellation = (unity_FogEnd.x - _DistanceCancellation) * fogDif;
				#endif
                #endif

				//autofog
				//UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Refraction;
			sampler2D _Reflection;
			sampler2D _Bump;
			half4 _SurfaceColor;
			half4 _WaterColor;
			half _Specularity;
			half _SpecPower;
			half4 _SunColor;

			half4 frag (v2f i) : COLOR {
				#ifdef FOGON
				#ifdef DCON
				if(i.dist>i.distCancellation){
				#endif
				#endif
												
					half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.5) -1;
					half3 tangentNormal = normalize(tangentNormal0);

					half4 result = half4(0, 0, 0, 1);

					half2 bumpSampleOffset = (i.objSpaceNormal.xz  + tangentNormal.xy) * 0.05  + i.projTexCoord.xy;// + projTexCoord.xy
	
					half3 reflection = tex2D( _Reflection,  bumpSampleOffset) * _SurfaceColor  ;
					half3 refraction = tex2D( _Refraction,  bumpSampleOffset ) * _WaterColor ;

					//half fresnelLookup = dot(tangentNormal, i.normViewDir);
					//float bias = 0.06;
					//float power = 4.0;
					//half fresnelTerm = 0.06 + (1.0 - 0.06)*pow(1.0 - fresnelLookup, 4);

					half fresnelTerm = 1 - saturate(dot (i.normViewDir, tangentNormal0));

					half3 floatVec = normalize(i.normViewDir - normalize(i.lightDir));

					half specular = pow(max(dot(floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *_SpecPower;


					//method2
					result.rgb = lerp(refraction, reflection, fresnelTerm)*_SunColor.rgb + specular*_SunColor.rgb;

					//fog
					//UNITY_APPLY_FOG(i.fogCoord, result); 

					#ifdef FOGON
					//manual fog (linear) (reduces instructions on d3d9)
					float ff = saturate(i.dist.x);
					result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
					#endif

    				return result;
				#ifdef FOGON
				#ifdef DCON
				}else{
					return unity_FogColor;
				}
				#endif
				#endif
			}

			ENDCG
		}
    }
	
}
		