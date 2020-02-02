using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class EraseDuplicateObject : MonoBehaviour
{
    List<GameObject> duplicateObject = new List<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ListupDuplicate();
        }
    }

    void ListupDuplicate()
    {
        duplicateObject.Clear();

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Ground");
        List<GameObject> objectList = new List<GameObject>(gos);
        gos = GameObject.FindGameObjectsWithTag("DeadZone");
        for (int i=0;i<gos.Length;i++)
        {
            objectList.Add(gos[i]);
        }
        gos = GameObject.FindGameObjectsWithTag("Nae");
        for (int i = 0; i < gos.Length; i++)
        {
            objectList.Add(gos[i]);
        }

        for (int i=0;i<objectList.Count-1;i++)
        {
            bool matched = false;
            for (int j=i+1;j<objectList.Count && !matched;j++)
            {
                float dist = Vector3.Distance(objectList[i].transform.position, objectList[j].transform.position);
                if (Mathf.Approximately(dist, 0f))
                {
                    Debug.Log($"Entry {i} / {objectList[i].transform.position}");
                    matched = true;
                    duplicateObject.Add(objectList[i]);
                    objectList.RemoveAt(j);
                    break;
                }
            }
        }
    }

}
#endif
