/*
Shader "Particle"{
    Properties{
        _MainTex("Texture", 2D)="black"{}
        _Color("Color", Color) = (1,1,1,1)
    }
    
    CGINCLUDE
    #include "UnityCG.cginc"
    
    sampler2D _MainTex;
    float4 _MainTexST;
    fixed4 _Color;
    
    v2f{
        float4 pos : POSITION;
        float4 color : TEXCOORD0;
    };
    
    
    ParticleData{
        float3 pos;
        float3 vel;
    };
    
    StructuredBuffer<ParticleData> _ParticlesBuffer;
    
    v2f vert(uint id : SV_VertexID){
        v2f o;
         _ParticlesBuffer[id].pos.x = id*5.0*sin(_Time.x);
         _ParticlesBuffer[id].pos.y = id*5.0;
         _ParticlesBuffer[id].pos.z = id*5.0;
        o.pos = UnityObjectToClipPos(float4(_ParticlesBuffer[id].pos, 1.0));
        o.color = float4(0, 0.1, 0.1, 1);
        return o; 
    }
    
    fixed4 frag(v2f i) : SV_Target{
        return fixed4(1.0, 0.0, 0.0, 1.0);
     }
    ENDCG
    
    SubShader {
        Tags{ "RenderType" = "Transparent" "RenderType" = "Transparent" }
        LOD 300
        ZWrite Off
        blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma target 4.5
            #vertex vert
            #fragment frag
            ENDCG
        }
    }
}

*/

Shader "Custom/Particle" {
	Properties {
		_MainTex("Texture",         2D) = "black" {}
		_ParticleRadius("Particle Radius", Float) = 0.05
		[HDR]
		_WaterColor("WaterColor", Color) = (1, 1, 1, 1)
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_ST;
	fixed4 _WaterColor;

	float  _ParticleRadius;
	float4x4 _InvViewMatrix;
	float4x4  modelMatrix;//model to world
        
	struct v2g {
		float4 pos   : SV_POSITION;
		float4 col : COLOR;
	};

	struct g2f {
		float4 pos   : POSITION;
		float2 uv   : TEXCOORD0;
		float4 col : COLOR;
	};

	struct ParticleData{
        float3 pos;
        float3 vel;
    };

	//StructuredBuffer<FluidParticle> _ParticlesBuffer;
	StructuredBuffer<ParticleData> _ParticlesBuffer;

	// --------------------------------------------------------------------
	// Vertex Shader
	// --------------------------------------------------------------------
	v2g vert(uint id : SV_VertexID) {
		v2g o = (v2g)0;
		//o.pos = float4(_ParticlesBuffer[id].pos.xy, 0, 1);
		//local -> world
		o.pos = mul(modelMatrix, float4(_ParticlesBuffer[id].pos.xyz, 1));
		o.col = float4(0, 0.1, 0.1, 1);
		return o;
	}

	// --------------------------------------------------------------------
	// Geometry Shader
	// --------------------------------------------------------------------

	[maxvertexcount(4)]
	void geom(point v2g IN[1], inout TriangleStream<g2f> triStream) {

        g2f o;
        float3 up = float3(0.0f, 1.0f, 0.0f);
        //ojectからcameraへのベクトル
        float3 forward =  _WorldSpaceCameraPos - IN[0].pos;
        forward.y = 0;
        forward = normalize(forward);
        float3 right = cross(up, forward);
        float texSize = 0.01;
        float halfS = 0.5 * texSize;
        float4 v[4];
        v[0] = float4(IN[0].pos + halfS * right - halfS * up, 1.0);
        v[1] = float4(IN[0].pos + halfS * right + halfS * up, 1.0);
        v[2] = float4(IN[0].pos - halfS * right - halfS * up, 1.0);
        v[3] = float4(IN[0].pos - halfS * right + halfS * up, 1.0);
        
        o.pos = mul(UNITY_MATRIX_VP, v[0]);
        o.uv = float2(1.0, 0.0);
        o.col = IN[0].col;
        triStream.Append(o);

        o.pos = mul(UNITY_MATRIX_VP, v[1]);
        o.uv = float2(1.0, 1.0);
        o.col = IN[0].col;
        triStream.Append(o);

        o.pos = mul(UNITY_MATRIX_VP, v[2]);
        o.uv = float2(0.0, 0.0);
        o.col = IN[0].col;
        triStream.Append(o);

        o.pos = mul(UNITY_MATRIX_VP, v[3]);
        o.uv = float2(0.0, 1.0);
        o.col = IN[0].col;
        triStream.Append(o);

        triStream.RestartStrip();


        /*
        _ParticleRadius = 0.05;
		float size = _ParticleRadius * 2;
		float halfS = _ParticleRadius;

		g2f pIn = (g2f)0;

		for (int x = 0; x < 2; x++) {
			for (int y = 0; y < 2; y++) {
				float4x4 billboardMatrix = UNITY_MATRIX_V;
				billboardMatrix._m03 = billboardMatrix._m13 = billboardMatrix._m23 = billboardMatrix._m33 = 0;

				float2 uv = float2(x, y);

				pIn.pos = IN[0].pos + mul(float4((uv * 2 - float2(1, 1)) * halfS, 0, 1), billboardMatrix);

				pIn.pos = mul(UNITY_MATRIX_VP, pIn.pos);

				pIn.color = IN[0].color;
				pIn.tex = uv;

				triStream.Append(pIn);
			}
		}
		triStream.RestartStrip();
        */
	}

	// --------------------------------------------------------------------
	// Fragment Shader
	// --------------------------------------------------------------------
	fixed4 frag(g2f input) : SV_Target {
	    _WaterColor.r = 1.0 *sin(_Time.x*5.0) + 0.5 ;
	    return _WaterColor * tex2D(_MainTex, input.uv); 
		//return tex2D(_MainTex, input.tex)*float4(0.0, 0.0, 1.0, 0.0);
	}

	ENDCG

	SubShader {
		Tags{ "RenderType" = "Transparent" "RenderType" = "Transparent" }
		LOD 300

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}
	}
}