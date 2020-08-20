using System.Collections.Generic;
using UnityEngine;

public class OctTree
{
    public Vector3 centerPos {get;}
    public float size {get;}
    public int capacity {get;}
    public bool subdiv {get; private set;}

    public OctTree parent {get; private set;}
    public OctTree[] nodes {get; private set;}

    List<GameObject> itemList;

    public OctTree(Vector3 worldCenterPos, float boxSize, int maxCapacity, OctTree nodeParent = null)
    {
        centerPos = worldCenterPos;
        size = boxSize;
        capacity = maxCapacity;
        subdiv = false;
        parent = nodeParent;

        itemList = new List<GameObject>();
    }

    bool Contains(Vector3 testPos) //check if an item is in the box
    {
        return ((centerPos.x - size < testPos.x && testPos.x < centerPos.x + size) &&
                (centerPos.y - size < testPos.y && testPos.y < centerPos.y + size) &&
                (centerPos.z - size < testPos.z && testPos.z < centerPos.z + size));
    }

    float half,x,y,z;
    OctTree NWU,NEU,SWU,SEU,NWD,NED,SWD,SED;
    void Subdivide() //subdivision method
    {
        half = size/2;
        x = centerPos.x;
        y = centerPos.y;
        z = centerPos.z;

        NWU = new OctTree(new Vector3(x-half,y+half,z+half), size/2, capacity, this);
        NEU = new OctTree(new Vector3(x+half,y+half,z+half), size/2, capacity, this);
        SWU = new OctTree(new Vector3(x-half,y+half,z-half), size/2, capacity, this);
        SEU = new OctTree(new Vector3(x+half,y+half,z-half), size/2, capacity, this);

        NWD = new OctTree(new Vector3(x-half,y-half,z+half), size/2, capacity, this);
        NED = new OctTree(new Vector3(x+half,y-half,z+half), size/2, capacity, this);
        SWD = new OctTree(new Vector3(x-half,y-half,z-half), size/2, capacity, this);
        SED = new OctTree(new Vector3(x+half,y-half,z-half), size/2, capacity, this);
        
        nodes = new OctTree[8] {NWU,NEU,SWU,SEU,NWD,NED,SWD,SED};

        subdiv = true;
    }

    public void Insert(GameObject item) //item insertion
    {
        /*
        1. Check if it contains it's pos
        2. If not subdivided and the size is still subdivideable, DO IT!
        3. If subdivided, PUSH the insertion to other nodes
        4. If not subdivided, ADD the item to the list 
        */
        if (!Contains(item.transform.position)) return;

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
        else
        {
            itemList.Add(item);
        }

    }

    bool ItemsInChildren() //looks for any other item in all child-branches
    {
        /*
        1. Loop through all nodes of SUBDIVIDED branch
        2. If it's node is subdivided, loop through all it's nodes again (has to be subidivided 
        to look for children - otherwise look directly for amount of items in list)
        3. If the node is NOT subdivided, check amount of items in list.
        
        (In all steps we are looking just for single one item -> returns true and ends process)
        */

        foreach (var node in nodes)
        {
            if (node.subdiv)
            {
                foreach (var n in node.nodes)
                {
                    if (n.subdiv) 
                    {
                        if (n.ItemsInChildren()) return true;
                    }
                    else if (n.itemList.Count > 0)
                    {
                        return true;
                    }
                }
            }
            else if (node.itemList.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void Remove(GameObject item) //removes item from the tree + reworks branches appropriately
    {
        /*
        1. Check if it contains it's pos
        2. If subdivided, push search to other nodes - don't continue for god's sake! (hell let's loose)
        3. If not subdivided and the item is contained in this list, REMOVE it and check for other 
        items in this parent. If non are found, de-subdivide parent and push remove to the parent.
        4. (step for re-recall of remove on parent) 
        If the parent is not null (orig branch), check for other items in it's parent. If non are 
        found, de-subdivide it's parent and push remove to the parent.

        LOOKS AWFUL!!! -> IT WAS AWFUL TO THINK ABOUT ALL THE RECURSION!!!
        */

        if (!Contains(item.transform.position)) return;

        if (subdiv)
        {
            foreach (var node in nodes)
            {
                node.Remove(item);
            }
            return;
        }

        if (itemList.Contains(item))
        {
            itemList.Remove(item);

            if (!parent.ItemsInChildren())
            {
                this.parent.subdiv = false;
                this.parent.Remove(item);
            }  
        }
        else
        {
            if (this.parent != null)
            {
                if (!parent.ItemsInChildren())
                {
                    this.parent.subdiv = false;
                    this.parent.Remove(item);
                }
            }
        }
    }

    // public List<OctTree> FindFullParent(GameObject item) //test for finding parent branches of items
    // {
    //     List<OctTree> found = new List<OctTree>();

    //     if (!Contains(item.transform.position)) return found;
     
    //     if (subdiv)
    //     {
    //         foreach (var node in nodes)
    //         {
    //             found.AddRange(node.FindFullParent(item));
    //         }
    //         return found;
    //     }

    //     if (itemList.Contains(item))
    //     {
    //         found.AddRange(parent.nodes);
    //         foreach (var n in parent.nodes)
    //         {
    //             foreach (var obj in n.itemList)
    //             {
    //                 obj.transform.gameObject.GetComponent<Renderer>().material.color = Random.ColorHSV();
    //             }
    //         }
    //     }
    //     return found;
    // }
  
    bool IsNeighbor(Vector3 searchCenter, float searchSize) //check if this cube is neighbor of searched one
    {
        /* 
        BUNCH OF POSITIONAL RULES TO TEST THE POS RELATION OF CHECKED CELLS

        ABLE TO FIND NEIGHBORING CELLS OF ALL SIZES (the center is always the smallest)
        */
        
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

    public List<OctTree> FindNeighborCells(Vector3 searchCenter, float searchSize) //returns found neighboring cells (6)
    {
        /*
        1. FOUND list
        2. If subdivided, loop through all nodes and try to find neighboring cells there, add the 
        possible range of found to FOUND list
        3. If not subdivide, do the search (from center search pos, size - of the cell)
        4. If found, add to FOUND list
        5. Return current FOUND  
        */

        List<OctTree> found = new List<OctTree>();
        if (subdiv)
        {
            foreach (var node in nodes)
            {
                found.AddRange(node.FindNeighborCells(searchCenter, searchSize));
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
