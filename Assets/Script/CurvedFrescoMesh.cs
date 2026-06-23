using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent]
public class CurvedFrescoMesh : MonoBehaviour
{
    public enum CurveAxis
    {
        Horizontal,
        Vertical,
        Both
    }

    [Header("Size")]
    [Min(0.001f)] public float width = 1.0f;
    [Min(0.001f)] public float height = 1.0f;

    [Header("Resolution")]
    [Range(1, 160)] public int subdivisionsX = 48;
    [Range(1, 160)] public int subdivisionsY = 48;

    [Header("Surface Shape")]
    [Tooltip("Small local Z lift above the scanned wall surface to avoid z-fighting.")]
    public float surfaceOffset = 0.006f;

    [Tooltip("Curves the fresco across the arch reveal. Positive pushes the centre forward; negative pulls it inward.")]
    public float depthCurve = 0.12f;

    [Tooltip("Controls whether the depth curve runs horizontally, vertically, or both.")]
    public CurveAxis depthCurveAxis = CurveAxis.Horizontal;

    [Tooltip("Raises the upper centre into a shallow Gothic arch profile.")]
    public float archCurveAmount = 0.20f;

    [Range(0.0f, 1.0f)]
    [Tooltip("Height where the arch lift starts. Lower values bend more of the mesh.")]
    public float archStart = 0.28f;

    [Range(0.1f, 4.0f)]
    [Tooltip("Higher values make the arch lift tighter near the top centre.")]
    public float archSharpness = 1.2f;

    [Tooltip("Adds a small asymmetric twist so the fresco can follow a reveal that turns into depth.")]
    public float revealTwist = 0.0f;

    [Header("UVs")]
    public Vector2 uvTiling = Vector2.one;
    public Vector2 uvOffset = Vector2.zero;
    public bool flipU;
    public bool flipV;

    [Header("Visual Blending")]
    [Range(0.0f, 1.0f)] public float opacity = 1.0f;
    [Range(0.0f, 0.49f)] public float edgeFade = 0.06f;
    [Range(0.0f, 1.0f)] public float roughness = 0.85f;
    [ColorUsage(false, true)] public Color stoneBlendColor = new Color(0.78f, 0.73f, 0.62f, 1.0f);
    [Range(0.0f, 1.0f)] public float stoneBlendAmount = 0.12f;

    [Header("Editor")]
    public bool regenerateInEditor = true;
    public bool recalculateNormals = true;
    public bool recalculateTangents = true;
    public bool drawPreviewGizmos = true;
    public Color gizmoColor = new Color(0.2f, 0.8f, 1.0f, 0.65f);

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
    private static readonly int FrescoOpacityId = Shader.PropertyToID("_FrescoOpacity");
    private static readonly int StoneBlendColorId = Shader.PropertyToID("_StoneBlendColor");
    private static readonly int StoneBlendAmountId = Shader.PropertyToID("_StoneBlendAmount");

    private Mesh generatedMesh;
    private MeshFilter cachedMeshFilter;
    private MeshRenderer cachedMeshRenderer;
    private MaterialPropertyBlock propertyBlock;
#if UNITY_EDITOR
    private bool regenerationQueued;
#endif

    public int TriangleCount
    {
        get { return subdivisionsX * subdivisionsY * 2; }
    }

    private void Reset()
    {
        GenerateMesh();
        ApplyMaterialControls();
    }

    private void OnEnable()
    {
        GenerateMesh();
        ApplyMaterialControls();
    }

    private void OnValidate()
    {
        width = Mathf.Max(0.001f, width);
        height = Mathf.Max(0.001f, height);
        subdivisionsX = Mathf.Clamp(subdivisionsX, 1, 160);
        subdivisionsY = Mathf.Clamp(subdivisionsY, 1, 160);

        if (Application.isPlaying)
            GenerateMesh();
#if UNITY_EDITOR
        else if (regenerateInEditor)
            QueueEditorRegeneration();
#endif

        ApplyMaterialControls();
    }

    private void OnDisable()
    {
        if (generatedMesh == null)
            return;

        if (cachedMeshFilter != null && cachedMeshFilter.sharedMesh == generatedMesh)
            cachedMeshFilter.sharedMesh = null;

        DestroyGeneratedMesh();
    }

