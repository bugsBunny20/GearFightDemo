using UnityEngine;

public class LockChildRotation : MonoBehaviour
{
    public Transform childToLock; // Assign the child object in the Inspector
    private Quaternion initialChildRotation;

    void Start()
    {
        if (childToLock != null)
        {
            // Store the initial local rotation of the child
            initialChildRotation = childToLock.localRotation;
        }
    }

    void LateUpdate()
    {
        if (childToLock != null)
        {
            // Reset the child's local rotation to its initial state
            childToLock.localRotation = initialChildRotation;
        }
    }
}