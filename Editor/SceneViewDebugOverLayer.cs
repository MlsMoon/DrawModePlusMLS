using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace DrawModePlusMLS.Editor
{
    [Overlay(typeof(SceneView), "DrawModePlusMLS Controller")]
    public class SceneViewDebugOverlay : Overlay
    {
        private static readonly int DepthMeter = Shader.PropertyToID("_DepthMeter");
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

        Slider _slider;

        static readonly List<string> StencilCompareNames = new List<string>()
        {
            "Disabled",
            "Never",
            "Less",
            "Equal",
            "LessEqual",
            "Greater",
            "NotEqual",
            "GreaterEqual",
            "Always"
        };

        public override VisualElement CreatePanelContent()
        {
            Debug.Log("DrawModePlusMLS: CreateOverlayPanelContent");

            var root = new VisualElement();
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;
            root.style.width = 260;

            #region Depth设置

            var title = new Label("Depth Settings");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(title);
            _slider = new Slider("Depth Meter (10m~100m)", 10f, 100f);
            _slider.value = 50f;
            Shader.SetGlobalFloat(DepthMeter, _slider.value);
            _slider.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log("DepthMeter: " + evt.newValue);
                Shader.SetGlobalFloat(DepthMeter, evt.newValue);
                SceneView.RepaintAll();
            });
            root.Add(_slider);

            #endregion

            #region Stencil设置
            
            var spacer = new VisualElement();
            spacer.style.height = 8;
            root.Add(spacer);
            
            var stencilTitle = new Label("Stencil Settings");
            stencilTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(stencilTitle);

            int _stencilCompare = 4; // 默认 Equal
            var stencilPopup = new PopupField<string>(
                "Comparison",
                StencilCompareNames.ToList(),
                _stencilCompare
            );
            root.Add(stencilPopup);

            stencilPopup.RegisterValueChangedCallback(evt =>
            {
                string currentName = evt.newValue;
                int stencilValue = StencilCompareNames.IndexOf(currentName);

                // 这里你就拿到了 0–8 的 int
                Debug.Log($"Stencil Compare = {stencilValue}");
                Shader.SetGlobalInt(StencilComp, stencilValue);
                SceneView.RepaintAll();
            });

            #endregion

            return root;
        }
    }
}