    [ContextMenu("Regenerate Mesh")]
    public void GenerateMesh()
    {
        CacheComponents();

        if (generatedMesh == null)
        {
            generatedMesh = new Mesh
            {
                name = "Curved Fresco Mesh",
                hideFlags = HideFlags.DontSave
            };
        }
        else
        {
            generatedMesh.Clear();
        }

        int vertexColumns = subdivisionsX + 1;
        int vertexRows = subdivisionsY + 1;
        int vertexCount = vertexColumns * vertexRows;

        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var colors = new Color[vertexCount];
        var triangles = new int[subdivisionsX * subdivisionsY * 6];

        int vertexIndex = 0;
        for (int y = 0; y < vertexRows; y++)
        {
            float v = y / (float)subdivisionsY;
            float localY = Mathf.Lerp(-height * 0.5f, height * 0.5f, v);
            float archHeightMask = SmoothStep(archStart, 1.0f, v);

            for (int x = 0; x < vertexColumns; x++)
            {
                float u = x / (float)subdivisionsX;
                float localX = Mathf.Lerp(-width * 0.5f, width * 0.5f, u);
                float centeredU = (u - 0.5f) * 2.0f;
                float centeredV = (v - 0.5f) * 2.0f;

                float horizontalFalloff = Mathf.Sin(u * Mathf.PI);
                float verticalFalloff = Mathf.Sin(v * Mathf.PI);
                float archSideMask = Mathf.Pow(Mathf.Clamp01(1.0f - Mathf.Abs(centeredU)), archSharpness);

                float depthMask = GetDepthMask(horizontalFalloff, verticalFalloff);
                float curvedZ = depthCurve * depthMask;
                float archedY = archCurveAmount * archSideMask * archHeightMask;
                float archedZ = archCurveAmount * 0.2f * archSideMask * archHeightMask;
                float twistZ = revealTwist * centeredU * SmoothStep(0.0f, 1.0f, v);

                vertices[vertexIndex] = new Vector3(
                    localX,
                    localY + archedY,
                    surfaceOffset + curvedZ + archedZ + twistZ);

                float uvU = flipU ? 1.0f - u : u;
                float uvV = flipV ? 1.0f - v : v;
                uvs[vertexIndex] = new Vector2(
                    uvOffset.x + uvU * uvTiling.x,
                    uvOffset.y + uvV * uvTiling.y);

                float alpha = opacity * GetEdgeFade(u, v);
                Color blendedWhite = Color.Lerp(Color.white, stoneBlendColor, stoneBlendAmount);
                blendedWhite.a = alpha;
                colors[vertexIndex] = blendedWhite;

                vertexIndex++;
            }
        }

        int triangleIndex = 0;
        for (int y = 0; y < subdivisionsY; y++)
        {
            for (int x = 0; x < subdivisionsX; x++)
            {
                int bottomLeft = y * vertexColumns + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + vertexColumns;
                int topRight = topLeft + 1;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomRight;

                triangles[triangleIndex++] = bottomRight;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;
            }
        }

        generatedMesh.vertices = vertices;
        generatedMesh.uv = uvs;
        generatedMesh.colors = colors;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateBounds();

        if (recalculateNormals)
            generatedMesh.RecalculateNormals();

        if (recalculateTangents)
            generatedMesh.RecalculateTangents();

        cachedMeshFilter.sharedMesh = generatedMesh;
    }

    [ContextMenu("Apply Material Controls")]
    public void ApplyMaterialControls()
    {
        CacheComponents();

        if (cachedMeshRenderer == null)
            return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        cachedMeshRenderer.GetPropertyBlock(propertyBlock);

        Color materialColor = Color.Lerp(Color.white, stoneBlendColor, stoneBlendAmount);
        materialColor.a = opacity;
        propertyBlock.SetColor(BaseColorId, materialColor);
        propertyBlock.SetColor(ColorId, materialColor);
        propertyBlock.SetFloat(SmoothnessId, 1.0f - roughness);
        propertyBlock.SetFloat(FrescoOpacityId, opacity);
        propertyBlock.SetColor(StoneBlendColorId, stoneBlendColor);
        propertyBlock.SetFloat(StoneBlendAmountId, stoneBlendAmount);

        cachedMeshRenderer.SetPropertyBlock(propertyBlock);
    }

    private float GetDepthMask(float horizontalFalloff, float verticalFalloff)
    {
        switch (depthCurveAxis)
        {
            case CurveAxis.Vertical:
                return verticalFalloff;
            case CurveAxis.Both:
                return horizontalFalloff * verticalFalloff;
            default:
                return horizontalFalloff;
        }
    }

    private float GetEdgeFade(float u, float v)
    {
        if (edgeFade <= 0.0001f)
            return 1.0f;

        float left = SmoothStep(0.0f, edgeFade, u);
        float right = SmoothStep(0.0f, edgeFade, 1.0f - u);
        float bottom = SmoothStep(0.0f, edgeFade, v);
        float top = SmoothStep(0.0f, edgeFade, 1.0f - v);
        return left * right * bottom * top;
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        if (Mathf.Approximately(edge0, edge1))
            return value >= edge1 ? 1.0f : 0.0f;

        float t = Mathf.Clamp01((value - edge0) / (edge1 - edge0));
        return t * t * (3.0f - 2.0f * t);
    }

    private void CacheComponents()
    {
        if (cachedMeshFilter == null)
            cachedMeshFilter = GetComponent<MeshFilter>();

        if (cachedMeshRenderer == null)
            cachedMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void DestroyGeneratedMesh()
    {
        if (Application.isPlaying)
            Destroy(generatedMesh);
        else
            DestroyImmediate(generatedMesh);

        generatedMesh = null;
    }

#if UNITY_EDITOR
    private void QueueEditorRegeneration()
    {
        if (regenerationQueued)
            return;

        regenerationQueued = true;
        UnityEditor.EditorApplication.delayCall += DelayedEditorRegeneration;
    }

    private void DelayedEditorRegeneration()
    {
        regenerationQueued = false;

        if (this == null)
            return;

        GenerateMesh();
        ApplyMaterialControls();
    }
#endif

    private void OnDrawGizmosSelected()
    {
        if (!drawPreviewGizmos)
            return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Gizmos.color = gizmoColor;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Bounds bounds = meshFilter.sharedMesh.bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.matrix = oldMatrix;
    }
}
