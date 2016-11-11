using UnityEngine;
using System.Collections;

public class Debris : MonoBehaviour {
    public enum Mode {
        Attached, //currently attached to something else
        Free, //detached, can be gathered
        Collect, //in the process of being gathered by collector
        Collected //collected by the collector
    }

    public bool startFree; //start as free, otherwise it is considered attached

    public Mode mode {
        get { return mMode; }
        set { mMode = value; }
    }

    public Rigidbody2D body { get { return mBody; } }
    public Collider2D coll { get { return mColl; } }

    /// <summary>
    /// Collision 'center' in world space.
    /// </summary>
    public Vector2 collOffset { get { return transform.TransformPoint(mColl.offset); } }
    public Bounds collBounds { get { return mColl.bounds; } }

    public event System.Action<Collision2D> collisionEnterCallback;
    public event System.Action<Collision2D> collisionExitCallback;

    private Mode mMode;
    private int mLayerDefault;

    private Rigidbody2D mBody;
    private Collider2D mColl;
    
    public void Reset() {
        mMode = startFree ? Mode.Free : Mode.Attached;
        gameObject.layer = mLayerDefault;
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if(collisionEnterCallback != null)
            collisionEnterCallback(coll);
    }
    
    void OnCollisionExit2D(Collision2D coll) {
        if(collisionExitCallback != null)
            collisionExitCallback(coll);
    }

    void Awake() {
        mMode = startFree ? Mode.Free : Mode.Attached;
        mLayerDefault = gameObject.layer;

        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponentInChildren<Collider2D>();
    }
}
