using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLib;

public sealed class LoopScrollRectExample : UIBaseView
{
    [SerializeField] private InputField m_InputField;
    [SerializeField] private LoopVerticalScrollRect m_ScrollRect_Vertical;
    [SerializeField] private LoopHorizontalScrollRect m_ScrollRect_Horizontal;
    [SerializeField] private LoopHorizontalScrollRect m_ScrollRect_Grid;

    [SerializeField] private int m_PushSize = 10;
    [SerializeField] private int m_LoadSize = 5;

    private int m_Times;
    private string[] m_Signs = { "normal", "yellow" };

    protected override void OnStart()
    {
        // vertical

        var multiObjectPool = m_ScrollRect_Vertical.GetObjectPool<UIMultiObjectPool>();
        multiObjectPool.WarmPool(m_PushSize / 2, m_Signs[0]);   // 对象池预加载，可选
        multiObjectPool.WarmPool(m_PushSize / 2, m_Signs[1]);   // 对象池预加载，可选

        multiObjectPool.OnSignGetter = (index) =>
        {
            return m_Signs[index % 2];
        };

        m_ScrollRect_Vertical.RegisterParentView(this);         // 注册 Parent View，可选

        m_ScrollRect_Vertical.ListenLoadingStart(() =>          // 如果勾选了 Auto Loading ，则需要监听这个事件
        {
            StartCoroutine(LoadDataCoroutine());
        });

        // horizontal

        var simpleObjectPool_Horizontal = m_ScrollRect_Horizontal.GetObjectPool<UISimpleObjectPool>();
        simpleObjectPool_Horizontal.WarmPool(m_PushSize * 2);   // 对象池预加载，可选

        // grid

        var simpleObjectPool_Grid = m_ScrollRect_Grid.GetObjectPool<UISimpleObjectPool>();
        simpleObjectPool_Grid.WarmPool(m_PushSize * 2);         // 对象池预加载，可选
    }

    /// <summary>
    /// 模拟请求服务器数据并在1s后返回，然后更新列表数据
    /// </summary>
    private IEnumerator LoadDataCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        List<object> data = new List<object>();
        for (int i = 0; i < m_LoadSize; i++)
        {
            data.Add(m_Times++.ToString());
        }

        m_ScrollRect_Vertical.PushData(data);
    }

    public void OnClickPush()
    {
        List<object> data = new List<object>();
        for (int i = 0; i < m_PushSize; i++)
        {
            data.Add(m_Times++.ToString());
        }

        m_ScrollRect_Vertical.PushData(data);                   // 第2个参数默认为false，如果传入true，立即刷新列表
        m_ScrollRect_Horizontal.PushData(data);
        m_ScrollRect_Grid.PushData(data);
    }

    public void OnClickDelete()
    {
        m_ScrollRect_Vertical.DeleteData(int.Parse(m_InputField.text));
    }

    public void OnClickScroll()
    {
        m_ScrollRect_Vertical.ScrollToCell(int.Parse(m_InputField.text));
        //m_ScrollRect_Grid.JumpToCell(int.Parse(m_InputField.text));
    }

    public void OnClickTop()
    {
        m_ScrollRect_Vertical.verticalNormalizedPosition = 0f;
    }

    public void OnClickBottom()
    {
        m_ScrollRect_Vertical.verticalNormalizedPosition = 1f;
    }

    public void OnClickCell()
    {
        Debug.Log("click cell");
    }
}
