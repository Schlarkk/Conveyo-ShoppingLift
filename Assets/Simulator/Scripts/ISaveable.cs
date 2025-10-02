using UnityEngine;

public interface ISaveable
{
    /// Return a simple serializable struct/class of your current state (or null if nothing).
    object CaptureState();

    /// Accept the object you previously returned in CaptureState and apply it.
    void RestoreState(object state);
}
