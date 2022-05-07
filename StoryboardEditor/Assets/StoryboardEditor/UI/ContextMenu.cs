using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : Popup {
    public readonly struct ItemValue {
        public string Left { get; }
        public string Right { get; }
        public bool Clickable { get; }

        public ItemValue(string left, string right, bool clickable) {
            this.Left = left;
            this.Right = right;
            this.Clickable = clickable;
        }
    }
    
    [SerializeField] private GameObject template;
    [SerializeField] private RectTransform layout;

    public event Action<int> OptionSelected;
    
    private List<Button> buttons = new();
    private List<ItemValue> values;

    public void Show(List<ItemValue> values, InputBlocker blocker) {
        this.values = values;
        
        while (buttons.Count > values.Count) {
            Destroy(buttons[^1].gameObject);
            buttons.RemoveAt(buttons.Count - 1);
        }

        while (buttons.Count < values.Count) {
            var go = Instantiate(template, layout);

            go.transform.SetSiblingIndex(buttons.Count);
            go.SetActive(true);

            var button = go.GetComponent<Button>();
            int index = buttons.Count;
            
            button.onClick.AddListener(() => OnButtonClicked(index));
            buttons.Add(button);
        }

        for (int i = 0; i < values.Count; i++) {
            var value = values[i];
            var texts = buttons[i].GetComponentsInChildren<TMP_Text>();

            texts[0].SetText(value.Left);
            texts[1].SetText(value.Right);

            if (value.Clickable) {
                texts[0].color = Color.black;
                texts[1].color = Color.black;
            }
            else {
                texts[0].color = Color.gray;
                texts[1].color = Color.gray;
            }
        }
        
        base.Show(blocker);
    }
    
    private void OnButtonClicked(int index) {
        if (!values[index].Clickable)
            return;
        
        OptionSelected?.Invoke(index);
        Hide();
    }
}
