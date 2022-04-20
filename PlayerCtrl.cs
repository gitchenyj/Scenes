using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using game4automation;

public class PlayerCtrl : MonoBehaviour
{
    public GameObject wayLook;//Ѱ·�ߵĺ��

    public GameObject Linex;//

    public GameObject Curvex;

    public GameObject AGV;

    public float moveSpeed = 10f;//��ɫǰ���ٶ�

    private CharacterController cc;//��ɫ������

    private Transform waysParent;//Ѱ·�ߵķ���λ��
    private Transform linesParent;

    private Ray ray;//���߼���������

    private RaycastHit hit;

    private bool IsMove = false;//�Ƿ�����Ѱ·����
    //�˸�����
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
        //��굥���ƶ�
        if (Input.GetMouseButtonDown(0))
        {
               ray = Camera.main.ScreenPointToRay(Input.mousePosition);//��ȡ�������������������
            //��������Ƿ���ײ�����棺����Ĳ㼶��9

            if (Physics.Raycast(ray, out hit, 1 << 9))
            {

                //��Ŀ����ƶ���ȥ

                //return;

                Vector3 starPoint = new Vector3(this.transform.position.x, 0, transform.position.z);//Ѱ·�����

                Vector3 targetPoint = new Vector3(hit.point.x, 0, hit.point.z);//Ѱ·���յ�

                if (!IsMove)

                    StartCoroutine(AutoMove(starPoint, targetPoint));//�����Զ�Ѱ·

            }
        }
    }

    /// <summary>

    /// �Զ�Ѱ·Э��

    /// </summary>

    /// <returns>The move.</returns>

    /// <param name="starPoint">���.</param>

    /// <param name="targetPoint">Ŀ���.</param>    
    [System.Obsolete]
    IEnumerator AutoMove(Vector3 starPoint,Vector3 targetPoint)
    {

        IsMove = true;

        yield return new WaitForFixedUpdate();

        //����A���㷨���������㵽Ŀ�������·��
        //Vector3[] ways = GetComponent<AstarRun>().AStarFindWay(starPoint, targetPoint);
        Vector3[] ways = GetComponent<AstarDoIt>().AstarFindWays(starPoint, targetPoint);

        if(ways.Length == 0)
        {
            IsMove = false;
            yield break;
        }



        //��ӡ��ʾ��Ѱ·��

/*        foreach (var v in ways)
        {
            GameObject way = Instantiate<GameObject>(wayLook);
            way.transform.parent = waysParent;

            way.transform.localPosition = v;

            way.transform.rotation = Quaternion.identity;

            way.transform.localScale = Vector3.one;
        }*/
        //������ʼ����
        GameObject StartLine = Instantiate<GameObject>(Linex);
        StartLine.name = "Path"+0;
        StartLine.transform.parent = linesParent;
        StartLine.transform.localPosition = ways[0];
        StartLine.transform.localRotation = Quaternion.Euler(90, 45 * GetDirection(ways[0], ways[1]), 0);
        StartLine.transform.localScale = Vector3.one;
        if (IsOdd(GetDirection(ways[0], ways[1])))   //б��ʱ����Ϊ����2
        {
            StartLine.GetComponent<Line>().Length = 1.4142f;
            StartLine.GetComponent<Line>().LengthChanged();
        }

        //�ж��´��ƶ��������ϴ��ƶ�����Ĺ�ϵ
        for (int wi =1; wi< ways.Length-1; wi++)
        {
            if (GetDirection(ways[wi], ways[wi + 1]) == GetDirection(ways[wi - 1], ways[wi]))
            {
                GameObject line = Instantiate<GameObject>(Linex);
                line.name = "Path" + wi;//��LineX(Clone)wi��
                line.transform.parent = linesParent;
                line.transform.localPosition = ways[wi];
                //��������
                GameObject parentPath = GameObject.Find("Path" + (wi - 1));
                line.GetComponent<Line>().AttachTo(parentPath.GetComponent<SimulationPath>());//�Ѹ���line���ӵ���һ��Path
                line.GetComponent<Line>().OnMyStartSnapped(parentPath.GetComponent<SimulationPath>());//���ø���line���������
                parentPath.GetComponent<SimulationPath>().OnMyEndSnapped(line.GetComponent<Line>());//���ø�Path���յ�����
                //����λ�úʹ�С
                line.transform.localRotation = Quaternion.Euler(90, 45 * GetDirection(ways[wi], ways[wi + 1]), 0);
                line.transform.localScale = Vector3.one;
                if (IsOdd(GetDirection(ways[wi], ways[wi + 1])))   //б��ʱ����Ϊ����2
                {
                    line.GetComponent<Line>().Length = 1.4142f;
                    line.GetComponent<Line>().LengthChanged();
                }
            }
            //������ת��
            else
            {
                //�ı丸path
                GameObject parentPath = GameObject.Find("Path" + (wi - 1));
                parentPath.GetComponent<SimulationPath>().Length -= 0.40f;
                parentPath.GetComponent<Line>().LengthChanged();
                //ʵ����curve
                GameObject curve = Instantiate<GameObject>(Curvex);
                curve.name = "Curve" + wi;
                curve.transform.parent = linesParent;
                curve.transform.localPosition = parentPath.GetComponent<SimulationPath>().EndPos;//���Ϊ���ڵ��end
                curve.transform.localRotation = parentPath.transform.localRotation;//rotationΪ���ڵ��rotation
                curve.transform.localScale = Vector3.one;
                //ת��Ϊ45�㣬�뾶Ϊ0.4*tan(67.5)=9657
                if (IsOdd(GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi])))
                {
                    curve.GetComponent<Curve>().Radius = 0.9657f;
                    curve.GetComponent<Curve>().Degrees = 45;
                    //��ʱ��

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
                //ת��Ϊ90��뾶Ϊ0.4
                if (!IsOdd(GetDirection(ways[wi], ways[wi + 1]) - GetDirection(ways[wi - 1], ways[wi])))
                {
                    curve.GetComponent<Curve>().Radius = 0.4f;
                    curve.GetComponent<Curve>().Degrees = 90;
                    //��ʱ��
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
                //��������
                curve.GetComponent<Curve>().AttachTo(parentPath.GetComponent<SimulationPath>());//�Ѹ���curve���ӵ���һ��Path
                curve.GetComponent<Curve>().OnMyStartSnapped(parentPath.GetComponent<SimulationPath>());//���ø���curve���������
                parentPath.GetComponent<SimulationPath>().OnMyEndSnapped(curve.GetComponent<Curve>());//���ø�Path���յ�����

                //ʵ����line
                GameObject line = Instantiate<GameObject>(Linex);
                line.name = "Path" + wi;//
                line.transform.parent = linesParent;
                line.transform.localPosition = curve.GetComponent<Curve>().EndPos;
                //��������
                GameObject parentCurve = curve;
                line.GetComponent<Line>().AttachTo(parentCurve.GetComponent<SimulationPath>());//�Ѹ���line���ӵ���һ��Path
                line.GetComponent<Line>().OnMyStartSnapped(parentCurve.GetComponent<SimulationPath>());//���ø���line���������
                parentCurve.GetComponent<SimulationPath>().OnMyEndSnapped(line.GetComponent<Line>());//���ø�Path���յ�����
                //����λ�úʹ�С
                line.transform.localRotation = Quaternion.Euler(90, 45 * GetDirection(ways[wi], ways[wi + 1]), 0);
                line.transform.localScale = Vector3.one;
                line.GetComponent<Line>().Length = 0.6f;
                line.GetComponent<Line>().LengthChanged();
                if (IsOdd(GetDirection(ways[wi], ways[wi + 1])))   //б��ʱ����Ϊ����2
                {
                    line.GetComponent<Line>().Length = 1.4142f-0.4f;
                    line.GetComponent<Line>().LengthChanged();
                }
            }
        }


        //����ҿ�ʼ����Ѱ·���ƶ�
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

        //��ʼ�ƶ�С��
        GameObject agv = Instantiate<GameObject>(AGV);
        agv.GetComponent<PathMover>().CreateOnPath = StartLine.GetComponent<SimulationPath>();
        agv.GetComponent<PathMover>().LeavePath = false;
        //agv.GetComponent<Source>().Destination = StartLine;
        yield return new WaitForFixedUpdate();
        agv.SetActive(true);
        Debug.Log(agv.activeInHierarchy);




        //�ƶ���ϣ�ɾ���ƶ�·��

        /*        for (int child = waysParent.childCount - 1; child >= 0; --child)
                    Destroy(waysParent.GetChild(child).gameObject);*/

        /*        for (int child = linesParent.childCount - 1; child >= 0; --child)
                    Destroy(linesParent.GetChild(child).gameObject);
        */



        //�ȴ�ִ����һ���Զ�Ѱ·
        IsMove = false;
    }





}

