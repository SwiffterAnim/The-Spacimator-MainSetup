using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineMeshController : MonoBehaviour
{
    [SerializeField]
    private SplineController splineController;

    [SerializeField]
    private SplineContainer splineContainer;

    [SerializeField]
    private GameObject circlePointVisualizer;

    [SerializeField]
    MeshFilter meshFilter;

    [SerializeField]
    private float meshWidth = 0.1f;

    [SerializeField]
    private int resolution = 1;

    public bool drawMesh;

    private float3 position;
    private float3 forward;
    private float3 upVector;
    private List<Vector3> mesh_vertsP1 = new List<Vector3>();
    private List<Vector3> mesh_vertsP2 = new List<Vector3>();
    private Mesh mesh;

    /*
        private void InstantiateVerts()
        {
            foreach (Vector3 point in mesh_vertsP1)
            {
                Instantiate(circlePointVisualizer, point, Quaternion.identity);
            }
            foreach (Vector3 point in mesh_vertsP2)
            {
                Instantiate(circlePointVisualizer, point, Quaternion.identity);
            }
        }
    */
    private void GetVerts()
    {
        mesh_vertsP1.Clear();
        mesh_vertsP2.Clear();
        int knotCount = splineContainer.Spline.Count;
        if (knotCount == 1)
        {
            return;
        }
        else if (knotCount > 1)
        {
            for (int i = 0; i < knotCount - 1; i++)
            {
                float startRatio = splineController.GetKnotRatioInSpline(i);
                float endRatio = splineController.GetKnotRatioInSpline(i + 1);

                for (int r = 0; r < resolution; r++)
                {
                    // Calculate the interpolation factor (t) based on resolution
                    float t = (float)r / resolution;
                    float interpolatedRatio = Mathf.Lerp(startRatio, endRatio, t);

                    // Sample points on the spline at the interpolated ratio
                    SampleSplineWitdh(interpolatedRatio, out Vector3 p1, out Vector3 p2);

                    // Add the vertices to the lists
                    mesh_vertsP1.Add(p1);
                    mesh_vertsP2.Add(p2);
                }
            }
            // Add the final knot to close the loop or end the spline
            float finalRatio = splineController.GetKnotRatioInSpline(knotCount - 1);
            SampleSplineWitdh(finalRatio, out Vector3 finalP1, out Vector3 finalP2);
            mesh_vertsP1.Add(finalP1);
            mesh_vertsP2.Add(finalP2);
        }
    }

    private void SampleSplineWitdh(float ratio, out Vector3 p1, out Vector3 p2)
    {
        splineContainer.Spline.Evaluate(ratio, out position, out forward, out upVector);

        Vector3 upVector3 = (Vector3)upVector;
        p1 = (Vector3)position + (upVector3.normalized * meshWidth);
        p2 = (Vector3)position + (-upVector3.normalized * meshWidth);
    }

    public void BuildMesh()
    {
        if (splineContainer.Spline.Count > 1)
        {
            GetVerts();
            // Check if the lists have the same number of points
            if (mesh_vertsP1.Count != mesh_vertsP2.Count)
            {
                Debug.LogError("Both sides of the spline must have the same number of points.");
                return;
            }

            // Reuse existing mesh if possible, otherwise create a new one
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            // Clear previous mesh data to avoid overlapping meshes
            mesh.Clear();

            // Total number of vertices is the sum of left and right side points
            int numVertices = mesh_vertsP1.Count + mesh_vertsP2.Count;

            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[(mesh_vertsP1.Count - 1) * 6]; // 6 indices per quad (2 triangles)
            Vector2[] uvs = new Vector2[numVertices];

            // Assign vertices from both sides
            for (int i = 0; i < mesh_vertsP1.Count; i++)
            {
                // Add left side vertices
                vertices[i * 2] = mesh_vertsP1[i];

                // Add right side vertices
                vertices[i * 2 + 1] = mesh_vertsP2[i];

                // Assign UV coordinates (can be customized based on how you want the texture to be applied)
                uvs[i * 2] = new Vector2(0, i / (float)(mesh_vertsP1.Count - 1)); // Left side UV
                uvs[i * 2 + 1] = new Vector2(1, i / (float)(mesh_vertsP2.Count - 1)); // Right side UV

                // Build triangles for each quad, except the last one
                if (i < mesh_vertsP1.Count - 1)
                {
                    // First triangle (left, right, next right)
                    triangles[i * 6] = i * 2;
                    triangles[i * 6 + 1] = i * 2 + 1;
                    triangles[i * 6 + 2] = i * 2 + 3;

                    // Second triangle (left, next right, next left)
                    triangles[i * 6 + 3] = i * 2;
                    triangles[i * 6 + 4] = i * 2 + 3;
                    triangles[i * 6 + 5] = i * 2 + 2;
                }
            }

            // Assign the vertices, triangles, and uvs to the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            // Optionally, calculate the mesh bounds and normals
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // Assign the mesh to the MeshFilter
            meshFilter.mesh = mesh;

            // Optional: Add or update MeshCollider to match the mesh
            MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }

            // Assign the generated mesh to the collider
            meshCollider.sharedMesh = mesh;
        }
    }

    public void BuildMesh2()
    {
        mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        int offset;

        int length = mesh_vertsP1.Count;

        for (int i = 1; i <= length; i++)
        {
            Vector3 p1 = mesh_vertsP1[i - 1];
            Vector3 p2 = mesh_vertsP2[i - 1];
            Vector3 p3 = mesh_vertsP1[i];
            Vector3 p4 = mesh_vertsP2[i];

            offset = 4 * (i - 1);

            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;

            verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
            tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
