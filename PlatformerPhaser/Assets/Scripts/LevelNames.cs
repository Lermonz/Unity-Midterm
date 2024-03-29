using TMPro;
using UnityEngine;

public class LevelNames : MonoBehaviour
{
    [TextAreaAttribute] [SerializeField] string[] _levelNames;
    public TMP_Text _name;
    void Update()
    {
        _name.text = _levelNames[GameBehaviour._onLevel-1];
    }
}
