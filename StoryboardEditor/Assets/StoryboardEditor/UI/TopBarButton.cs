using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarButton : MonoBehaviour {
    [SerializeField] private BindableAction[] actions;
    [SerializeField] private Button button;
    [SerializeField] private ContextMenu contextMenu;

    public BindableAction[] Actions => actions;

    private IEnumerable<ContextMenu.ItemValue> getValues;
    private Action<BindableAction> callback;
    private List<ContextMenu.ItemValue> values = new();
    private InputBlocker blocker;

    public void Init(IEnumerable<ContextMenu.ItemValue> getValues, Action<BindableAction> callback, InputBlocker blocker) {
        contextMenu.OptionSelected += OnContextMenuOptionSelected;
        this.callback = callback;
        this.getValues = getValues;
        this.blocker = blocker;
    }
    
    private void Awake() => button.onClick.AddListener(OnButtonClick);

    private void OnButtonClick() {
        values.Clear();
        values.AddRange(getValues);
        contextMenu.Show(values, blocker);
    }

    private void OnContextMenuOptionSelected(int index) => callback(actions[index]);
}
