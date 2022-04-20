using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using game4automation;

public class PlayerCtrl : MonoBehaviour
{
    public GameObject wayLook;//寻路线的红点

    public GameObject Linex;//

    public GameObject Curvex;

    public GameObject AGV;

    public float moveSpeed = 10f;//角色前进速度

    private CharacterController cc;//角色控制器

    private Transform waysParent;//寻路线的放置位置
    private Transform linesParent;

    private Ray ray;//射线检测鼠标点击点

    private RaycastHit hit;

    private bool IsMove = false;//是否正在寻路过程
    //八个方向
    private bool IsOdd(int num)
    {
        return (num & 1) == 1;  
    }
    private int GetDirection(Vector3 start,Vector3 end)
    {
        int vertical = (int)end.x - (int)start.x;
        int horizontal = (int)end.z - (int)start.z;
        switch(horizontal,vertical)
        {
            case (1, 1):
                return -1;
            case (0, 1):
                return 0;
            case (-1, 1):
                return 1;
            case (1, 0):
                return -2;
            case (-1, 0):
                return 2;
            case (1, -1):
                return -3;
            case (0, -1):
                return 4;
            case (-1, -1):
                return 3;
            default:
                return 0;
        }
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        waysParent = GameObject.Find("Ways").transform;
        linesParent = GameObject.Find("Lines").transform;
    }

    void Update()
    {
        //鼠标单击移动
        if (Input.GetMouseButtonDown(0))
        {
               ray = Camera.main.ScreenPointToRay(Input.mousePosition);//获取主相机到鼠标点击点的射线
            //检测射线是否碰撞到地面：地面的层级是9

            if (Physics.Raycast(ray, out hit, 1 << 9))
            {

                //往目标点移动过去

                //return;

                Vector3 starPoint = new Vector3(this.transform.position.x, 0, transform.position.z);//寻路的起点

                Vector3 targetPoint = new Vector3(hit.point.x, 0, hit.point.z);//寻路的终点

                if (!IsMove)

                    StartCoroutine(AutoMove(starPoint, targetPoint));//开启自动寻路

            }
        }
    }

    /// <summary>

    /// 自动寻路协程

    /// </summary>

    /// <returns>The move.</returns>

    /// <param name="starPoint">起点.</param>

