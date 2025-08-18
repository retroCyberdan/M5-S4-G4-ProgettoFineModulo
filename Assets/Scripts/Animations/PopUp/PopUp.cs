using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] private string _textValue;

    // Start is called before the first frame update
    void Start()
    {
        _text.text = _textValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
