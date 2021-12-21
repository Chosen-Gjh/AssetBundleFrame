using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragArea : MonoBehaviour
{
    public Text text0;
    public Text text1;
    public Text text2;
    public Text text3;
    public string str;
    private float preDistance = 0;
    public Transform m_zoom = null;
    private float max = 3.7f;
    private float min = 0;
    protected static float current = 0;
    private float last = -1;
    
    
    private PointerEventData currentEvent;
    private bool isLocking = false;

    private int FingerId = -1;

    private void CheckJoyStickExist()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).fingerId == this.FingerId || EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
            {
                this.FingerId = Input.GetTouch(i).fingerId;
                this.text0.text = this.FingerId.ToString();
                return;
            }
        }
        this.FingerId = -1;
        this.text0.text = this.FingerId.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        //遥感手指存在检测
        CheckJoyStickExist();
        //除开遥感还有刚好一个指头 镜头转向
        if ((this.FingerId==-1&&Input.touchCount==1)||(this.FingerId!=-1&& Input.touchCount == 2))
        {
            //找到非遥感上的那个指头id
            Touch CmrRotateTouch = default;
            foreach (var tt in Input.touches)
            {
                if (tt.fingerId != this.FingerId)
                {
                    CmrRotateTouch = tt;
                }
            }
            if(!CmrRotateTouch.Equals(default))
                Camera.main.transform.localEulerAngles += new Vector3( CmrRotateTouch.deltaPosition.y*0.5f,-CmrRotateTouch.deltaPosition.x*0.5f, 0);
        }
        //除开遥感还有刚好两个指头
        else if((this.FingerId==-1&&Input.touchCount==2)||(this.FingerId!=-1&& Input.touchCount ==3))
        {
            Touch tt1 = default;
            Touch tt2 = default;
            foreach (var tt in Input.touches)
            {
                if (tt.fingerId != this.FingerId&&tt1.Equals(default))
                {
                    tt1 = tt;
                }
                else if (tt.fingerId != this.FingerId)
                {
                    tt2 = tt;
                }
            }
            if(tt1.Equals(default)||tt2.Equals(default))
                return;
            //进行缩放
            float dis = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);//两指之间的距离
            if (-1 == last) last = dis;
            float result = dis - last;//与上一帧比较变化
            if (result + current < min)//区间限制：最小
                result = min - current;
            else if (result + current > max)//区间限制：最大
                result = max - current;
            result *= 0.1f;//系数
            m_zoom.position += m_zoom.forward.normalized * result;
            current += result;//累计当前
            last = dis;//记录为上一帧的值
        }
        else
        {
            last = -1;//不触发逻辑时
        }
    }
    
}
