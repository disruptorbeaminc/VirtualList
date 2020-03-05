using UnityEngine;
using UnityEngine.UI;
using VirtualList;

// IViewFor is not required, just a helper for the SimpleSource wrapper
// obviously, you can bind to more than just primitive values
public class TestDisplay : MonoBehaviour, IViewFor<string>
{
    public Text text;

    public void Set(string value)
    {
        text.text = value;
    }
}
