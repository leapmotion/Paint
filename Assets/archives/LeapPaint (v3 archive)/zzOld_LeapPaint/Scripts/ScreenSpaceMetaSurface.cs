using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace Leap.Unity.LeapPaint_v3 {

  public class ScreenSpaceMetaSurface : MonoBehaviour {

    [SerializeField]
    private Shader _metaShader;

    private CommandBuffer _commandBuffer;

    private RenderTargetIdentifier _temp2;
    private RenderTargetIdentifier _temp4;
    private RenderTargetIdentifier _temp8;
    private RenderTargetIdentifier _gbuffer2;
    private RenderTargetIdentifier _albedo;


    private Material _material;

    private Camera _camera;

    void Awake() {
      _camera = GetComponent<Camera>();

      _commandBuffer = new CommandBuffer();

      _temp2 = new RenderTargetIdentifier(10002);
      _temp4 = new RenderTargetIdentifier(10004);
      _temp8 = new RenderTargetIdentifier(10008);
      _gbuffer2 = new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer2);
      _albedo = new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer0);

      _material = new Material(_metaShader);//

      _commandBuffer.GetTemporaryRT(10002, Screen.width, Screen.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      _commandBuffer.GetTemporaryRT(10004, Screen.width / 2, Screen.height / 2, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      _commandBuffer.GetTemporaryRT(10008, Screen.width / 4, Screen.height / 4, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

      _commandBuffer.Blit(_gbuffer2, _temp2, _material);
      _commandBuffer.Blit(_temp2, _temp4, _material);
      _commandBuffer.Blit(_temp4, _temp8, _material);
      _commandBuffer.Blit(_temp8, _temp4, _material);
      _commandBuffer.Blit(_temp4, _temp2, _material);
      _commandBuffer.Blit(_temp2, _gbuffer2, _material);//

      _commandBuffer.Blit(_albedo, _temp2, _material);
      _commandBuffer.Blit(_temp2, _temp4, _material);
      _commandBuffer.Blit(_temp4, _temp8, _material);
      _commandBuffer.Blit(_temp8, _temp4, _material);
      _commandBuffer.Blit(_temp4, _temp2, _material);
      _commandBuffer.Blit(_temp2, _albedo, _material);

      _commandBuffer.ReleaseTemporaryRT(10002);
      _commandBuffer.ReleaseTemporaryRT(10004);
      _commandBuffer.ReleaseTemporaryRT(10008);
    }

    void OnEnable() {
      _camera.AddCommandBuffer(CameraEvent.AfterGBuffer, _commandBuffer);
    }

    void OnDisable() {
      _camera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, _commandBuffer);
    }
  }


}
