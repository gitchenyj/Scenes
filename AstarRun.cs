using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using Codice.Client.BaseCommands;

//A*Ѱ·
public class AstarRun : MonoBehaviour 
{
    private Map map = new Map();//���ӵ�ͼ
    
    //�����б�
    private List<MapPoint> open_List = new List<MapPoint>();
    //�ر��б�
    private List<MapPoint> close_List = new List<MapPoint>();
    //����һ��·������
    private ArrayList way = new ArrayList();

    //�ж�ĳ���Ƿ��ڿ����б��У�������������openlist�е�Ԫ��Mappoint��x��z������t/f
    private bool IsInOpenList(int x, int z)
    {
        foreach (var v in open_List)
        {
            if (v.x == x && v.z == z)
            return true;
        }
        return false;
    }

    //�ж�ĳ���Ƿ��ڹر��б���
    private bool IsInCloseList(int x, int z)
    {
        foreach (var v in close_List)
        {
            if (v.x == x && v.z == z)
            return true;
        }
        return false;
    }
    
    //�ӿ����б����ҵ��Ǹ�Fֵ��С�ĸ���
    private MapPoint FindMinFInOpenList()
    {
        MapPoint minPoint = null;
        foreach (var v in open_List)
        {
            if (minPoint == null || minPoint.GetF > v.GetF)
            minPoint = v;
        }
        return minPoint;
    }
    //�ӿ����б����ҵ����ӣ����ظ���Mappoint

    private MapPoint FindInOpenList(int x, int z)
    {
        foreach (var v in open_List)
        {
            if (v.x == x && v.z == z)
            return v;
        }
        return null;
    }
    /// <summary>

    /// a���㷨Ѱ·

    /// </summary>

    /// <returns>Ѱ����·���.</returns>

    /// <param name="starPoint">���    </param>

    /// <param name="targetPoint">�յ�</param>
    /// 
    public Vector3[] AStarFindWay(Vector3 starPoint, Vector3 targetPoint)
    {
        //�������
        way.Clear();
        open_List.Clear();
        close_List.Clear();

        //��ʼ��������
        MapPoint starMapPoint = new MapPoint();
        starMapPoint.x = (int)starPoint.x;
        starMapPoint.z = (int)starPoint.z;


        //��ʼ���յ����
        MapPoint targetMapPoint = new MapPoint();
        targetMapPoint.x = (int)targetPoint.x;
        targetMapPoint.z = (int)targetPoint.z;

        //����������ӵ������б���
        open_List.Add(starMapPoint);

        //Ѱ�����·��

        //��Ŀ��㲻�ڴ�·����ʱ���ߴ��б�Ϊ��ʱѭ��ִ��

        while (!IsInOpenList(targetMapPoint.x, targetMapPoint.z) || open_List.Count == 0)
        {
            //�ӿ����б����ҵ��Ǹ�Fֵ��С�ĸ���

            MapPoint minPoint = FindMinFInOpenList();
            if (minPoint == null)
                return null;
            //���õ�ӿ����б���ɾ����ͬʱ��ӵ��ر��б���
            open_List.Remove(minPoint);
            close_List.Add(minPoint);
            //���ĵ��ܱߵĸ���
            CheckPerPointWithMap(minPoint,targetMapPoint);

        }
        //�ڿ����б����ҵ��յ�

        MapPoint endPoint = FindInOpenList(targetMapPoint.x, targetMapPoint.z);
        Vector3 everyWay = new Vector3(endPoint.x, 0, endPoint.z);//���浥��·����
        way.Add(everyWay);//��ӵ�·��������
        //�����յ㣬�ҵ�ÿһ�����ڵ㣺��Ѱ����·

        while (endPoint.fatherPoint != null)
        {

            everyWay.x = endPoint.fatherPoint.x;
            everyWay.z = endPoint.fatherPoint.z;
            everyWay.y = 0;
            way.Add(everyWay);
            endPoint = endPoint.fatherPoint;

        }
        //��·������ӵ��������򲢷���
        Vector3[] ways = new Vector3[way.Count];
        for (int i = way.Count - 1; i >= 0; --i)
        {
           ways[way.Count - i - 1] = (Vector3)way[i];
        }
        //�������
        way.Clear();
        open_List.Clear();
        close_List.Clear();
        //���������·������
        return ways;
    }

