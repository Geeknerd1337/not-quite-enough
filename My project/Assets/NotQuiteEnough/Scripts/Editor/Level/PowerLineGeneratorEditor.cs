using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PowerLineGenerator))]
public class PowerLineGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PowerLineGenerator generator = (PowerLineGenerator)target;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        bool changed = EditorGUI.EndChangeCheck();

        if (GUILayout.Button("Generate Catenary") || (changed && generator.autoUpdate))
        {
            if (generator.endPoint == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign an end point", "OK");
                return;
            }

            Vector3[] points = generator.GenerateCatenaryPoints();
            if (points != null)
            {
                LineRenderer lineRenderer = generator.GetComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false; // Important: Use local space
                lineRenderer.positionCount = points.Length;
                lineRenderer.SetPositions(points);
            }
        }
    }
}