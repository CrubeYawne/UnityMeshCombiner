using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombineForExport))]
public class CombineForExportEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate"))
        {
            var localT = (CombineForExport)target;

            localT.RunGenerator();
        }

         if(GUILayout.Button("Remove Generated"))
        {
            var localT = (CombineForExport)target;

            localT.RemoveGenerated();
        }

         if(GUILayout.Button("Combine Target"))
        {
            var localT = (CombineForExport)target;

            localT.CombineTarget();
        }

        if(GUILayout.Button("Combine Target Sub Mesh"))
        {
            var localT = (CombineForExport)target;

            localT.CombineTargetSubMesh();
        }

         if(GUILayout.Button("Check Generated"))
        {
            var localT = (CombineForExport)target;

            localT.CheckGenerated();
        }

         if(GUILayout.Button("Combine Generated"))
        {
            var localT = (CombineForExport)target;

            localT.CombineGenerated();
        }

        if(GUILayout.Button("Cleanup Bad Materials"))
        {
            var localT = (CombineForExport)target;

            localT.CleanupBadSubMaterials();
        }
    }
}
