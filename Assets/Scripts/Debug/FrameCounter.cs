using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCounter : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 100, 30), Time.frameCount.ToString());
    }
}
