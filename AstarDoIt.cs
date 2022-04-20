using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarDoIt : MonoBehaviour
{
    private List<MapNode> OpenList = new List<MapNode>();
    private List<MapNode> CloseList = new List<MapNode>();
    private ArrayList way = new ArrayList();
    private MapSE map = new MapSE();
    public Vector3[] AstarFindWays(Vector3 start,Vector3 end)
    {
        //return null;
        //清空列表
        OpenList.Clear();
        CloseList.Clear();
        way.Clear();
        //初始化起点和终点；
        MapNode Start = new MapNode((int)start.x, (int)start.z, 0, 0, null);//初始值为0/null
        //Start.X = (int)start.x;
        //Start.Z = (int)start.z;
        //Start.G = 0;

        MapNode End = new MapNode((int)end.x, (int)end.z, 0, 0, null);
        //End.X = (int)end.x;
        //End.Z = (int)end.z;
        //End.G = 0;

        //把起始点加入OpenList；
        OpenList.Add(Start);
        //寻找终点，openlist未出现end或者还有node时继续循环。
        //print(Start.F)==0;
        //return null;
        while (OpenList.Count == 0 ||!IsEndPointInOpen(End))
        {
            //获取F最小的点
            //print("2");
            //返回open中F最小的对象
            MapNode minFNode = FindMinFNodeInOpen();

            //print("3");
            //if (minFNode == null)
            //return null;
            OpenList.Remove(minFNode);
            CloseList.Add(minFNode);
            //break;
            CheckNearNode(minFNode,End);
            
        }
        
        MapNode EndNode = GetTheOpenNode(End.X, End.Z);
        Vector3 WayNode = new Vector3(EndNode.X, 0, EndNode.Z);
        way.Add(WayNode);
        //父节点不为空时，把每个父节点的pos向量放入way中
        while (EndNode.parent != null)
        {
            WayNode[0] = EndNode.parent.X;
            WayNode[2] = EndNode.parent.Z;
            way.Add(WayNode);

            EndNode = EndNode.parent;
        }

        //倒序返回
        Vector3[] ways = new Vector3[way.Count];
        for(int i =0; i< way.Count; i++)
        {
            ways[i] = (Vector3)way[way.Count - 1 - i];
        }
        way.Clear();
        OpenList.Clear();
        CloseList.Clear();
        return ways;
    }
    private void CheckNearNode(MapNode thisnode, MapNode endnode)
    {
        for(int i = thisnode.X - 1; i <= thisnode.X + 1; i++)
        {
            for (int j = thisnode.Z - 1; j <= thisnode.Z + 1; j++)//j++写成了i++
            {
                if (i < map.StartX || i > map.EndX || j < map.StartZ || j > map.EndZ)
                    continue;
                if (IsBarNode(i, j))
                    continue;
                if (IsInClose(i, j) || (i == thisnode.X && j == thisnode.Z))
                    continue;
                
                //还未在OpenList，初始化该点。
                if (!IsInOpen(i, j))
                {
                     MapNode NewNode = new MapNode(i, j, 0, 0, thisnode);
                    NewNode.G = CalG(NewNode, thisnode);
                    NewNode.H = CalH(NewNode, endnode);

                    OpenList.Add(NewNode);

                }
                //已经在openlist 更新g和parents
                else
                {
                    MapNode oldnode = GetTheOpenNode(i, j);
                    if(oldnode == null)
                    {
                        System.Console.Write("not found in OpenList");
                        return;
                    }
                    int G = CalG(oldnode, thisnode);
                    if (G < oldnode.G)
                    {
                        oldnode.G = G;
                        oldnode.parent = thisnode;
                    }
                }

            }
        }
    }
    private int CalH(MapNode node,MapNode end)
    {
        return (Mathf.Abs(node.X - end.X) + Mathf.Abs(node.Z - end.Z))*10;//此处未*10 bug1
    }
    private int CalG(MapNode node,MapNode parent)
    {
        if (node.X == parent.X || node.Z == parent.Z)
            return parent.G + 10;//在水平或垂直
        else
            return parent.G + 14;//对角

    }

    private MapNode GetTheOpenNode(int x,int z)
    {
        foreach(var v in OpenList)
        {
            if (v.X == x && v.Z == z)
            {
                return v;
            }
        }
        return null;
    }
    private bool IsInOpen(int i, int j)
    {
        
        foreach (var v in OpenList)
        {
            if (v.X == i && v.Z == j)
                return true;
        }
        return false;
    }
    private bool IsInClose(int x,int z)
    {
        //MapNode closenode = new MapNode(i,j,)
        foreach(var v in CloseList)
        {
            if (v.X == x && v.Z == z)
                return true;
        }
        return false;
    }
    private bool IsBarNode(int x,int z)
    {
        Vector3 position = new Vector3(x, 0, z);
        Collider[] colliders = Physics.OverlapSphere(position, 1, 1<<8);
        if(colliders.Length>0)
            return true;
        return false;

    }
    private bool IsEndPointInOpen(MapNode end)
    {
        foreach (var v in OpenList)
        {
            if (v.X == end.X && v.Z == end.Z)
                return true;
        }
        return false;
    }
    private MapNode FindMinFNodeInOpen()
    {
        MapNode minF = null;
        //MapNode minF = new MapNode(0,0,0,0,null);
        foreach (MapNode v in OpenList)
        {
            if (minF == null || minF.F > v.F)
                minF = v;
        }
        return minF;
    }


}

public class MapSE
{
    public int StartX;
    public int StartZ;
    public int EndX;
    public int EndZ;
    
    public MapSE()
    {
        StartX = -100;
        StartZ = -100;
        EndX = 100;
        EndZ = 100;
    }
    public MapSE(int Sx, int Sz, int Ex, int Ez)
    {
        StartX = Sx;
        StartZ = Sz;
        EndX = Ex;
        EndZ = Ez;
    }
}
public class MapNode
{
    public int X { get; set; }
    public int Z { get; set; }
    private int g;
    public int G
    {
        get
        {
            return g;
        }
        set
        {
            g = value;
            f = g + h;
        }
    }

    private int h;
    public int H
    {
        get
        {
            return h;
        }
        set
        {
            h = value;
            f = g + h;
        }
    }

    private int f;
    public int F
    {
        get
        {
            return f;
        }
    }
    public MapNode parent { get; set; }    
    public MapNode() { }
    public MapNode(int x,int z,int g,int h,MapNode parent)
    {
        X = x;
        Z = z;
        G = g;
        H = h;
        this.parent = parent;
    }
    

}
