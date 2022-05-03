using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarButton : MonoBehaviour {
    [SerializeField] private BindableAction[] actions;
    [SerializeField] private Button button;
    [SerializeField] private ContextMenu contextMenu;

    public BindableAction[] Actions => actions;

    private Action<BindableAction> callback;
    private InputBlocker blocker;

    public void Init(List<ContextMenu.StringPair> values, Action<BindableAction> callback, InputBlocker blocker) {
        contextMenu.SetValues(values);
        contextMenu.OptionSelected += OnContextMenuOptionSelected;
        this.callback = callback;
        this.blocker = blocker;
    }
    
    private void Awake() => button.onClick.AddListener(OnButtonClick);

    private void OnButtonClick() => contextMenu.Show(blocker);

    private void OnContextMenuOptionSelected(int index) => callback(actions[index]);
}
