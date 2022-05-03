using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextInputPopup : Popup {
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_InputField inputField;

    public void Show(string message, string initialValue, Action<string> callback, InputBlocker blocker) {
        base.Show(blocker);
        text.SetText(message);
        inputField.SetTextWithoutNotify(initialValue);
        inputField.onSubmit.RemoveAllListeners();
        inputField.onSubmit.AddListener(new UnityAction<string>(callback));
        inputField.onSubmit.AddListener(_ => Hide());
        inputField.Select();
    }
}
