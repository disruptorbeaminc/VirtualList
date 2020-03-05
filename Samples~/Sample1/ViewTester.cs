using UnityEngine;
using UnityEngine.UI;
using VirtualList;

public class ViewTester : MonoBehaviour
{
   public AbstractVirtualList list;
   public Button source1Button;
   public Button source2Button;
   public Button source3Button;
   public Button clearButton;
   public Button invalidateButton;

   private string[] _someNames =
   {
      "Hello",
      "There",
      "I",
      "am",
      "a",
      "Tree"
   };

   public void Start() {
      source1Button.onClick.AddListener(() => list.SetSource(new DummySource()));
      source2Button.onClick.AddListener(() => list.SetSource(new RandomSource()));
      source3Button.onClick.AddListener(() => list.SetSource(new SimpleSource<string, TestDisplay>(_someNames)));
      clearButton.onClick.AddListener(list.Clear);
      invalidateButton.onClick.AddListener(list.Invalidate);
   }
}