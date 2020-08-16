using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTWrapper : MonoBehaviour
{  
    public bool DRAWTREE;
    public bool DRAWNEIGHBOR;

    public float boxSize;
    public int treeCapacity;
    public float overlapBox;

    OctTree OT;

    public List<GameObject> objects;
    public GameObject prefab;

    public objectPooler pooler;

    // Start is called before the first frame update
    void Start()
    {
        objects = new List<GameObject>();
    }

    private List<OctTree> found;
    void Update() 
    {     
        if(Input.GetMouseButton(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit)) {
                Vector3 pos = hit.transform.position;
                float size = hit.transform.localScale.x + 0.1f;
                Debug.DrawLine(pos,new Vector3(pos.x + size, pos.y, pos.z),Color.red,10f);
                Debug.DrawLine(pos,new Vector3(pos.x - size, pos.y, pos.z),Color.red,10f);

                Debug.DrawLine(pos,new Vector3(pos.x, pos.y + size, pos.z),Color.red,10f);
                Debug.DrawLine(pos,new Vector3(pos.x, pos.y - size, pos.z),Color.red,10f);

                Debug.DrawLine(pos,new Vector3(pos.x, pos.y, pos.z + size),Color.red,10f);
                Debug.DrawLine(pos,new Vector3(pos.x, pos.y, pos.z - size),Color.red,10f);

                found = new List<OctTree>();
                found.AddRange(OT.FindNeighborCells(pos,size));
            }
        }

        if(Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit)) {
                Debug.DrawRay(Camera.main.transform.position, hit.point - Camera.main.transform.position,Color.red, 1);
                
                Vector3 position = hit.point;
                Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up,hit.normal);
                if (Physics.OverlapBox(position, new Vector3(overlapBox,overlapBox,overlapBox), spawnRot).Length > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        position += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                        if (!(Physics.OverlapBox(position, new Vector3(overlapBox,overlapBox,overlapBox), spawnRot, LayerMask.NameToLayer("plane")).Length > 0))
                        {
                            if (position.y < 0.5f) return;

                            var obj = pooler.GetPooledObject(prefab);
                            obj.transform.position += position;
                            obj.transform.rotation = spawnRot;
                            obj.SetActive(true);
                            objects.Add(obj);
                            break;
                        }
                    }
                }
                else
                {
                    if (position.y < 0) return;
                    
                    var obj = pooler.GetPooledObject(prefab);
                    obj.transform.position += position;
                    obj.transform.rotation = spawnRot;
                    obj.SetActive(true);
                    objects.Add(obj);
                }                
            }
        }
    }

    void FixedUpdate() 
    {
        OT = new OctTree(this.transform.position, boxSize, treeCapacity);

        foreach (var item in objects)
        {
            OT.Insert(item);
        }
    }

    void DrawTree(OctTree tree, Color color)
    {
        try
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(tree.centerPos,new Vector3(tree.size*2,tree.size*2,tree.size*2));
            if (tree.subdiv)
            {
                foreach (var item in tree.nodes)
                {
                    DrawTree(item, color);
                }
            }
        }
        catch (System.Exception)
        {
            
        }
    }

    void DrawFound(Color color)
    {
        Gizmos.color = color;
        foreach (var item in found)
        {
            Gizmos.DrawWireCube(item.centerPos,new Vector3(item.size*2,item.size*2,item.size*2));
        }
    }

    void OnDrawGizmos() {
        if (DRAWTREE)
        {
            DrawTree(OT, Color.white);
        }
        if (DRAWNEIGHBOR)
        {
            if (found != null)
            {
                DrawFound(Color.green);
            }
        }
    }
}
