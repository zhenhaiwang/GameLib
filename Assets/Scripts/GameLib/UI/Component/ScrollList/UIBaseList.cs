using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    public class UIBaseList : UIBaseView
    {
        public UISimpleObjectPool objectPool;
        public Transform contentPanel;
        public int fixedChildCount;

        protected object[] m_Datas;
        protected List<UIBaseCell> m_Cells = new List<UIBaseCell>();

        public object[] datas
        {
            get
            {
                return m_Datas;
            }
            set
            {
                m_Datas = value;

                OnDataChanged();
                InvalidView();
            }
        }

        public void Clear()
        {
            datas = null;
        }

        public void Refresh()
        {
            int length = m_Datas != null ? m_Datas.Length : 0;
            for (int i = 0; i < length; i++)
            {
                m_Cells[i].InvalidView();
            }
        }

        public void RefreshCell(int index, object data)
        {
            if (index >= 0 && index < m_Cells.Count)
            {
                m_Cells[index].Provide(index, data);
            }
        }

        public void RefreshCell(int index)
        {
            if (index >= 0 && index < m_Cells.Count)
            {
                m_Cells[index].InvalidView();
            }
        }

        public void PushCell(object data)
        {
            List<object> list = new List<object>();

            if (m_Datas.Length() > 0)
            {
                list.AddRange(m_Datas);
            }

            list.Add(data);

            m_Datas = list.ToArray();

            OnDataChanged();

            m_Cells.Add(CreateCell(contentPanel.childCount - fixedChildCount));
        }

        public void PushData(object[] dataArray)
        {
            List<object> list = new List<object>();

            if (m_Datas.Length() > 0)
            {
                list.AddRange(m_Datas);
            }

            list.AddRange(dataArray);

            m_Datas = list.ToArray();

            OnDataChanged();

            int startIndex = contentPanel.childCount - fixedChildCount;
            int cellCount = dataArray.Length();

            for (int i = 0; i < cellCount; i++)
            {
                m_Cells.Add(CreateCell(startIndex++));
            }
        }

        public override void UpdateView()
        {
            base.UpdateView();

            RemoveCells();
            AddCells();
        }

        protected virtual void OnDataChanged() { }

        private UIBaseCell CreateCell(int index)
        {
            GameObject cellGameObject = objectPool.GetObject(index);
            cellGameObject.transform.SetParent(contentPanel, false);

            var cellScript = cellGameObject.GetComponent<UIBaseCell>();
            if (cellScript != null)
            {
                cellScript.Provide(index, m_Datas[index]);
            }

            return cellScript;
        }

        private void DeleteCell(int index)
        {
            GameObject cellGameObject = contentPanel.transform.GetChild(index).gameObject;

            var cellScript = cellGameObject.GetComponent<UIBaseCell>();
            if (cellScript != null)
            {
                cellScript.Recycle();
            }

            objectPool.ReturnObject(cellGameObject);
        }

        private void AddCells()
        {
            int length = m_Datas != null ? m_Datas.Length : 0;
            for (int i = 0; i < length; i++)
            {
                m_Cells.Add(CreateCell(i));
            }
        }

        private void RemoveCells()
        {
            while (contentPanel.childCount > fixedChildCount)
            {
                DeleteCell(fixedChildCount);
            }

            m_Cells.Clear();
        }
    }
}