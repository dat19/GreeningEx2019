using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StageEditor : MonoBehaviour
{
    [SerializeField]
    MapType selectedMapChip = MapType.GroundUp;
    [SerializeField]
    GameObject[] mapChipPrefabs = null;

    public enum MapType
    {
        GroundUp,
        GroundDown,
        WaterUp,
        WaterDown,
        FlowerLeaf,
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Camera myCam = Camera.main;
        Vector3 mpos = myCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cpos = myCam.transform.position;

        mpos.x = Mathf.Round(mpos.x);
        mpos.y = Mathf.Round(mpos.y);
        mpos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            PrefabUtility.InstantiatePrefab(mapChipPrefabs[(int)selectedMapChip]);
        }

        //カメラ移動
        cpos.x = cpos.x + (Input.GetAxisRaw("Horizontal") / 4);
        cpos.y = cpos.y + (Input.GetAxisRaw("Vertical") / 4);
        myCam.transform.position = cpos;
    }
}
