using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Result")]
public class Result : ScriptableObject {

    public string resultName;
    [Space]
    public float requiredHandSize;
    [Space]
    public float requiredSpeed;

}
