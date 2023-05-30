using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.thorsdayorigin.tools
{
    /// <summary>
    /// This class is used to group 'like' mesh objects for combination
    /// </summary>
    public class MeshGrouping
    {
        /// <summary>
        /// collection of mesh objects to combine
        /// </summary>
        public List<MeshFilter> item_list;

        /// <summary>
        /// Common name for mesh
        /// </summary>
        public string mesh_name;

        /// <summary>
        /// List of materials assigned to mesh
        /// </summary>
        public Material[] materials;
    }
}