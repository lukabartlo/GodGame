using UnityEngine;

public abstract class Abilities : MonoBehaviour
{

    public bool isUsing;
    
    public abstract void Capacity();

    public abstract void StartUsing();

    public abstract void StopUsing();
}