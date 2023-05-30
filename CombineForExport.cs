using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace com.thorsdayorigin.tools
{
    /// <summary>
    /// This is a component used to combine child gameobject meshes
    /// </summary>
    public class CombineForExport : MonoBehaviour
    {

        [Tooltip("Where generated meshes will be created under. Auto-generated.")]
        public GameObject generatedContainer;

        [Tooltip("Use to limit which objects meshes to combine. Can be empty")]
        public GameObject targetGameObjectFilter;

    
        /// <summary>
        /// Remove objects with more materials that submesh count. Runs on self.
        /// </summary>
        public void CleanupBadSubMaterials()
        {
            CleanupBadSubMaterialsRecursive(this.transform);
        }

        /// <summary>
        /// This function checks that all generated meshes have only 1 mesh. Having more than 1 mesh means the combine most likely didn't work correctly.
        /// </summary>
        public void CheckGenerated()
        {
            var allFilters = generatedContainer.GetComponentsInChildren<MeshFilter>();

            var potentialList = new List<MeshGrouping>();

            foreach(var f in allFilters)
            {
                var r = f.GetComponent<Renderer>();

                if(r  != null)
                {
                    if(r.sharedMaterials.Count() > 1)
                        Debug.Log("Has more than 1 material: " + r.name, r);
                }
            }


        }

        /// <summary>
        /// Iterates over all of the already "generated" meshes and combines what is similar
        /// </summary>
        public void CombineGenerated()
        {
            var allFilters = generatedContainer.GetComponentsInChildren<MeshFilter>();

            var potentialList = new List<MeshGrouping>();

            foreach(var f in allFilters)
            {
                var r = f.GetComponent<Renderer>();

                if(r  != null)
                {
                    if(f.sharedMesh == null)
                    {
                        Debug.LogError("Empty mesh?", f.gameObject);
                        break;
                    }

                    if(r.sharedMaterials[0] == null)
                    {
                        Debug.LogError("Empty material?", f.gameObject);
                        break;
                    }

                    var foundItem = potentialList.FirstOrDefault( t => t.mesh_name == f.sharedMesh.name
                    
                        && t.materials[0].name == r.sharedMaterials[0].name
                    );

                    if(foundItem == null)
                    {
                        foundItem = new MeshGrouping();
                        foundItem.item_list = new List<MeshFilter>();
                        foundItem.mesh_name = f.sharedMesh.name;
                        foundItem.materials = r.sharedMaterials;
                        potentialList.Add(foundItem);
                    }

                    if(foundItem.materials[0].name != r.sharedMaterials[0].name)
                    {
                        Debug.LogWarning("Mismatch materials: " + f.name, f);
                    }
                    else
                        Debug.Log("good materials " + r.name);

                    foundItem.item_list.Add(f);
                    
                }
            }

            foreach(var p in potentialList)
            {
                //Debug.Log(p.mesh_name + " " + p.item_list.Count());

                var combineList = new List<CombineInstance>();

                foreach(var i in p.item_list)
                {
                    var ci = new CombineInstance();
                    ci.mesh = i.sharedMesh;
                    ci.transform = i.transform.localToWorldMatrix;
                    combineList.Add(ci);

                    DestroyImmediate(i.gameObject);
                }

                var newMesh = new Mesh();
                //newMesh.name = p.mesh_name + "GEN";
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                newMesh.CombineMeshes(combineList.ToArray());

                var new_go = new GameObject();
                new_go.name = string.Format("{0}-{1}", p.mesh_name, p.materials[0].name);
                new_go.transform.SetParent(generatedContainer.transform);

                var mr = new_go.AddComponent<MeshRenderer>();
                var mf = new_go.AddComponent<MeshFilter>();

                mr.materials = p.materials;
                mf.sharedMesh = newMesh;
            }


        }

        /// <summary>
        /// Fixes gameobject meshes that have more material slots than submesh items
        /// </summary>
        /// <param name="t">Gameobject to check for meshes or children</param>
        private void CleanupBadSubMaterialsRecursive(Transform t)
        {
            foreach(Transform t_child in t)
                if (t_child.gameObject.activeSelf)
                    CleanupBadSubMaterialsRecursive(t_child);


            var itemRenderer = t.GetComponent<Renderer>();        

            if(itemRenderer != null)
            {
                var top_mesh = t.GetComponent<MeshFilter>();

                if(top_mesh == null)
                {
                    Debug.LogWarning("No filter?", top_mesh);
                }

                if(top_mesh.sharedMesh == null)
                {
                    Debug.LogWarning("empty mesh", top_mesh);
                }
                
                if(itemRenderer.sharedMaterials.Length != top_mesh.sharedMesh.subMeshCount)
                {
                    Debug.Log(string.Format("Mismatch with {0}; {1}-{2}",itemRenderer.name, itemRenderer.sharedMaterials.Length, top_mesh.sharedMesh.subMeshCount), itemRenderer.gameObject);

                    List<Material> newMatList = new List<Material>();
                    newMatList.Add( itemRenderer.sharedMaterial);

                    itemRenderer.sharedMaterials = newMatList.ToArray();
                }
            }
        }

        /// <summary>
        /// Will combine all gameobject children having a single material slot. Will use filter if set.
        /// </summary>
        public void CombineTarget()
        {
            CheckContainer();
            var allMeshes = this.gameObject.GetComponentsInChildren<MeshFilter>();

            var combineGrouping = new List<MeshGrouping>();

            foreach(var m in allMeshes)
            {
                var mRen = m.GetComponent<Renderer>();

                if(targetGameObjectFilter != null)
                {
                    if(m.sharedMesh.name != targetGameObjectFilter.GetComponent<MeshFilter>().sharedMesh.name)
                    {
                        continue;
                    }
                }

                if(mRen == null)
                    continue;

                if(mRen.sharedMaterials.Count() > 1 || mRen.sharedMaterials[0] == null)//test
                    continue;            

                var foundItem = combineGrouping.FirstOrDefault( t => t.mesh_name == 
                    m.sharedMesh.name
                    &&
                    t.materials.Count() == mRen.sharedMaterials.Count()
                    &&
                    t.materials[0].name == mRen.sharedMaterials[0].name
                );

                if(foundItem == null)
                {
                    foundItem = new MeshGrouping();
                    foundItem.mesh_name = m.sharedMesh.name;
                    foundItem.materials = mRen.sharedMaterials;
                    foundItem.item_list = new List<MeshFilter>();
                    combineGrouping.Add(foundItem);
                }

                foundItem.item_list.Add( m );
                        
                
            }


            foreach(var match in combineGrouping)
            {
                var combine = new List<CombineInstance>();

                foreach(var i in match.item_list)
                {
                    var ci = new CombineInstance();
                    ci.mesh = i.sharedMesh;
                    ci.transform = i.transform.localToWorldMatrix;
                    combine.Add(ci);
                }

                var gno = new GameObject();
                gno.name = match.mesh_name + "-" + match.item_list.Count();
                gno.transform.SetParent(generatedContainer.transform);

                var mr = gno.AddComponent<MeshRenderer>();
                var mf = gno.AddComponent<MeshFilter>();

                mr.materials = match.materials;

                var newMesh = new Mesh();
                //newMesh.name = match.mesh_name + "GEN";
                newMesh.CombineMeshes(combine.ToArray());

                mf.sharedMesh = newMesh;
                
                Debug.Log(string.Format("{0}-Material Count: {1} = {2}", match.mesh_name, match.materials.Count(), match.item_list.Count()));
            }

            


        }


        /// <summary>
        /// Combine gameobject meshes that have more than 1 material slot
        /// </summary>
        public void CombineTargetSubMesh()
        {
            CheckContainer();

            var allMeshes = this.gameObject.GetComponentsInChildren<MeshFilter>();

            var combineGrouping = new List<MeshGrouping>();

            foreach(var m in allMeshes)
            {
                var mRen = m.GetComponent<Renderer>();

                if(targetGameObjectFilter != null)
                {
                    if(m.sharedMesh.name != targetGameObjectFilter.GetComponent<MeshFilter>().sharedMesh.name)
                    {
                        continue;
                    }
                }

                

                if(mRen == null)
                    continue;

                if(mRen.sharedMaterials.Count() < 2)//test
                    continue;


                var potentialMatches = combineGrouping.Where( t => t.mesh_name == 
                    m.sharedMesh.name
                    &&
                    t.materials.Count() == mRen.sharedMaterials.Count()                
                );

                MeshGrouping foundItem = null;

                foreach(var pm in potentialMatches)
                {
                    int matching_materials = 0;

                    for(var im=0; im != mRen.sharedMaterials.Count(); ++im)
                    {
                        if(pm.materials[im].name == mRen.sharedMaterials[im].name)
                            matching_materials++;
                    }

                    if(matching_materials == pm.materials.Count())
                    {
                        foundItem = pm;
                        break;
                    }
                }

                if(foundItem == null)
                {
                    foundItem = new MeshGrouping();
                    foundItem.mesh_name = m.sharedMesh.name;
                    foundItem.materials = mRen.sharedMaterials;
                    foundItem.item_list = new List<MeshFilter>();
                    combineGrouping.Add(foundItem);
                }

                foundItem.item_list.Add( m );
                        
                
            }


            foreach(var match in combineGrouping)
            {
                for(var smi=0; smi != match.materials.Count();++smi)
                {

                    var combine = new List<CombineInstance>();

                    foreach(var i in match.item_list)
                    {
                        var ci = new CombineInstance();
                        ci.mesh = i.sharedMesh;
                        ci.subMeshIndex = smi;
                        ci.transform = i.transform.localToWorldMatrix;
                        combine.Add(ci);
                    }

                    var newMesh = new Mesh();
                    //newMesh.name = match.mesh_name + smi + "GEN";
                    newMesh.CombineMeshes(combine.ToArray());

                    var gno = new GameObject();
                    gno.name = match.mesh_name + "-" + match.item_list.Count() + "-SM"+smi;
                    gno.transform.SetParent(generatedContainer.transform);

                    var mr = gno.AddComponent<MeshRenderer>();
                    var mf = gno.AddComponent<MeshFilter>();

                    mr.material = match.materials[smi];
                    
                    mf.sharedMesh = newMesh;
                
                    Debug.Log(string.Format("{0}-Material Count: {1} = {2}", match.mesh_name, match.materials.Count(), match.item_list.Count()));
                }
            }

            


        }


        private void CheckContainer()
        {
            if(generatedContainer == null)
            {
                generatedContainer = new GameObject();
                generatedContainer.name = "#Generated#";
            }
        }
        

        public void RemoveGenerated()
        {
            if(generatedContainer == null)
                return;
            else
            {
                if(Application.isPlaying)   
                {
                    foreach(Transform t in generatedContainer.transform)
                        Destroy(t.gameObject);
                }
                else
                {
                    while(generatedContainer.transform.childCount > 0)
                        DestroyImmediate(generatedContainer.transform.GetChild(0).gameObject);
                }
            }

        }

        

    
    }
}