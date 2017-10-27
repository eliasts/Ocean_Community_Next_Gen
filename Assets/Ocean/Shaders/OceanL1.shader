// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/OceanL1" {
	Properties {
	    _SurfaceColor ("SurfaceColor", Color) = (1,1,1,1)
	    _WaterColor ("WaterColor", Color) = (1,1,1,1)

		_Specularity ("Specularity", Range(0.01,1)) = 0.3
		_SpecPower("Specularity Power", Range(0,1)) = 1

		[HideInInspector] _SunColor ("SunColor", Color) = (1,1,0.901,1)

		_Bump ("Bump (RGB)", 2D) = "bump" {}
		_Foam("Foam (RGB)", 2D) = "white" {}
		_FoamFactor("Foam Factor", Range(0,3)) = 1.8
		_Size ("UVSize", Float) = 0.015625//this is the best value (1/64) to have the same uv scales of normal and foam maps on all ocean sizes
		_FoamSize ("FoamUVSize", Float) = 2//tiling of the foam texture
		[HideInInspector] _SunDir ("SunDir", Vector) = (0.3, -0.6, -1, 0)

		[NoScaleOffset] _FoamGradient ("Foam gradient ", 2D) = "white" {}
		_ShoreDistance("Shore Distance", Range(0,20)) = 4
		_ShoreStrength("Shore Strength", Range(1,4)) = 1.5

		_FakeUnderwaterColor ("Water Color LOD1", Color) = (0.196, 0.262, 0.196, 1)
		_DistanceCancellation ("Distance Cancellation", Float) = 2000
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
			#pragma multi_compile DCON	DCOFF

			#pragma target 2.0
			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
				half3 floatVec : TEXCOORD0;
    			float3  bumpTexCoord : TEXCOORD1;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD2;
				#endif
    			half3  lightDir : TEXCOORD3;
				half2 buv : TEXCOORD4;
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
			half _FoamFactor;
			half4 _SunDir;
			half4 _FakeUnderwaterColor;
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

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///float2(_Size.x, _Size.z)*5;
    			o.pos = UnityObjectToClipPos (v.vertex);
    			o.bumpTexCoord.z = v.tangent.w * _FoamFactor;

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.buv = float2(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3);

				o.normViewDir = normalize(viewDir);

				o.floatVec = normalize(o.normViewDir - normalize(o.lightDir));

				#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif

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

			sampler2D _Bump;
			sampler2D _Foam;
			half _FoamSize;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half4 _WaterColor;//Lod1;
			half4 _SurfaceColor;
			half _Specularity;
			half _SpecPower;
            half4 _SunColor;

			half4 frag (v2f i) : COLOR {

				#ifdef FOGON
				#ifdef DCON
				if(i.dist>i.distCancellation){
				#endif
				#endif
					half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) -1;
					half3 tangentNormal = normalize(tangentNormal0);

					half4 result = half4(0, 0, 0, 1);
                
					//half fresnelLookup = dot(tangentNormal, i.normViewDir);
					//float bias = 0.06;
					//float power = 4.0;
					//half fresnelTerm = 0.06 + (1.0-0.06)*pow(1.0 - fresnelLookup, 4.0);

					half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));

					half _foam = tex2D(_Foam, i.buv.xy *_FoamSize).r;
					half foam = clamp(_foam  - 0.5, 0.0, 1.0) * i.bumpTexCoord.z;

					half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;

					//SHORELINES
					#ifdef SHORE_ON
					//UNITY5.5
					//#if defined(UNITY_REVERSED_Z)
						//float zdepth = 1.0f - LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#else
						float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
					//#endif
                    float intensityFactor = 1 - saturate((zdepth - i.ref.w) / _ShoreDistance);
					foam += _ShoreStrength * intensityFactor * _foam  ;
					#endif
                
					result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, _SunColor.rgb*_SurfaceColor*0.85, fresnelTerm*0.6) + clamp(foam.r, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;

					//fog
					//UNITY_APPLY_FOG(i.fogCoord, result); 

					#ifdef FOGON
					//manual fog (linear) (reduces instructions on d3d9)
					float ff = saturate(i.dist);
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
