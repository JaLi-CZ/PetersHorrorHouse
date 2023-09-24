using UnityEngine;
using UnityEngine.UI;

public class FreeEditionLimit : MonoBehaviour
{
    public bool interactable = true;
    public bool watermark = false;
    public bool grayOut = false;

    void Awake()
    {
        if (GameInfo.isProEdition) {
            if (watermark) gameObject.SetActive(false);
            return;
        }

        if (!interactable)
        {
            Slider slider = GetComponent<Slider>();
            InputField inputField = GetComponent<InputField>();
            Dropdown dropdown = GetComponent<Dropdown>();
            Toggle toggle = GetComponent<Toggle>();
            if (slider != null) slider.interactable = false;
            if (inputField != null) inputField.interactable = false;
            if (dropdown != null) dropdown.interactable = false;
            if (toggle != null) toggle.interactable = false;
        }
        if (watermark)
        {
            gameObject.SetActive(true);
        }
        if(grayOut)
        {
            Text text = GetComponent<Text>();
            Color color = text.color;
            color.a *= 0.4f;
            text.color = color;
        }
    }
}
