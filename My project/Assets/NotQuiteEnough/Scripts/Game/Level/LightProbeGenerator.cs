using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
[RequireComponent(typeof(BoxCollider))]
public class LightProbeGenerator : MonoBehaviour
{
    public float resolution = 1f; // Distance between probes
    public bool visualizeProbes = true;

    private void OnDrawGizmos()
    {
        if (!visualizeProbes) return;

        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 size = box.size;
        Vector3 center = box.center;

        // Calculate number of probes in each dimension
        int xCount = Mathf.Max(2, Mathf.CeilToInt(size.x / resolution));
        int yCount = Mathf.Max(2, Mathf.CeilToInt(size.y / resolution));
        int zCount = Mathf.Max(2, Mathf.CeilToInt(size.z / resolution));

        // Draw probe positions
        Gizmos.color = Color.yellow;
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                for (int z = 0; z < zCount; z++)
                {
                    Vector3 position = transform.TransformPoint(
                        center + Vector3.Scale(new Vector3(
                            (x / (float)(xCount - 1) - 0.5f) * size.x,
                            (y / (float)(yCount - 1) - 0.5f) * size.y,
                            (z / (float)(zCount - 1) - 0.5f) * size.z
                        ), Vector3.one));

                    Gizmos.DrawWireSphere(position, 0.1f);
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LightProbeGenerator))]
public class LightProbeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LightProbeGenerator generator = (LightProbeGenerator)target;

        if (GUILayout.Button("Generate Light Probes"))
        {
            BoxCollider box = generator.GetComponent<BoxCollider>();
            Vector3 size = box.size;
            Vector3 center = box.center;

            // Calculate number of probes in each dimension
            int xCount = Mathf.Max(2, Mathf.CeilToInt(size.x / generator.resolution));
            int yCount = Mathf.Max(2, Mathf.CeilToInt(size.y / generator.resolution));
            int zCount = Mathf.Max(2, Mathf.CeilToInt(size.z / generator.resolution));

            // Create positions array
            List<Vector3> positions = new List<Vector3>();

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        Vector3 position = generator.transform.TransformPoint(
                            center + Vector3.Scale(new Vector3(
                                (x / (float)(xCount - 1) - 0.5f) * size.x,
                                (y / (float)(yCount - 1) - 0.5f) * size.y,
                                (z / (float)(zCount - 1) - 0.5f) * size.z
                            ), Vector3.one));

                        positions.Add(position);
                    }
                }
            }

            // Create or get LightProbeGroup
            LightProbeGroup probeGroup = generator.GetComponent<LightProbeGroup>();
            if (probeGroup == null)
            {
                probeGroup = generator.gameObject.AddComponent<LightProbeGroup>();
            }

            // Set positions
            probeGroup.probePositions = positions.ToArray();

            Debug.Log($"Generated {positions.Count} light probes");
        }
    }
}
#endif