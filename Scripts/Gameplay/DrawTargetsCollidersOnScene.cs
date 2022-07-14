using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class DrawTargetsCollidersOnScene : MonoBehaviour
{

#if UNITY_EDITOR
    
    public bool drawSupport;
    public bool drawReset;
    public bool drawTargets;
    
    [Range(0.1f, 1.0f)] public float alphaSupport = 0.4f;
    [Range(0.1f, 1.0f)] public float alphaReset = 0.7f;
    [Range(0.1f, 1.0f)] public float alphaTarget = 0.25f;
    
    public Mesh mesh;
    public Material material;
    public Level level;
    private Color[] _colors;
    private bool _render;
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");


    private void OnEnable() {
 
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;

        level = FindObjectOfType<Level>();

        SetRandomColors();
    }

    private void SetRandomColors()
    {
        _colors = new Color[level.targets.Length * 3];
        //purple
        for (var index = 0; index < _colors.Length / 3; index++)
        {
            _colors[index] = Random.ColorHSV(0.8f, 0.95f, 0.5f, 0.7f, 0.3f, 1f);
            _colors[index] = new Color(_colors[index].r, _colors[index].g, _colors[index].b, alphaSupport);
        }
        //red
        for (var index = _colors.Length / 3; index < _colors.Length / 3 * 2; index++)
        {
            _colors[index] = Random.ColorHSV(0f, 0.07f, 0.5f, 0.7f, 0.3f, 1f);
            _colors[index] = new Color(_colors[index].r, _colors[index].g, _colors[index].b, alphaReset);
        }
        //blue
        for (var index = _colors.Length / 3 * 2; index < _colors.Length; index++)
        {
            _colors[index] = Random.ColorHSV(0.6f, 0.7f, 0.5f, 0.7f, 0.3f, 1f);
            _colors[index] = new Color(_colors[index].r, _colors[index].g, _colors[index].b, alphaTarget);
        }
    }

    private void OnDisable() {
 
        SceneView.duringSceneGui -= OnSceneGUI;
    }
 
    private void OnSceneGUI(SceneView sceneView) {
 
        if (_render) {
 
            _render = false;
            
            for (var i = 0; i < level.targets.Length; i++)
            {
                var target = level.targets[i];
                if (target.supportCollider && drawSupport)
                {
                    var supportColliders = target.supportCollider.GetComponents<BoxCollider>();
                    DrawCollidersArray(sceneView, supportColliders, _colors[i]);
                }

                if (target.resetCollider && drawReset)
                {
                    var colliderToReset = target.resetCollider.GetComponents<BoxCollider>();
                    DrawCollidersArray(sceneView, colliderToReset, _colors[i + level.targets.Length]);
                }

                if (target.targetCollider && drawTargets)
                {
                    var colliderToNext = target.targetCollider.GetComponents<BoxCollider>();
                    DrawCollidersArray(sceneView, colliderToNext, _colors[i + level.targets.Length * 2]);
                }
            }
        }
    }

    private void OnValidate()
    {
        SetRandomColors();
        _render = true;
    }

    private void DrawCollidersArray(SceneView sceneView, BoxCollider[] collidersArray, Color color)
    {
        foreach (var s in collidersArray)
        {
            Vector3[] vertices;
            Vector3[] normals = new Vector3[8];
            int[] triangles;
            var bounds = s.bounds;
            Vector3 heightVector = new Vector3(0, bounds.max.y - bounds.min.y, 0);
            Vector3 widthVector = new Vector3(bounds.max.x - bounds.min.x, 0, 0);

            /*
             *              6-------5
             *              |       |
             *              7-------4
             *                  
             *      2-------1      
             *      |       |   
             *      3-------0
             */
            vertices = new[]
            {
                bounds.min,
                bounds.min + heightVector,
                bounds.min + widthVector + heightVector,
                bounds.min + widthVector,
                bounds.max - heightVector - widthVector,
                bounds.max - widthVector,
                bounds.max,
                bounds.max - heightVector
            };

            triangles = new[]
            {
                0, 1, 2, //front
                2, 3, 0,
                7, 6, 5, //back
                5, 4, 7,
                5, 1, 0, //right
                0, 4, 5,
                7, 3, 2, //left
                2, 6, 7,
                5, 6, 2, //up
                2, 1, 5,
                0, 3, 7, //down
                7, 4, 0
            };

            for (int j = 0; j < vertices.Length; j++)
                normals[j] = Vector3.back;

            mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                normals = normals
            };
            
            var mat = new Material(Shader.Find("Standard"));
            mat.CopyPropertiesFromMaterial(material);
            mat.SetColor(Color1, color);
            Draw(sceneView.camera, Matrix4x4.identity, mat);
        }
    }

    private void OnDrawGizmos() {
 
        _render = true;
    }
 
    private void Draw(Camera camera, Matrix4x4 matrix, Material mat) {
 
        if (mesh && mat && camera) {
 
            Graphics.DrawMesh(mesh, matrix, mat, gameObject.layer, camera, 0, 
                new MaterialPropertyBlock(), ShadowCastingMode.Off, false);
        }
    }
    
#endif
    
}