using UnityEngine;

public class PixelSnapToParent : MonoBehaviour
{
    [SerializeField] private int PixelsPerUnit = 8; 
    
    private Transform parentTransform;
    private Vector3 snappedPosition;
    private Vector3 newPosition;

    void Start()
    {
        parentTransform = transform.parent;
    }

    void LateUpdate()
    {
        Vector3 parentPos = parentTransform.position;
        snappedPosition.x = Mathf.Round(parentPos.x * PixelsPerUnit) / PixelsPerUnit;
        snappedPosition.y = Mathf.Round(parentPos.y * PixelsPerUnit) / PixelsPerUnit;
        snappedPosition.z = transform.localPosition.z; 
        transform.position = snappedPosition;
    }
}