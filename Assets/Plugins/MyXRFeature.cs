using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
namespace Plugins
{
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "Plug Power Plugin",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Intercept OpenXR lifecycle to trigger floor tracking and device ready state.",
        DocumentationLink = "https://docs.unity3d.com/Packages/com.unity.xr.openxr@0.1/manual/index.html",
        OpenxrExtensionStrings = "XR_test",
        Version = "0.0.1",
        FeatureId = "Doo Doo fart")]
#endif
    public class MyXRReadyCallback : OpenXRFeature
    {
        
    }
}
