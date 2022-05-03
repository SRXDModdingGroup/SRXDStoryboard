using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour {
    [Serializable]
    public struct StringPair {
        public string left;
        public string right;

        public StringPair(string left, string right) {
            this.left = left;
            this.right = right;
        }
    }
    
    [SerializeField] private List<StringPair> values;
    [SerializeField] private GameObject template;
    [SerializeField] private RectTransform layout;
    
    private List<Button> buttons = new();
    private Action<int> callback;
    private Button blocker;

    private void Awake() => SetValues(values);

    public void Init(Button blocker) => this.blocker = blocker;

    public void SetValues(List<StringPair> values) {
        this.values = values;
        
        while (buttons.Count > 0) {
            Destroy(buttons[buttons.Count - 1].gameObject);
            buttons.RemoveAt(buttons.Count - 1);
        }

        for (int i = 0; i < values.Count; i++) {
            var go = Instantiate(template, layout);
            var button = go.GetComponent<Button>();
            int j = i;
            var pair = values[i];
            var texts = go.GetComponentsInChildren<TMP_Text>();

            go.SetActive(true);
            texts[0].SetText(pair.left);
            texts[1].SetText(pair.right);
            go.transform.SetSiblingIndex(i);
            buttons.Add(button);
            button.onClick.AddListener(() => OnButtonClicked(j));
        }
    }

    public void Show(Action<int> callback) {
        this.callback = callback;
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(gameObject);
        blocker.gameObject.SetActive(true);
        blocker.onClick.AddListener(Hide);
    }

    public void Hide() {
        callback = null;
        gameObject.SetActive(false);

        if (EventSystem.current.gameObject == gameObject)
            EventSystem.current.SetSelectedGameObject(null);
        
        blocker.gameObject.SetActive(false);
        blocker.onClick.RemoveListener(Hide);
    }
    
    private void OnButtonClicked(int index) {
        callback(index);
        Hide();
    }
}
