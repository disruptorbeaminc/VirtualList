using UnityEngine;

class DummySource : VirtualList.IListSource
{
    public int Count
    {
        get { return 101; }
    }

    public void SetItem(GameObject view, int index)
    {
        var item = view.GetComponent<TestDisplay>();
        if(item != null)
        {
            item.text.text = index.ToString();
        }
    }
}
