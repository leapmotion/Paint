using UnityEngine;
using System.Collections;
using zzOld_MeshGeneration_LeapPaint_v3;
using Leap.Unity.Attributes;

namespace Leap.Unity.LeapPaint_v3 {
  /// <summary>
  /// Generates a RibbonCircle mesh (or multiple concentric RibbonCircles) based on editor parameters.
  /// </summary>
  public class RibbonCircleGenerator : MonoBehaviour {

    #region PUBLIC FIELDS

    [Header("Generation Settings")]
    [MinValue(0)]
    public int _numCircles = 32;

    [MinValue(0F)]
    public float _initRadius = 0.5F;

    [MinValue(0F)]
    public float _radiusStep = 1F;

    [MinValue(0F)]
    public float _thickness = 0.02F;

    [Range(0F, 360F)]
    public float _tangentAngle = 0F;

    [MinValue(3)]
    public int _radialSubdivisions = 64;

    [SerializeField]
    public bool doGenerate = false;

    public bool _shouldMeshUseShadows = true;

    [Header("Generated Object Settings")]
    [Tooltip("If this GameObject is null, it will be instantiated in OnValidate.")]
    public GameObject _ribbonCircleObject;
    public Material _ribbonCircleMaterial;
    public Vector3 _ribbonCirclePosition = Vector3.up * 0.005F;

    #endregion

    #region UNITY CALLBACKS

    protected virtual void OnValidate() {
      if (_ribbonCircleObject == null) {
        _ribbonCircleObject = new GameObject();
      }
      if (doGenerate) {
        RefreshRibbonCircles();
      }
    }

    protected virtual void OnDestroy() {
      if (_ribbonCircleObject != null) {
        Destroy(_ribbonCircleObject);
      }
    }

    #endregion

    #region PRIVATE METHODS

    private void RefreshRibbonCircles() {
      float[] radii = new float[_numCircles];
      for (int i = 0; i < radii.Length; i++) {
        radii[i] = _initRadius + i * _radiusStep;
      }

      CreateRibbonCircles(radii, _thickness, _radialSubdivisions, _tangentAngle);
      _ribbonCircleObject.transform.localPosition = _ribbonCirclePosition;
    }

    private void CreateRibbonCircles(float[] radii, float[] thicknesses, int[] radialSubdivisionsArr, float[] tangentAngles) {
      MeshFilter meshFilter = _ribbonCircleObject.GetComponent<MeshFilter>();
      if (meshFilter == null) {
        meshFilter = _ribbonCircleObject.AddComponent<MeshFilter>();
      }
      MeshRenderer meshRenderer = _ribbonCircleObject.GetComponent<MeshRenderer>();
      if (meshRenderer == null) {
        meshRenderer = _ribbonCircleObject.AddComponent<MeshRenderer>();
      }
      meshRenderer.shadowCastingMode = (_shouldMeshUseShadows ? UnityEngine.Rendering.ShadowCastingMode.TwoSided : UnityEngine.Rendering.ShadowCastingMode.Off);
      meshRenderer.receiveShadows = _shouldMeshUseShadows;
      meshRenderer.material = _ribbonCircleMaterial;

      ShapeCombiner c = new ShapeCombiner();
      if (radii.Length != thicknesses.Length || radii.Length != radialSubdivisionsArr.Length) {
        Debug.LogError("[RibbonCircleCreator] radii/thickness/subdivision arrays must match each other in size.");
      }
      for (int i = 0; i < radii.Length; i++) {
        c.AddShape(new RibbonCircle(radii[i], thicknesses[i], radialSubdivisionsArr[i], tangentAngles[i]));
      }
      meshFilter.mesh = c.FinalizeCurrentMesh();
    }

    private void CreateRibbonCircles(
      float[] radii,
      float thickness,
      int radialSubdivisions,
      float tangentAngle) {
      float[] thicknesses = new float[radii.Length];
      int[] radialSubdivisionsArr = new int[radii.Length];
      float[] tangentAngles = new float[radii.Length];

      for (int i = 0; i < radii.Length; i++) {
        thicknesses[i] = thickness;
        radialSubdivisionsArr[i] = radialSubdivisions;
        tangentAngles[i] = tangentAngle;
      }

      CreateRibbonCircles(radii, thicknesses, radialSubdivisionsArr, tangentAngles);
    }

    private void CreateRibbonCircle(
      float radius,
      float thickness,
      int radialSubdivisions,
      float tangentAngle) {
      CreateRibbonCircles(new float[] { radius }, new float[] { thickness }, new int[] { radialSubdivisions }, new float[] { tangentAngle });
    }

    #endregion

  }

}

