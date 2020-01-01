using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    [CreateAssetMenu(fileName ="MapAttr", menuName ="GreeningEx2019/Create MapAttribute")]
    public class MapAttribute : ScriptableObject
    {
        [Tooltip("重ねて苗をおけるならtrue")]
        public bool overlapNae = false;
    }
}