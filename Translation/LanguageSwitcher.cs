using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSwitcher : MonoBehaviour
{
    public Dropdown languageDropdown;

    public void OnLanguageChanged()
    {
        Translator.ChangeLanguage(languageDropdown.value);
    }
}
