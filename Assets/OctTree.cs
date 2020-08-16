using System.Collections.Generic;
using UnityEngine;

public class OctTree
{
    public Vector3 centerPos {get;}
    public float size {get;}
    public int capacity {get;}
    public bool subdiv {get; private set;}
    public OctTree[] nodes {get; private set;}

    List<GameObject> itemList;

    public OctTree(Vector3 worldCenterPos, float boxSize, int maxCapacity)
    {
        centerPos = worldCenterPos;
        size = boxSize;
        capacity = maxCapacity;
        subdiv = false;
        itemList = new List<GameObject>();
    }

    bool Contains(Vector3 testPos) //check if an item is in the box
    {
        return ((centerPos.x - size < testPos.x && testPos.x < centerPos.x + size) &&
                (centerPos.y - size < testPos.y && testPos.y < centerPos.y + size) &&
                (centerPos.z - size < testPos.z && testPos.z < centerPos.z + size));
    }

    public void Insert(GameObject item) //item insertion
    {
        if (!Contains(item.transform.position)) return;

        // if (itemList.Count < capacity)
        // {
        //     itemList.Add(item);
        // }
        // else
        // {
        //     if (!subdiv )
        //     {
        //         Subdivide();
        //     }

        //     for (int i = 0; i < nodes.Length; i++)
        //     {
        //         nodes[i].Insert(item);
        //     }
        // }

        if (!subdiv && size > item.transform.localScale.x/2)
        {
            Subdivide();
        }

        if (subdiv)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Insert(item);
            }
        }

    }

    float half,x,y,z;
    OctTree NWU,NEU,SWU,SEU,NWD,NED,SWD,SED;
    void Subdivide() //subdivision method
    {
        half = size/2;
        x = centerPos.x;
        y = centerPos.y;
        z = centerPos.z;

        NWU = new OctTree(new Vector3(x-half,y+half,z+half), size/2, capacity);
        NEU = new OctTree(new Vector3(x+half,y+half,z+half), size/2, capacity);
        SWU = new OctTree(new Vector3(x-half,y+half,z-half), size/2, capacity);
        SEU = new OctTree(new Vector3(x+half,y+half,z-half), size/2, capacity);

        NWD = new OctTree(new Vector3(x-half,y-half,z+half), size/2, capacity);
        NED = new OctTree(new Vector3(x+half,y-half,z+half), size/2, capacity);
        SWD = new OctTree(new Vector3(x-half,y-half,z-half), size/2, capacity);
        SED = new OctTree(new Vector3(x+half,y-half,z-half), size/2, capacity);
        
        nodes = new OctTree[8] {NWU,NEU,SWU,SEU,NWD,NED,SWD,SED};

        subdiv = true;
    }

    bool IsNeighbor(Vector3 searchCenter, float searchSize)
    {
        // return ((searchCenter.x < centerPos.x + size && searchCenter.x > centerPos.x - size) &&
        //         (searchCenter.y < centerPos.y + size && searchCenter.y > centerPos.y - size) &&
        //         (searchCenter.z < centerPos.z + size && searchCenter.z > centerPos.z - size));

        var SC = searchCenter;
        var CP = centerPos;
        var ss = searchSize/2;
        var s = size;
        
        if (((SC.x + ss > CP.x - s && SC.x + ss < CP.x) || (SC.x - ss < CP.x + s && SC.x - ss > CP.x)) && 
            (SC.y > CP.y - s && SC.y < CP.y + s) && 
            (SC.z > CP.z - s && SC.z < CP.z + s))
        {
            return true;
        }
        else if ((SC.x > CP.x - s && SC.x < CP.x + s) && 
                ((SC.y + ss > CP.y - s && SC.y + ss < CP.y) || (SC.y - ss < CP.y + s && SC.y - ss > CP.y)) && 
                (SC.z > CP.z - s && SC.z < CP.z + s))
        {
            return true;
        }
        else if ((SC.x > CP.x - s && SC.x < CP.x + s) && 
                (SC.y > CP.y - s && SC.y < CP.y + s) && 
                ((SC.z + ss > CP.z - s && SC.z + ss < CP.z) || (SC.z - ss < CP.z + s && SC.z - ss > CP.z)))
        {
            return true;  
        }
        return false;

    }

    public IEnumerable<OctTree> FindNeighborCells(Vector3 searchCenter, float searchSize)
    {
        List<OctTree> found = new List<OctTree>();
        if (subdiv)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                found.AddRange(nodes[i].FindNeighborCells(searchCenter, searchSize));
            }
            return found;
        }
        
        if (IsNeighbor(searchCenter, searchSize))
        {
            found.Add(this);
        }
        return found;

    }
}
