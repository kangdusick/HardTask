using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    public StatusDictionary fairySpawnChanceDict = new();
    public StatusDictionary fairyDamageDict = new();

    private void Awake()
    {
        Instance = this;
        fairySpawnChanceDict[(ELanguageTable.DefaultValue, EStatusType.baseValue)] = 20f;
    }
}