    //�жϵ�ͼ��ĳ��������ǲ����ϰ���
    private bool IsBar(int x, int z)
    {
        
        Vector3 p = new Vector3(x, 0, z);
        //���õ��ܱ��Ƿ����ϰ���
        //�ϰ���㼶Ϊ8//�ú�������8�㼶p����Χ�뾶Ϊ1��collider
        Collider[] colliders = Physics.OverlapSphere(p, 1, 1 << 8);
        
        if (colliders.Length > 0)
        return true;//�õ���Χ���ϰ��˵���õ㲻��ͨ�������ϰ����
        return false;

    }
    //����ĳ�����Gֵ
    public int GetG(MapPoint p)

    {
        if (p.fatherPoint == null)
            return 0;
        if (p.x == p.fatherPoint.x || p.z == p.fatherPoint.z)
            return p.fatherPoint.G + 10;
        else
            return p.fatherPoint.G + 14;
    }

    //����ĳ�����Hֵ
    public int GetH(MapPoint p, MapPoint targetPoint)
    {
        return (Mathf.Abs(targetPoint.x - p.x) + Mathf.Abs(targetPoint.z - p.z)) * 10;
    }

    //���ĳ���ܱߵĸ��ӣ�����G parents
    private void CheckPerPointWithMap(MapPoint _point, MapPoint targetPoint)
    {
        for (int i = _point.x - 1; i <= _point.x + 1; ++i)
        {
            for (int j = _point.z - 1; j <= _point.z + 1; ++j)
            {
                //�޳�������ͼ�ĵ�
                if (i < map.star_X || i > map.end_X || j < map.star_Z || j > map.end_Z)
                continue;
                //�޳��õ����ϰ��㣺����Χ��ǽ�ĵ�
                if (IsBar(i, j))
                continue;
                //�޳��Ѿ����ڹر��б���߱����
                if (IsInCloseList(i, j) || (i == _point.x && j == _point.z))
                continue;
                //ʣ�µľ���û���жϹ��ĵ���
                if (IsInOpenList(i, j))
                {
                    //����õ��ڿ����б���
                    //�ҵ��õ�
                    MapPoint point = FindInOpenList(i, j);
                    int G = 0;
                    //������õ��µ��ƶ�����
                    if (point.x == _point.x || point.z == _point.z)
                        G = point.G + 10;
                    else
                        G = point.G + 14;
                    //����õ����Gֵ��ǰһ��С
                    if (G < point.G)
                    {
                        //�����µ�G��,�͸��ڵ㡣F���Զ�����
                        point.G = G;
                        point.fatherPoint = _point;
                    }
                }
                else
                {
                   //����õ㲻�ڿ����б���
                   //��ʼ���õ㣬�����õ���ӵ������б���
                    MapPoint newPoint = new MapPoint();
                    newPoint.x = i;
                    newPoint.z = j;
                    newPoint.fatherPoint = _point;
                    //����õ��Gֵ��Hֵ����ֵ
                    newPoint.G = GetG(newPoint);
                    newPoint.H = GetH(newPoint, targetPoint);
                    //����ʼ����ϵĸ�����ӵ������б���
                    open_List.Add(newPoint);
                }
            }
        }
    }
}

//��ͼ��
public class Map
{
    public int star_X;// ���������    
    public int star_Z;// ���������
    public int end_X;// �������յ�
    public int end_Z;//�������յ�
    public Map()
    {
        star_X = -100;
        star_Z = -100;
        end_X = 100;
        end_Z = 100;
    }
}

//ÿһ�����ӵ���Ϣ
public class MapPoint
{
    //F = G + H
    //G        �����A�ƶ���ָ��������ƶ����ۣ������ӵ������Ӵ��ۣ�ֱ��Ϊ10��б��Ϊ14(10����2)
    //H        ʹ�� Manhattan ���㷽����    ���㣨��ǰ����Ŀ�귽��ĺ�����+�������������ķ�������* 10
    public int x;//���ӵ�x����
    public int z;//���ӵ�z����
    public int G;
    public int H;
    public int GetF
    {
        get
        {
            return G + H;
        }
    }
    public MapPoint fatherPoint;//������
    public MapPoint() { }
    public MapPoint(int _x, int _z, int _G, int _H, MapPoint _fatherPoint)
    {
        this.x = _x;
        this.z = _z;
        this.G = _G;
        this.H = _H;
        this.fatherPoint = _fatherPoint;
    }

}
