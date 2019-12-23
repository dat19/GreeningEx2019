using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreeningEx2019
{
    public class Version : MonoBehaviour
    {
        void Start()
        {
            TMPro.TextMeshProUGUI vertex = GetComponent<TMPro.TextMeshProUGUI>();
            vertex.text = "Ver " + Application.version;
        }
    }
}