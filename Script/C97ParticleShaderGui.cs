using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace C97
{
    public class C97ParticleShaderGui : ShaderGUI
    {
        // Blend Mode
        public enum BlendModeType
        {
            None = 0,
            Add,
            SoftAdd,
            AlphaBlend,
            Multiply,
        }
        
        /// <summary>
        /// ParticleSystemのCustomDataのindex定義
        /// </summary>
        public enum CustomCoord
        {
            None				= 2,
            CustomCoord1		= 0,
            CustomCoord2		= 1,
        }

        /// <summary>
        /// ParticleSystemのCustomDataのswizzle定義
        /// </summary>
        public enum ShaderVector
        {
            X					= 0,
            Y					= 1,
            Z					= 2,
            W					= 3,
        }
        
        public enum ShaderCustomCoordVector
        {
            None = 0,
            TexCoord1X,
            TexCoord1Y,
            TexCoord1Z,
            TexCoord1W,
            TexCoord2X,
            TexCoord2Y,
            TexCoord2Z,				
            TexCoord2W,
        }
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {            
            // Cull
            var cullModeProperty = FindProperty("_CullMode", properties);

            EditorGUILayout.BeginVertical("box");
            materialEditor.ShaderProperty(cullModeProperty, "Cull Mode");
            EditorGUILayout.EndVertical();
            
            // ZTest
            var zTestModeProperty = FindProperty("_ZTest", properties);

            EditorGUILayout.BeginVertical("box");
            materialEditor.ShaderProperty(zTestModeProperty, "ZTest");
            EditorGUILayout.EndVertical();
            
            // Blend
            var blendModeProperty = FindProperty("_BlendMode", properties);
            var blendMode = (BlendModeType)blendModeProperty.floatValue;

            // TODO 初期化
            
            // 更新
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                
                var selectBlendMode = (BlendModeType) EditorGUILayout.EnumPopup("ブレンド", blendMode);
                if (check.changed)
                {
                    blendModeProperty.floatValue = (float) selectBlendMode;
                    var src = FindProperty("_BlendSrc", properties);
                    var dst = FindProperty("_BlendDst", properties);
                    
                    // 変更されたPopupのTypeからsrcとdstを設定する
                    switch (selectBlendMode)
                    {
                        case BlendModeType.Add:
                            src.floatValue = (float) BlendMode.SrcAlpha;
                            dst.floatValue = (float) BlendMode.One;
                            break;
                        case BlendModeType.SoftAdd:
                            src.floatValue = (float) BlendMode.OneMinusDstColor;
                            dst.floatValue = (float) BlendMode.One;
                            break;
                        case BlendModeType.AlphaBlend:
                            src.floatValue = (float) BlendMode.SrcAlpha;
                            dst.floatValue = (float) BlendMode.OneMinusSrcAlpha;
                            break;
                        case BlendModeType.Multiply:
                            src.floatValue = (float) BlendMode.DstColor;
                            dst.floatValue = (float) BlendMode.Zero;
                            break;
                        default:
                            src.floatValue = (float) BlendMode.One;
                            dst.floatValue = (float) BlendMode.One;
                            break;
                    }
                }
            }
            
            
            
            /*
             * メインテクスチャ
             * 
             */
            
            var mainTexProperty = FindProperty("_MainTex", properties);
            var mainUCoordProp	= FindProperty("_MainU", properties);
            var mainUSwizzleProp = FindProperty("_MainUSwizzle", properties);
            var mainVCoordProp = FindProperty("_MainV", properties);
            var mainVSwizzleProp = FindProperty("_MainVSwizzle", properties);
            
            // メインカラー
            var mainColor = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(mainColor, "MainColor");
            
            // デフォルトのUIを描画する
            materialEditor.ShaderProperty(mainTexProperty, mainTexProperty.name);
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                SetCustomDataProp(mainUCoordProp, mainVCoordProp, mainUSwizzleProp, mainVSwizzleProp);
            }

            /*
             * ゆがみ
             */
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var isDistProperty = FindProperty("_IsDist", properties);
                InitializeToggleLeftUi(materialEditor, isDistProperty, "ゆがみ");
                
                if (isDistProperty.floatValue != 0)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        // Texture
                        var distTexProperty = FindProperty("_DistTex", properties);
                        
                        // CustomData
                        var distUCoordProp	= FindProperty("_DistU", properties);
                        var distUSwizzleProp = FindProperty("_DistUSwizzle", properties);
                        var distVCoordProp = FindProperty("_DistV", properties);
                        var distVSwizzleProp = FindProperty("_DistVSwizzle", properties);
                        
                        // デフォルトのUIを描画する
                        materialEditor.ShaderProperty(distTexProperty, distTexProperty.name);
                        SetCustomDataProp(distUCoordProp, distVCoordProp, distUSwizzleProp, distVSwizzleProp);
                    }
                }
            }
            /*
             * リムライト
             */

            using (new EditorGUILayout.VerticalScope("box"))
            {
                var isRimProperty = FindProperty("_IsRimLight", properties);
                InitializeToggleLeftUi(materialEditor, isRimProperty, "リムライト");
                if (isRimProperty.floatValue != 0)
                {
                    // 反転かどうか
                    var isRimLightRevers = FindProperty("_IsRimLightRevers", properties);
                    InitializeToggleUi(materialEditor, isRimLightRevers, "反転する");

                    // Power
                    var powerProperty = FindProperty("_RimLightPower", properties);
                    powerProperty.floatValue =
                        EditorGUILayout.IntField("Power:", (int) powerProperty.floatValue);
                }
            }
            
            /*
             * マスク
             */
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var isMaskProperty = FindProperty("_IsMask", properties);
                InitializeToggleLeftUi(materialEditor, isMaskProperty, "マスク");

                if (isMaskProperty.floatValue != 0)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        var maskTexProperty = FindProperty("_MaskTex", properties);
                        
                        // CustomData
                        var maskUCoordProp	= FindProperty("_MaskU", properties);
                        var maskUSwizzleProp = FindProperty("_MaskUSwizzle", properties);
                        var maskVCoordProp = FindProperty("_MaskV", properties);
                        var maskVSwizzleProp = FindProperty("_MaskVSwizzle", properties);
                        
                        // デフォルトのUIを描画する
                        materialEditor.ShaderProperty(maskTexProperty, maskTexProperty.name);
                        SetCustomDataProp(maskUCoordProp, maskVCoordProp, maskUSwizzleProp, maskVSwizzleProp);
                        
                        // Power
                        var maskPowerProperty = FindProperty("_MaskPower", properties);
                        maskPowerProperty.floatValue = EditorGUILayout.FloatField("Power:", maskPowerProperty.floatValue);
                    }
                }
            }
        }

        private void SetCustomDataProp(MaterialProperty uCoordProp, MaterialProperty vCoordProp, MaterialProperty uSwizzleProp,
            MaterialProperty vSwizzleProp)
        {
            
            var coordU = (CustomCoord)uCoordProp.floatValue;
            var swizzleU = (ShaderVector)uSwizzleProp.floatValue;
            var coordV = (CustomCoord)vCoordProp.floatValue;
            var swizzleV = (ShaderVector)vSwizzleProp.floatValue;
            
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CustomData1", GUILayout.MaxWidth(150));
            // Getして
            var customCoordVector = (ShaderCustomCoordVector) EditorGUILayout.EnumPopup(
                GetCustomCoordVector(coordU, swizzleU));
            // Setする
            SetCustomCoordVector(customCoordVector, uCoordProp, uSwizzleProp);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CustomData2", GUILayout.MaxWidth(150));
            var customCoordVector2 = (ShaderCustomCoordVector) EditorGUILayout.EnumPopup(
                GetCustomCoordVector(coordV,swizzleV));
            SetCustomCoordVector(customCoordVector2, vCoordProp, vSwizzleProp);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private void CommonShaderKeyWordUpdate(MaterialEditor materialEditor, MaterialProperty properties)
        {            
            // keyworkの更新
            var targetMat = materialEditor.target as Material;
            if (targetMat != null)
            {
                targetMat.shaderKeywords = GetShaderKeyWord(targetMat);
            }
        }
        
        private string[] GetShaderKeyWord(Material mat)
        {
            var keywords = new List<string>();
            var isDist = mat.GetFloat("_IsDist");
            var isMask = mat.GetFloat("_IsMask");
            var isRimLight = mat.GetFloat("_IsRimLight");
            var isRimLightRevers = mat.GetFloat("_IsRimLightRevers");
            
            if (isDist != 0)
            {
                Debug.LogError("ゆがみを追加する");
                keywords.Add("IS_DIST");
            }

            if (isMask != 0)
            {
                Debug.LogError("マスクを追加する");
                keywords.Add("IS_MASK");
            }

            if (isRimLight != 0)
            {
                Debug.LogError("リムライトを追加する");
                keywords.Add("IS_RIM_LIGHT");
            }

            if (isRimLightRevers != 0)
            {
                Debug.LogError("リムライト反転" + isRimLightRevers);
                keywords.Add("IS_RIM_LIGHT_RIVERS");
            }


            
            return keywords.ToArray();
        }

        private void InitializeToggleLeftUi(MaterialEditor materialEditor, MaterialProperty property, string toggleName)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                // CheckBox
                var toggle = property.floatValue == 0 ? false : true;
                property.floatValue = EditorGUILayout.ToggleLeft(toggleName, toggle) ? 1 : 0;
                if (check.changed)
                {
                    CommonShaderKeyWordUpdate(materialEditor, property);
                }
            }
        }
        
        private void InitializeToggleUi(MaterialEditor materialEditor, MaterialProperty property, string toggleName)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                // CheckBox
                var toggle = property.floatValue == 0 ? false : true;
                property.floatValue = EditorGUILayout.Toggle(toggleName, toggle) ? 1 : 0;
                if (check.changed)
                {
                    CommonShaderKeyWordUpdate(materialEditor, property);
                }
            }
        }

        private ShaderCustomCoordVector GetCustomCoordVector(CustomCoord coord, ShaderVector vector)
        {
            if (coord == CustomCoord.CustomCoord1)
            {
                switch (vector)
                {
                    case ShaderVector.X:
                        return ShaderCustomCoordVector.TexCoord1X;
                    case ShaderVector.Y:
                        return ShaderCustomCoordVector.TexCoord1Y;
                    case ShaderVector.Z:
                        return ShaderCustomCoordVector.TexCoord1Z;
                    case ShaderVector.W:
                        return ShaderCustomCoordVector.TexCoord1W;
                }
            }
            else if (coord == CustomCoord.CustomCoord2)
            {
                switch (vector)
                {
                    case ShaderVector.X:
                        return ShaderCustomCoordVector.TexCoord2X;
                    case ShaderVector.Y:
                        return ShaderCustomCoordVector.TexCoord2Y;
                    case ShaderVector.Z:
                        return ShaderCustomCoordVector.TexCoord2Z;
                    case ShaderVector.W:
                        return ShaderCustomCoordVector.TexCoord2W;
                }
            }

            return ShaderCustomCoordVector.None;
        }
        
        protected void SetCustomCoordVector(ShaderCustomCoordVector coordVector, MaterialProperty coordProperty, MaterialProperty swizzleProperty)
        {
            switch (coordVector) {
                case ShaderCustomCoordVector.None:
                    coordProperty.floatValue = (float)CustomCoord.None;
                    break;
                case ShaderCustomCoordVector.TexCoord1X:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord1;
                    swizzleProperty.floatValue = (float)ShaderVector.X;
                    break;
                case ShaderCustomCoordVector.TexCoord1Y:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord1;
                    swizzleProperty.floatValue = (float)ShaderVector.Y;
                    break;
                case ShaderCustomCoordVector.TexCoord1Z:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord1;
                    swizzleProperty.floatValue = (float)ShaderVector.Z;
                    break;
                case ShaderCustomCoordVector.TexCoord1W:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord1;
                    swizzleProperty.floatValue = (float)ShaderVector.W;
                    break;
                case ShaderCustomCoordVector.TexCoord2X:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord2;
                    swizzleProperty.floatValue = (float)ShaderVector.X;
                    break;
                case ShaderCustomCoordVector.TexCoord2Y:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord2;
                    swizzleProperty.floatValue = (float)ShaderVector.Y;
                    break;
                case ShaderCustomCoordVector.TexCoord2Z:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord2;
                    swizzleProperty.floatValue = (float)ShaderVector.Z;
                    break;
                case ShaderCustomCoordVector.TexCoord2W:
                    coordProperty.floatValue = (float)CustomCoord.CustomCoord2;
                    swizzleProperty.floatValue = (float)ShaderVector.W;
                    break;
                default:
                    break;
            }
        }
    }
}


