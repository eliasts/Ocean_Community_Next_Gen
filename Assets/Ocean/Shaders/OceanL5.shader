Shader "Mobile/OceanL5" {
	Properties {
	    _SurfaceColor ("SurfaceColor", Color) = (1,1,1,1)
	    _WaterColor ("WaterColor", Color) = (1,1,1,1)

		_Specularity ("Specularity", Range(0.01,1)) = 0.3

		_SunColor ("SunColor", Color) = (1,1,0.901,1)

		_Bump ("Bump (RGB)", 2D) = "bump" {}
		_FoamFactor("Foam Factor", Range(0,3)) = 1.8
		_Size ("UVSize", Float) = 0.015625//this is the best value (1/64) to have the same uv scales of normal and foam maps on all ocean sizes
		_FoamSize ("FoamUVSize", Float) = 2//tiling of the foam texture
		_SunDir ("SunDir", Vector) = (0.3, -0.6, -1, 0)

		_FakeUnderwaterColor ("Water Color LOD1", Color) = (0.196, 0.262, 0.196, 1)
		_WaterLod1Alpha ("Water Transparency", Range(0,1)) = 0.95
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
			#pragma multi_compile_fog
			#pragma target 2.0
			#include "UnityCG.cginc"

			struct v2f {
    			float4 pos : SV_POSITION;
				half3 floatVec : TEXCOORD0;
    			float3  bumpTexCoord : TEXCOORD1;
    			half3  viewDir : TEXCOORD2;
    			half3  lightDir : TEXCOORD3;
				float2 buv : TEXCOORD4;
				half3 normViewDir : TEXCOORD5;
				UNITY_FOG_COORDS(7)
			};

			half _Size;
			half _FoamSize;
			half4 _SunDir;
			half4 _FakeUnderwaterColor;
            
			v2f vert (appdata_tan v) {
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

    			o.bumpTexCoord.xy = v.vertex.xz*_Size;///float2(_Size.x, _Size.z)*5;
    			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    			o.bumpTexCoord.z = v.tangent.w;

    			half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			o.viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.buv = float2(o.bumpTexCoord.x + _CosTime.x * 0.2, o.bumpTexCoord.y + _SinTime.x * 0.3);

				o.normViewDir = normalize(o.viewDir);

				o.floatVec = normalize(o.normViewDir - normalize(o.lightDir));

				UNITY_TRANSFER_FOG(o, o.pos);

    			return o;
			}

			sampler2D _Bump;
			sampler2D _Foam;
			half _FoamFactor;
			half4 _WaterColor;//Lod1;
			half4 _SurfaceColor;
            half _WaterLod1Alpha;
			half _Specularity;
            half4 _SunColor;

			half4 frag (v2f i) : COLOR {

				half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) -1;
				half3 tangentNormal = normalize(tangentNormal0);

				half4 result = half4(0, 0, 0, 1);
                
				half fresnelLookup = dot(tangentNormal, i.normViewDir);
				//float bias = 0.06;
				//float power = 4.0;
				half fresnelTerm = 0.06 + (1.0-0.06)*pow(1.0 - fresnelLookup, 4.0);

				half foam = clamp(tex2D(_Foam, -i.buv.xy  *_FoamSize)  - 0.5, 0.0, 1.0) * i.bumpTexCoord.z * _FoamFactor;

				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam);
                
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor, _SunColor.rgb*_SurfaceColor*0.85, fresnelTerm*0.65) + clamp(foam.r, 0.0, 1.0)*_SunColor.b + specular*_SunColor.rgb;
                result.a = _WaterLod1Alpha;

				UNITY_APPLY_FOG(i.fogCoord, result); 

    			return result;
			}
			ENDCG

		}
    }
	
}
		