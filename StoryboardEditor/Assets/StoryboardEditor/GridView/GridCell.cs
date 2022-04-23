using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour {
    [SerializeField] private Color normalSelectedColor;
    [SerializeField] private Color startSelectedColor;
    [SerializeField] private Sprite[] highlightSlices;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private Image selectionHighlight;
    [SerializeField] private TMP_Text text;

    public void SetText(string value) => text.SetText(value);
    
    public void SetSelected(bool thisSelected, bool isSelectionStart, bool left, bool right, bool above, bool below) {
        if (!thisSelected) {
            selectionHighlight.gameObject.SetActive(false);
            selectionBorder.gameObject.SetActive(false);
            
            return;
        }
        
        selectionHighlight.gameObject.SetActive(true);

        if (isSelectionStart)
            selectionHighlight.color = startSelectedColor;
        else
            selectionHighlight.color = normalSelectedColor;

        if (left && right && above && below) {
            selectionBorder.gameObject.SetActive(false);
            
            return;
        }
        
        selectionBorder.gameObject.SetActive(true);

        int index = 0;

        if (!left)
            index |= 1;

        if (!right)
            index |= 1 << 1;

        if (!above)
            index |= 1 << 2;

        if (!below)
            index |= 1 << 3;

        selectionBorder.sprite = highlightSlices[index - 1];
    }
}
