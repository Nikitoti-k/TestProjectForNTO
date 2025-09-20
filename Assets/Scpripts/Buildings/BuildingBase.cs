using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public Vector3 Position => transform.position;

    public virtual void Initialize() { }
}