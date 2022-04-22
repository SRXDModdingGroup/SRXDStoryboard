using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour {
    [SerializeField] private Sprite[] highlightSlices;
    [SerializeField] private Image selectionHighlight;

    public void SetSelected(bool thisSelected, bool left, bool right, bool above, bool below) {
        if (!thisSelected || left && right && above && below) {
            selectionHighlight.gameObject.SetActive(false);
            
            return;
        }
        
        selectionHighlight.gameObject.SetActive(true);

        int index = 0;

        if (left)
            index |= 1;

        if (right)
            index |= 1 << 1;

        if (above)
            index |= 1 << 2;

        if (below)
            index |= 1 << 3;

        selectionHighlight.sprite = highlightSlices[index - 1];
    }
}
