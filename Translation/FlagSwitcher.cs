using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagSwitcher : MonoBehaviour
{
    private static FlagSwitcher instance;

    public Sprite[] flags;
    public Dropdown languageDropdown;

    void Start()
    {
        instance = this;
        languageDropdown.value = Translator.currentLanguageIndex;
        SwitchFlag(Translator.currentLanguageIndex);
    }

    public static void SwitchFlag(int languageId)
    {
        instance.GetComponent<Image>().sprite = instance.flags[languageId];
    }
}
