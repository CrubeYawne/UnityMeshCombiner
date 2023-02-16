# Unity Mesh Combiner

This is a Unity monobehaviour script that can be used to combine objects.

Steps:
1. Place CombineForExport component on parent of object set you with to combine (will include children)
2. [Optional] Set [generatedContainer] property to an object of your choice to fill with generated meshes
3. [Optional] Use "Cleanup Bad Materials" in editor control to remove any extra material slots on a mesh. This occurs when the submesh count doesn't match the material slot numbers. Normally when this occurs, the mesh has *more* material slots than submesh items and can safely be removed.
4. Use editor controls to combine meshes	
	- Use "Combine Target (Single Material Mode)" to combine objects that match the [targetGameObjectFilter] property of the CombineForExport component if set. Will not *remove* any gameobjects and will only include objects that have a single material slot. Will act on all if [targetGameObjectFilter] is null
	- Use "Combine Target Sub Mesh" to combine only objects that match the [targetGameObjectFilter] property of the CombineForExport component. Will not *remove* any gameobjects and will only include objects that have meshes with more than 1 material slot.
3. [Optional] Use "Combine Generated" to combine children objects in [generatedContainer]. Used after "Generate" or "Combine Target"/"Combine Target Submesh"
4. [Optional] Use "Check Generated" to verify that all generated meshes only have 1 material assigned. Extra material slots indicate the meshes may not have combined properly.
3. [Optional] Use editor control "Remove Generated" to clear generated meshes

**Note: Observe the console debug window for any logs indicating bad material assignments when running any above steps

You can then use Unity's Built in system to export the data to fbx [Here](https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/exporting.html#:~:text=Use%20Export%20To%20FBX%20(menu,the%20parent's%20hierarchy%20is%20exported.)