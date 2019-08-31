using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LocusIcon : MonoBehaviour
{
    [SerializeField]
    Text text;

    public string Codon
    {
        get { return text.text; }
        set { text.text = value; }
    }
}
