using UnityEngine;

class RandomSource : VirtualList.IListSource
{
    public int Count
    {
        get { return 100; }
    }

    public void SetItem(GameObject view, int index)
    {
        var item = view.GetComponent<TestDisplay>();
        if(item != null)
        {
            item.text.text = Random.Range(0, 1000).ToString();
        }
    }
}
