using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Panel_FindGroup : BasePanel
{
    protected override void Awake()
    {
        base.Awake();
        TryGetUi<TMP_InputField>("InputField",out TMP_InputField inputField);
        Debug.Log(inputField);
    }
}
