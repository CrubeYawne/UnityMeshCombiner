using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class CombineForExport : MonoBehaviour
{

    public GameObject generatedContainer;

    public GameObject targetFilter;

    public bool merge_submeshes = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CleanupBadSubMaterials()
    {
        CleanupBadSubMaterialsRecursive(this.transform);
    }

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

    public void CombineGenerated()
    {
        var allFilters = generatedContainer.GetComponentsInChildren<MeshFilter>();

        var potentialList = new List<MeshGrouping>();

        foreach(var f in allFilters)
        {
            var r = f.GetComponent<Renderer>();

            if(r  != null)
            {
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
            Debug.Log(p.mesh_name + " " + p.item_list.Count());

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

    private class MeshGrouping
    {
        public List<MeshFilter> item_list;

        public string mesh_name;

        public Material[] materials;
    }

    private class SubMeshGrouping
    {
        public List<Mesh> item_list;

        public string mesh_name;

        public int subMeshIndex;

        public Material targetMaterial;

    }

    private class CombineCombo
    {
        public MeshFilter mf;
        public Material[] materialList;
    }

    public void CombineTarget()
    {
        var allMeshes = this.gameObject.GetComponentsInChildren<MeshFilter>();

        var combineGrouping = new List<MeshGrouping>();

        foreach(var m in allMeshes)
        {
            var mRen = m.GetComponent<Renderer>();

            if(targetFilter != null)
            {
                if(m.sharedMesh.name != targetFilter.GetComponent<MeshFilter>().sharedMesh.name)
                {
                    continue;
                }
            }

            if(mRen == null)
                continue;

            if(mRen.sharedMaterials.Count() > 1)//test
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

    public void CombineTargetSubMesh()
    {
        var allMeshes = this.gameObject.GetComponentsInChildren<MeshFilter>();

        var combineGrouping = new List<MeshGrouping>();

        foreach(var m in allMeshes)
        {
            var mRen = m.GetComponent<Renderer>();

            if(targetFilter != null)
            {
                if(m.sharedMesh.name != targetFilter.GetComponent<MeshFilter>().sharedMesh.name)
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

    private void RecursiveCheck(Transform t_child)
    {
        /*
        foreach(Transform t in t_child)
            if (t.gameObject.activeSelf)
                RecursiveCheck(t);            

        var itemRenderer = t_child.GetComponent<Renderer>();        

        if(itemRenderer != null && itemRenderer.sharedMaterial != null)
        {
            var top_mesh = t_child.GetComponent<MeshFilter>();

            if(top_mesh != null)
            {
                var matchingMeshes = mesh_collection.Where( t => 
                    t.mesh_name == top_mesh.sharedMesh.name);

                MeshGrouping existingSB = null;

                foreach(var mm in matchingMeshes)
                {
                    if(mm.targetMaterials.Count() != itemRenderer.sharedMaterials.Count())
                        continue;
                    else
                    {
                        int match_count = 0;

                        for(int i=0; i != mm.targetMaterials.Count(); ++i)
                        {
                            if(mm.targetMaterials[i].name == itemRenderer.sharedMaterials[i].name)
                            {
                                match_count++;
                            }
                            
                        }

                        if(match_count == mm.targetMaterials.Count())
                        {
                            existingSB = mm;
                        }

                    }

                    if(existingSB != null)
                        break;
                }

                

                if(existingSB == null)
                {
                    existingSB = new MeshGrouping();
                    existingSB.mesh_name = top_mesh.sharedMesh.name;
                    existingSB.item_list = new List<CombineCombo>();
                    existingSB.targetMaterials = itemRenderer.sharedMaterials;
                    existingSB.material_count = itemRenderer.sharedMaterials.Count();                    
                    mesh_collection.Add(existingSB);
                }

                var cc = new CombineCombo();
                cc.mf = top_mesh;
                cc.materialList = itemRenderer.sharedMaterials;
                existingSB.item_list.Add(cc);
            }
        }
        */
    }

    /*
    private Mesh CombineMeshes(List<MeshFilter> meshes)
    {
        var combine = new CombineInstance[meshes.Count];
        for (int i = 0; i < meshes.Count; i++)
        {
            var loopMesh = meshes[i].mf;
            if(loopMesh == null || loopMesh.sharedMesh == null)
            {
                Debug.LogWarning("Dead mesh " + i, loopMesh);
                continue;
            }

            if(meshes[i].materialList.Length != loopMesh.sharedMesh.subMeshCount)
            {
                Debug.LogWarning("Material list doesn't match submesh count", loopMesh);
            }

            if(!loopMesh.sharedMesh.isReadable)
            {
                Debug.LogWarning("Issue with: " + loopMesh.name, loopMesh.gameObject);

                SerializedObject s = new SerializedObject(loopMesh.sharedMesh);
                s.FindProperty("m_IsReadable").boolValue = true;              

                Debug.LogWarning(string.Format("{0}={1}", loopMesh.name, loopMesh.sharedMesh.isReadable));

            }
            combine[i].mesh = loopMesh.sharedMesh;
            combine[i].transform = loopMesh.transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(combine, true);
        return mesh;
    }
    */

    private List<MeshGrouping> mesh_collection = new List<MeshGrouping>();

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

    public void RunGenerator()
    {
        if(generatedContainer ==null)
        {
            generatedContainer = new GameObject();
            generatedContainer.name = "#Generated#";
        }

        mesh_collection.Clear();

        foreach(Transform t in this.transform)
        {
            if (t.gameObject.activeSelf)
                RecursiveCheck(t);
        }

        RemoveGenerated();
        

        foreach(var smg in mesh_collection)
        {
            var created_child = new GameObject();
            var mr = created_child.AddComponent<MeshRenderer>();
            created_child.AddComponent<MeshFilter>();
            created_child.transform.SetParent(generatedContainer.transform);

            created_child.name = string.Format("{0}-{1}", smg.mesh_name, smg.materials[0].name);

            mr.sharedMaterials = smg.materials;
            
            Debug.Log(string.Format("combineCount: {0} for {1}", mesh_collection.Count, smg.mesh_name));
            //created_child.GetComponent<MeshFilter>().sharedMesh = CombineMeshes(smg.item_list);
        }
    }

    // Update is called once per frame    
    void Update()
    {
        
    }
}
