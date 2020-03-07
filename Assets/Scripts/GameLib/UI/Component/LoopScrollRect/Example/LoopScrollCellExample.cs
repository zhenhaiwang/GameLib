using UnityEngine;
using UnityEngine.UI;
using GameLib;

public sealed class LoopScrollCellExample : UIBaseCell
{
    [SerializeField] private LayoutElement m_LayoutElement;
    [SerializeField] private Text m_CellText;

    public override void UpdateView()
    {
        m_CellText.text = data.ToString();
    }

    protected override void OnRecycle()
    {
        // do something
    }

    public void OnClick()
    {
        m_LayoutElement.preferredHeight += 100f;

        (parentView as LoopScrollRectExample).OnClickCell();
    }
}
