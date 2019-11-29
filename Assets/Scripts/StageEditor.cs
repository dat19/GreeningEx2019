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
        Vector3 mpos = myCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        Vector3 cpos = myCam.transform.position;

        mpos.x = Mathf.Round(mpos.x);
        mpos.y = Mathf.Round(mpos.y);

        if (Input.GetMouseButtonDown(0))
        {
            GameObject clone = PrefabUtility.InstantiatePrefab(mapChipPrefabs[(int)selectedMapChip]) as GameObject;
            clone.transform.position = mpos;
        }

        //カメラ移動
        cpos.x = cpos.x + (Input.GetAxisRaw("Horizontal") / 4);
        cpos.y = cpos.y + (Input.GetAxisRaw("Vertical") / 4);
        myCam.transform.position = cpos;
    }
}