    /// <param name="targetPoint">目标点.</param>    
    [System.Obsolete]
    IEnumerator AutoMove(Vector3 starPoint,Vector3 targetPoint)
    {

        IsMove = true;

        yield return new WaitForFixedUpdate();

        //运用A星算法计算出到起点到目标点的最佳路径
        //Vector3[] ways = GetComponent<AstarRun>().AStarFindWay(starPoint, targetPoint);
        Vector3[] ways = GetComponent<AstarDoIt>().AstarFindWays(starPoint, targetPoint);

        if(ways.Length == 0)
        {
            IsMove = false;
            yield break;
        }



        //打印显示出寻路线

/*        foreach (var v in ways)
        {
            GameObject way = Instantiate<GameObject>(wayLook);
            way.transform.parent = waysParent;

            way.transform.localPosition = v;

            way.transform.rotation = Quaternion.identity;

            way.transform.localScale = Vector3.one;
        }*/
        //画出起始线条
        GameObject StartLine = Instantiate<GameObject>(Linex);
        StartLine.name = "Path"+0;
        StartLine.transform.parent = linesParent;
        StartLine.transform.localPosition = ways[0];
        StartLine.transform.localRotation = Quaternion.Euler(90, 45 * GetDirection(ways[0], ways[1]), 0);
        StartLine.transform.localScale = Vector3.one;
        if (IsOdd(GetDirection(ways[0], ways[1])))   //斜向时长度为根号2
        {
            StartLine.GetComponent<Line>().Length = 1.4142f;
            StartLine.GetComponent<Line>().LengthChanged();
        }

        //判断下次移动方向与上次移动方向的关系
        for (int wi =1; wi< ways.Length-1; wi++)
        {
            if (GetDirection(ways[wi], ways[wi + 1]) == GetDirection(ways[wi - 1], ways[wi]))
            {
                GameObject line = Instantiate<GameObject>(Linex);
                line.name = "Path" + wi;//“LineX(Clone)wi”
                line.transform.parent = linesParent;
                line.transform.localPosition = ways[wi];
                //设置连接
                GameObject parentPath = GameObject.Find("Path" + (wi - 1));
                line.GetComponent<Line>().AttachTo(parentPath.GetComponent<SimulationPath>());//把该条line连接到上一条Path
                line.GetComponent<Line>().OnMyStartSnapped(parentPath.GetComponent<SimulationPath>());//设置该条line的起点连接
                parentPath.GetComponent<SimulationPath>().OnMyEndSnapped(line.GetComponent<Line>());//设置父Path的终点连接
                //设置位置和大小
                line.transform.localRotation = Quaternion.Euler(90, 45 * GetDirection(ways[wi], ways[wi + 1]), 0);
                line.transform.localScale = Vector3.one;
                if (IsOdd(GetDirection(ways[wi], ways[wi + 1])))   //斜向时长度为根号2
                {
                    line.GetComponent<Line>().Length = 1.4142f;
                    line.GetComponent<Line>().LengthChanged();
                }
            }
            //发生了转弯
            else
            {
                //改变父path
                GameObject parentPath = GameObject.Find("Path" + (wi - 1));
                parentPath.GetComponent<SimulationPath>().Length -= 0.40f;
                parentPath.GetComponent<Line>().LengthChanged();
                //实例化curve
                GameObject curve = Instantiate<GameObject>(Curvex);
                curve.name = "Curve" + wi;
                curve.transform.parent = linesParent;
                curve.transform.localPosition = parentPath.GetComponent<SimulationPath>().EndPos;//起点为父节点的end
                curve.transform.localRotation = parentPath.transform.localRotation;//rotation为父节点的rotation
                curve.transform.localScale = Vector3.one;
                //转角为45°，半径为0.4*tan(67.5)=9657
                if (IsOdd(GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi])))
                {
                    curve.GetComponent<Curve>().Radius = 0.9657f;
                    curve.GetComponent<Curve>().Degrees = 45;
                    //逆时针

                    if (GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi]) < 0 || GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi])== 7)
                    {
                        curve.GetComponent<Curve>().Clockwise = false;
                    }
                    if (GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi]) == -7)
                    {
                        curve.GetComponent<Curve>().Clockwise = true;
                    }
                    curve.GetComponent<Curve>().DrawPath();
                }
                //转角为90°半径为0.4
                if (!IsOdd(GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi])))
                {
                    curve.GetComponent<Curve>().Radius = 0.4f;
                    curve.GetComponent<Curve>().Degrees = 90;
                    //逆时针
                    if (GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi]) < 0 || GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi]) == 6)
                    {
                        curve.GetComponent<Curve>().Clockwise = false;
                    }
                    if (GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi]) == -6)
                    {
                        curve.GetComponent<Curve>().Clockwise = true;
                    }
                    curve.GetComponent<Curve>().DrawPath();
                }
                //设置连接
                curve.GetComponent<Curve>().AttachTo(parentPath.GetComponent<SimulationPath>());//把该条curve连接到上一条Path
                curve.GetComponent<Curve>().OnMyStartSnapped(parentPath.GetComponent<SimulationPath>());//设置该条curve的起点连接
                parentPath.GetComponent<SimulationPath>().OnMyEndSnapped(curve.GetComponent<Curve>());//设置父Path的终点连接

                //实例化line
                GameObject line = Instantiate<GameObject>(Linex);
                line.name = "Path" + wi;//
                line.transform.parent = linesParent;
                line.transform.localPosition = curve.GetComponent<Curve>().EndPos;
                //设置连接
                GameObject parentCurve = curve;
                line.GetComponent<Line>().AttachTo(parentCurve.GetComponent<SimulationPath>());//把该条line连接到上一条Path
                line.GetComponent<Line>().OnMyStartSnapped(parentCurve.GetComponent<SimulationPath>());//设置该条line的起点连接
                parentCurve.GetComponent<SimulationPath>().OnMyEndSnapped(line.GetComponent<Line>());//设置父Path的终点连接
                //设置位置和大小
                line.transform.localRotation = Quaternion.Euler(90, 45 * GetDirection(ways[wi], ways[wi + 1]), 0);
                line.transform.localScale = Vector3.one;
                line.GetComponent<Line>().Length = 0.6f;
                line.GetComponent<Line>().LengthChanged();
                if (IsOdd(GetDirection(ways[wi], ways[wi + 1])))   //斜向时长度为根号2
                {
                    line.GetComponent<Line>().Length = 1.4142f-0.4f;
                    line.GetComponent<Line>().LengthChanged();
                }
            }
        }


        //让玩家开始沿着寻路线移动
        int i = 0;

        Vector3 target = new Vector3(ways[i].x, transform.position.y, ways[i].z);

        transform.LookAt(target);

        while (true)
        {
            yield return new WaitForFixedUpdate();

            Debug.Log("run run run !!!");

            cc.SimpleMove(transform.forward * moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 1f)
            {
                Debug.Log("run is ok !!!");

                ++i;

                if (i >= ways.Length)
                    break;

                target = new Vector3(ways[i].x, transform.position.y, ways[i].z);
                transform.LookAt(target);
            }
        }

        //开始移动小车
        GameObject agv = Instantiate<GameObject>(AGV);
        agv.GetComponent<PathMover>().CreateOnPath = StartLine.GetComponent<SimulationPath>();
        agv.GetComponent<PathMover>().LeavePath = false;
        //agv.GetComponent<Source>().Destination = StartLine;
        yield return new WaitForFixedUpdate();
        agv.SetActive(true);
        Debug.Log(agv.activeInHierarchy);




        //移动完毕，删除移动路径

        /*        for (int child = waysParent.childCount - 1; child >= 0; --child)
                    Destroy(waysParent.GetChild(child).gameObject);*/

        /*        for (int child = linesParent.childCount - 1; child >= 0; --child)
                    Destroy(linesParent.GetChild(child).gameObject);
        */



        //等待执行下一次自动寻路
        IsMove = false;
    }





}

