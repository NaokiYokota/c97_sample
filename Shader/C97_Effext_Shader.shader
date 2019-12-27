Shader "C97/C97_Effext_Shader"
{
    Properties
    {
        //Main
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color",Color) = (1,1,1,1)
        
        //Distortion
        [Toggle]_IsDist ("Use Distortion", int) = 0
        _DistTex ("Texture", 2D) = "white" {}

        //Mask
        [Toggle]_IsMask ("Use Mask", int) = 0
        _MaskTex ("Texture", 2D) = "white" {}
        _MaskPower("Mask Power",float) = 1
                
        // Rim Light
        [Toggle]_IsRimLight ("Use Rim",int) = 0
        [Toggle]_IsRimLightRevers("Use Rim Light Revers", int) = 0
		_RimLightPower("Rim Light Power", float) = 1

        // Blend Mode
        [HideInInspector]_BlendMode("Blend Mode", int) = 0
        _BlendSrc("Blend Src", int) = 1
		_BlendDst("Blend Dst", int)	= 0	
		
		// Custom Data (Main Texture)
		[HideInInspector]_MainU ("_mainucoord", int) = 2
		[HideInInspector]_MainUSwizzle ("_mainuswizzle", int) = 0
		[HideInInspector]_MainV ("_mainvcoord", int) = 2
		[HideInInspector]_MainVSwizzle ("_mainvswizzle", int) = 0
		
		// Custom Data (Dist Texture)
		[HideInInspector]_DistU ("_distucoord", int) = 2
		[HideInInspector]_DistUSwizzle ("_distuswizzle", int) = 0
		[HideInInspector]_DistV ("_distvcoord", int) = 2
		[HideInInspector]_DistVSwizzle ("_distvswizzle", int) = 0
		
		// Custom Data (Mask Texture)
		[HideInInspector]_MaskU ("_maskucoord", int) = 2
		[HideInInspector]_MaskUSwizzle ("_maskuswizzle", int) = 0
		[HideInInspector]_MaskV ("_maskvcoord", int) = 2
		[HideInInspector]_MaskVSwizzle ("_maskvswizzle", int) = 0
		
		[HideInInspector]_MaskCustomPower ("__maskPower", int) = 0

		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest Mode", int) = 4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        Pass
        {
            Blend [_BlendSrc] [_BlendDst]
            Cull [_CullMode]
            Lighting Off 
            ZWrite off
            ZTest[_ZTest]

    
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            //keyWord
            #pragma shader_feature _ IS_DIST
            #pragma shader_feature _ IS_MASK
            #pragma shader_feature _ IS_RIM_LIGHT
            #pragma shader_feature _ IS_RIM_LIGHT_RIVERS
            
            #include "UnityCG.cginc"
            #include "c97_Particle.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                half4 normal : NORMAL;
                float4 texcoords : TEXCOORD0;
                float4 customCoords1 : TEXCOORD1;
                float4 customCoords2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                half4 rimLightValue : TEXCOORD2;
            };
            
            //Main
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            int _BlendMode;
           
            // Dist
            sampler2D _DistTex;
            float4 _DistTex_ST;
            
            // Mask
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            float _MaskPower;
            
            // CutOff
            sampler2D _CutOffTex;
            float4 _CutOffTex_ST;
            float _CutOffPower;
            
            // Rim
            float _RimLightPower;
            
            // CustomData
            SET_CUSTOM_DATA_PROPERTY(_Main);
            SET_CUSTOM_DATA_PROPERTY(_Dist);
            SET_CUSTOM_DATA_PROPERTY(_Mask);

            uniform int _MaskCustomPower;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // custom Data
                half4x4 customData = { v.customCoords1, v.customCoords2, half4(0,0,0,0), half4(0,0,0,0) };
               
                o.color = v.color;
                half2 mainCustomCoord = GET_CUSTOM_DATA(_Main);
                o.uv.xy = TRANSFORM_TEX(v.texcoords.xy,_MainTex) + mainCustomCoord;
                
                // Distortion
                #ifdef IS_DIST
                    half2 distCustomCoord = GET_CUSTOM_DATA(_Dist);
                	o.uv.zw	= TRANSFORM_TEX(v.texcoords.xy, _DistTex) + distCustomCoord;
				#endif
				
				// Mask
				#ifdef IS_MASK
                    half2 maskCustomCoord = GET_CUSTOM_DATA(_Mask);
				    o.uv2.xy = TRANSFORM_TEX(v.texcoords.xy, _MaskTex) + maskCustomCoord;
				#endif
				
				// Rim
				#ifdef IS_RIM_LIGHT
				    // ローカル => world座標へ変換
				    float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
				    // 法線
				    half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				    // カメラへのベクトル
				    // _WorldSpaceCameraPos : カメラのworldPos
					half3 worldViewDir = normalize(_WorldSpaceCameraPos - worldPos);
					
					// dot(x,y) = x,yの内積
					// abs => 絶対値
					o.rimLightValue.x = abs(dot(worldViewDir, worldNormal));
					#ifdef IS_RIM_LIGHT_RIVERS
					    o.rimLightValue.x = 1.0 - o.rimLightValue.x;
					#endif
					o.rimLightValue.y = _RimLightPower;
				#endif
				
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            	half dist = 0.0;
            	// ゆがみ
                #ifdef IS_DIST
					dist = (tex2D(_DistTex, i.uv.zw)).r;
				#endif
				
				half blendRate = 1.0f;
				half4 color = 0;

				// マスク			
				#ifdef IS_MASK
				    blendRate *= pow(tex2D(_MaskTex, i.uv2.xy).g, _MaskPower);
				#endif
				
				// リムの強さをブレンド率に反映する
				#ifdef IS_RIM_LIGHT
				    // pow(x,y) => xのy乗
					blendRate *= pow(i.rimLightValue.x, i.rimLightValue.y);
				#endif
				
				half4 col = tex2D(_MainTex, i.uv + dist);
				col.a *= blendRate;
			    color = col * i.color * _Color;
			    
                return color;
            }
            ENDCG
        }
    }
    	CustomEditor "C97.C97ParticleShaderGui"
}
