using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings {
        [Header("Properties")]
        public RenderTextureFormat colorFormat;
        public int depthBufferBits;
        public FilterMode filterMode;
        public Vector4 backgroundColor;
    }
    class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        // RTHandle is RenderTargetHandle
        private readonly RenderTargetHandle normals;
        // render material
        private readonly Material normalsMat;
        // texture settings
        private ViewSpaceNormalsTextureSettings textureSettings;
        // list of shader tag id's
        private readonly List<ShaderTagId> shaderTagList;
        // filtering settings
        private FilteringSettings filteringSettings;
        // Constructor
        public ViewSpaceNormalsTexturePass(RenderPassEvent e, ViewSpaceNormalsTextureSettings s, LayerMask m)
        {
            this.renderPassEvent = e;
            // initialize settings
            this.textureSettings = s;
            // initalize normals texture material
            this.normalsMat = new Material(Shader.Find("Hidden/BananaCowboyCustom/ViewSpaceNormalsShader"));
            // initialize renderer target handle
            this.normals.Init("_SceneViewSpaceNormals");
            // create shader tag list
            this.shaderTagList = new List<ShaderTagId> {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit") };
            // initialize filtering settings
            this.filteringSettings = new FilteringSettings(RenderQueueRange.opaque, m);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            // normals Texture Descriptor setup
            normalsTextureDescriptor.colorFormat = textureSettings.colorFormat;
            normalsTextureDescriptor.depthBufferBits = textureSettings.depthBufferBits;
            // gets a temporary render texture for our view normals
            cmd.GetTemporaryRT(normals.id, normalsTextureDescriptor, textureSettings.filterMode);
            ConfigureTarget(normals.Identifier());
            ConfigureClear(ClearFlag.All, textureSettings.backgroundColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // return if normalMat does not exist
            if (!normalsMat)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation"))) { 
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                DrawingSettings drawSettings = CreateDrawingSettings(shaderTagList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawSettings.overrideMaterial = normalsMat;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(normals.id);
        }
    }

    class ScreenSpaceOutlinesPass : ScriptableRenderPass {

        private readonly Material outlineMat;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetIdentifier tempBuffer;
        private int tempBufferID = Shader.PropertyToID("_TemporaryBuffer");
        // class constructor
        public ScreenSpaceOutlinesPass(RenderPassEvent e)
        {
            this.renderPassEvent = e;
            this.outlineMat = new Material(Shader.Find("Hidden/BananaCowboyCustom/OutlineShader"));
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            // if outline shader material does not exist, return
            if (!outlineMat)
                return;
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines"))) {
                Blit(cmd, cameraColorTarget, tempBuffer);
                Blit(cmd, tempBuffer, cameraColorTarget, outlineMat);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // cmd.ReleaseTemporaryRT(tempBufferID);
        }

    }

    // variables and methods
    // we only want objects in the foreground to have 
    [SerializeField] private LayerMask outlinesLayerMask;
    // specifies when in the render pipeline they should execute
    [SerializeField] private RenderPassEvent renderPassEvent;
    [SerializeField] private ViewSpaceNormalsTextureSettings textureSettings;
    
    // instantiate custom render passes
    ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    ScreenSpaceOutlinesPass screenSpaceOutlinesPass;

    /// <inheritdoc/>
    public override void Create()
    {
        viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(renderPassEvent, textureSettings, outlinesLayerMask);
        screenSpaceOutlinesPass = new ScreenSpaceOutlinesPass(renderPassEvent);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        renderer.EnqueuePass(screenSpaceOutlinesPass);
    }
}


