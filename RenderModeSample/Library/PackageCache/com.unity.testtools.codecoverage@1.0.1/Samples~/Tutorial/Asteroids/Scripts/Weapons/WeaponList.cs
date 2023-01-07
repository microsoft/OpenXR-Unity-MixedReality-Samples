using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon List", order = 1)]
public class WeaponList : ScriptableObject
{
    [System.Serializable]
    public struct Weapon
    {
        public string weaponName;
        public GameObject weaponPrefab;
    };

    [SerializeField]
    public List<Weapon> weapons;
}
