// Copyright 2024-2026, DisplayXR contributors
// SPDX-License-Identifier: BSL-1.0

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// One-shot URP pipeline setup. Runs the first time the Editor loads this
/// project — if no Universal Render Pipeline Asset is assigned, creates one
/// with XR-friendly defaults (UpscalingFilter = Auto, no MSAA-quirks) and
/// wires it into Project Settings → Graphics + Quality.
///
/// Why this exists: Unity 6's URP-converter wizard creates the URP asset in
/// the Editor but its default UpscalingFilter sometimes lands on FSR/STP,
/// which trips an OpenXR project-validator warning ("Enabling URP upscaling
/// decreases performance significantly because it is currently not supported
/// by XR"). Pinning to Auto keeps OpenXR happy without sacrificing
/// rendering quality.
///
/// The script self-disables after the asset is created so it doesn't churn
/// on every reload.
/// </summary>
[InitializeOnLoad]
internal static class URPSetupBootstrap
{
    private const string kAssetDir = "Assets/Settings";
    private const string kPipelineAssetPath = "Assets/Settings/URP-Pipeline.asset";
    private const string kRendererAssetPath = "Assets/Settings/URP-Renderer.asset";
    private const string kSentinelKey = "DisplayXRTest.URPSetupBootstrap.Done";

    static URPSetupBootstrap()
    {
        // Defer until the editor has finished loading — AssetDatabase.CreateAsset
        // can't run during the static ctor on first import.
        EditorApplication.delayCall += TrySetup;
    }

    private static void TrySetup()
    {
        EditorApplication.delayCall -= TrySetup;
        if (EditorPrefs.GetBool(kSentinelKey, false)) return;
        if (GraphicsSettings.defaultRenderPipeline != null)
        {
            // URP asset already assigned — just patch upscaling filter and call it done.
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset existing)
            {
                if (existing.upscalingFilter != UpscalingFilterSelection.Auto)
                {
                    existing.upscalingFilter = UpscalingFilterSelection.Auto;
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[DisplayXRTest] URP UpscalingFilter set to Auto on existing pipeline asset.");
                }
            }
            EditorPrefs.SetBool(kSentinelKey, true);
            return;
        }

        Directory.CreateDirectory(kAssetDir);

        // Renderer first — pipeline asset references it.
        var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
        renderer.name = "URP-Renderer";
        AssetDatabase.CreateAsset(renderer, kRendererAssetPath);

        var pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
        pipeline.name = "URP-Pipeline";
        AssetDatabase.CreateAsset(pipeline, kPipelineAssetPath);

        // Wire the renderer into the pipeline asset via SerializedObject (the
        // public API doesn't expose the renderer list mutator before play).
        var so = new SerializedObject(pipeline);
        var rendererList = so.FindProperty("m_RendererDataList");
        if (rendererList != null)
        {
            rendererList.arraySize = 1;
            rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            so.FindProperty("m_DefaultRendererIndex").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // XR-friendly defaults.
        pipeline.upscalingFilter = UpscalingFilterSelection.Auto;
        pipeline.msaaSampleCount = 1; // disable MSAA — not needed for our Kooima path

        EditorUtility.SetDirty(pipeline);
        EditorUtility.SetDirty(renderer);
        AssetDatabase.SaveAssets();

        // Assign to GraphicsSettings + every quality level.
        GraphicsSettings.defaultRenderPipeline = pipeline;
        for (int i = 0; i < QualitySettings.count; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
        }

        // Persist the project settings changes.
        AssetDatabase.SaveAssets();
        EditorPrefs.SetBool(kSentinelKey, true);
        Debug.Log("[DisplayXRTest] URP pipeline asset created at " + kPipelineAssetPath +
                  " with UpscalingFilter=Auto and assigned to all quality levels.");
    }
}
