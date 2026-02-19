// // Save this as: Assets/Scripts/Rendering/OutlineRendererFeature.cs
// // Requires URP 16+ with Render Graph enabled
//
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.RendererUtils;
// using UnityEngine.Rendering.Universal;
// using UnityEngine.Rendering.RenderGraphModule;
//
// public class OutlineRendererFeature : ScriptableRendererFeature
// {
//     [Header("Materials")]
//     public Material maskMaterial;
//     public Material compositeMaterial;
//
//     [Header("Outline Settings")]
//     public Color outlineColor = new Color(1f, 0.6f, 0f, 1f);
//
//     [Range(1f, 10f)]
//     public float outlineThickness = 2f;
//
//     private MaskPass _maskPass;
//     private CompositePass _compositePass;
//
//     public override void Create()
//     {
//         _maskPass = new MaskPass(maskMaterial);
//         _maskPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
//
//         _compositePass = new CompositePass(compositeMaterial);
//         _compositePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
//     }
//
//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {
//         if (renderingData.cameraData.cameraType == CameraType.Preview) return;
//         if (SelectionHandler.Instance == null) return;
//         if (SelectionHandler.Instance.SelectedRenderers.Count == 0) return;
//         if (maskMaterial == null || compositeMaterial == null)
//         {
//             Debug.LogWarning("OutlineRendererFeature: materials not assigned in Inspector!");
//             return;
//         }
//
//         compositeMaterial.SetColor("_OutlineColor", outlineColor);
//         compositeMaterial.SetFloat("_OutlineThickness", outlineThickness);
//
//         renderer.EnqueuePass(_maskPass);
//         renderer.EnqueuePass(_compositePass);
//     }
//
//     protected override void Dispose(bool disposing) { }
//
//     // -------------------------------------------------------------------------
//     // PASS 1: Draw selected renderers as white silhouettes into a mask texture
//     // -------------------------------------------------------------------------
//     class MaskPass : ScriptableRenderPass
//     {
//         private readonly Material _maskMaterial;
//
//         private class PassData
//         {
//             public RendererListHandle rendererListHandle;
//             public TextureHandle maskTexture;
//         }
//
//         public MaskPass(Material maskMaterial)
//         {
//             _maskMaterial = maskMaterial;
//             profilingSampler = new ProfilingSampler("OutlineMaskPass");
//         }
//
//         public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//         {
//             var selected = SelectionHandler.Instance?.SelectedRenderers;
//             if (selected == null || selected.Count == 0) return;
//             if (_maskMaterial == null) return;
//
//             UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
//             UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
//             UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
//
//             var desc = cameraData.cameraTargetDescriptor;
//             desc.colorFormat = RenderTextureFormat.R8;
//             desc.depthBufferBits = 0;
//             desc.msaaSamples = 1;
//
//             TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
//                 renderGraph, desc, "_OutlineMaskTexture", false
//             );
//
//             // Store for CompositePass to read
//             MaskPassOutput.maskTexture = maskTexture;
//
//             var rendererListDesc = new RendererListDesc(
//                 new ShaderTagId("SRPDefaultUnlit"),
//                 renderingData.cullResults,
//                 cameraData.camera
//             )
//             {
//                 overrideMaterial = _maskMaterial,
//                 overrideMaterialPassIndex = 0,
//                 layerMask = cameraData.camera.cullingMask,
//                 sortingCriteria = SortingCriteria.CommonOpaque,
//                 renderQueueRange = RenderQueueRange.all
//             };
//
//             RendererListHandle rendererListHandle = renderGraph.CreateRendererList(rendererListDesc);
//
//             using (var builder = renderGraph.AddRasterRenderPass<PassData>(
//                 "OutlineMaskPass", out var passData, profilingSampler))
//             {
//                 passData.maskTexture = maskTexture;
//                 passData.rendererListHandle = rendererListHandle;
//
//                 builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);
//                 builder.UseRendererList(rendererListHandle);
//                 builder.AllowPassCulling(false);
//
//                 builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
//                 {
//                     ctx.cmd.ClearRenderTarget(false, true, Color.black);
//                     ctx.cmd.DrawRendererList(data.rendererListHandle);
//                 });
//             }
//         }
//
//         [System.Obsolete]
//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
//     }
//
//     static class MaskPassOutput
//     {
//         public static TextureHandle maskTexture;
//     }
//
//     // -------------------------------------------------------------------------
//     // PASS 2: Dilate mask edges and composite the outline ring onto the screen
//     // -------------------------------------------------------------------------
//     class CompositePass : ScriptableRenderPass
//     {
//         private readonly Material _compositeMaterial;
//
//         // MaterialPropertyBlock lets us set per-draw properties without
//         // touching global shader state — required inside Render Graph raster passes
//         private readonly MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
//
//         private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
//
//         private class PassData
//         {
//             public TextureHandle maskTexture;
//             public TextureHandle cameraColorTexture;
//             public Material compositeMaterial;
//             public MaterialPropertyBlock mpb;
//         }
//
//         public CompositePass(Material compositeMaterial)
//         {
//             _compositeMaterial = compositeMaterial;
//             profilingSampler = new ProfilingSampler("OutlineCompositePass");
//         }
//
//         public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//         {
//             if (_compositeMaterial == null) return;
//             if (!MaskPassOutput.maskTexture.IsValid())
//             {
//                 Debug.LogWarning("OutlineCompositePass: mask texture handle is not valid");
//                 return;
//             }
//
//             UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
//
//             using (var builder = renderGraph.AddRasterRenderPass<PassData>(
//                 "OutlineCompositePass", out var passData, profilingSampler))
//             {
//                 passData.maskTexture = MaskPassOutput.maskTexture;
//                 passData.cameraColorTexture = resourceData.activeColorTexture;
//                 passData.compositeMaterial = _compositeMaterial;
//                 passData.mpb = _mpb;
//
//                 // Declare the mask texture as an input the pass reads
//                 builder.UseTexture(MaskPassOutput.maskTexture, AccessFlags.Read);
//
//                 // Declare we write to the camera color (composite on top of scene)
//                 builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
//
//                 builder.AllowPassCulling(false);
//
//                 builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
//                 {
//                     // Use MaterialPropertyBlock to bind the mask texture locally —
//                     // this is allowed inside raster passes unlike SetGlobalTexture
//                     
//                     data.mpb.SetTexture(MainTexID, data.maskTexture);
//
//                     ctx.cmd.DrawMesh(
//                         RenderingUtils.fullscreenMesh,
//                         Matrix4x4.identity,
//                         data.compositeMaterial,
//                         0,  // submesh
//                         0,  // pass index
//                         data.mpb
//                     );
//                     //Blitter.BlitTexture(ctx.cmd, data.maskTexture, new Vector4(1, 1, 0, 0), data.compositeMaterial, 0);
//                 });
//             }
//         }
//
//         [System.Obsolete]
//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
//     }
// }