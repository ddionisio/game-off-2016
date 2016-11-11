using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisCollector : MonoBehaviour {
    public M8.Auxiliary.AuxTrigger2D auxTrigger;
    public Transform debrisHolder;

    public float collectDelay = 0.15f;
    public float collectForce = 30f;
    public float collectTriggerRadiusOfs = 0.15f;

    public Rigidbody2D body { get { return mBody; } }
    public new Collider2D collider { get { return mColl; } }
    public CircleCollider2D triggerCollider { get { return mTriggerColl; } }

    /// <summary>
    /// The trigger collider's 'center' in world space
    /// </summary>
    public Vector2 triggerCollOffset { get { return mTriggerColl.transform.TransformPoint(mTriggerColl.offset); } }

    public event System.Action<DebrisCollector, Debris> debrisCollectedCallback;

    private struct DebrisData {
        public Debris debris;
        public Coroutine rout;
    }

    private Rigidbody2D mBody;
    private Collider2D mColl;
    private CircleCollider2D mTriggerColl;

    private Dictionary<Rigidbody2D, DebrisData> mDebris = new Dictionary<Rigidbody2D, DebrisData>();
    private List<Debris> mDebrisCollected = new List<Debris>();
    private Vector2 mDebrisCollectedTotalLocalPos; //local to trigger
    private Vector2 mDebrisCollectedLocalBoundMin;
    private Vector2 mDebrisCollectedLocalBoundMax;

    void OnDestroy() {
        if(auxTrigger)
            auxTrigger.enterCallback -= AuxTriggerEnter;
    }

    void Awake() {
        auxTrigger.enterCallback += AuxTriggerEnter;

        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponent<Collider2D>();
        mTriggerColl = auxTrigger.GetComponent<CircleCollider2D>();

        mDebrisCollectedTotalLocalPos = mColl.offset;
        mDebrisCollectedLocalBoundMin = mColl.offset - (Vector2)mColl.bounds.extents;
        mDebrisCollectedLocalBoundMax = mColl.offset + (Vector2)mColl.bounds.extents;
    }

    void AuxTriggerEnter(Collider2D coll) {
        var collBody = coll.attachedRigidbody;

        //already being processed
        if(mDebris.ContainsKey(collBody))
            return;

        var debris = collBody.GetComponent<Debris>();
        if(!debris)
            return;

        var dat = new DebrisData() {
            debris=debris,
            rout = StartCoroutine(DoDebrisGather(debris))
        };

        mDebris.Add(collBody, dat);
    }

    IEnumerator DoDebrisGather(Debris debris) {
        yield return null;

        var fixWait = new WaitForFixedUpdate();

        bool isCollided = false;

        System.Action<Collision2D> enterCall = delegate (Collision2D coll) {
            if(coll.rigidbody == mBody)
                isCollided = true;
        };

        System.Action<Collision2D> exitCall = delegate (Collision2D coll) {
            if(coll.rigidbody == mBody)
                isCollided = false;
        };

        debris.collisionEnterCallback += enterCall;
        debris.collisionExitCallback += exitCall;

        float t = 0f;

        while(!isCollided) {
            //keep moving towards trigger's center
            Vector2 pos = triggerCollOffset;
            Vector2 debrisPos = debris.collOffset;
            Vector2 delta = pos - debrisPos;
            Vector2 dir = delta.normalized;

            //check if we are going the opposite side, if so, reset velocity
            //if(Vector2.Dot(dir, debris.body.velocity) < 0f)
            //debris.body.velocity = Vector2.zero;

            if(debris.body.velocity.sqrMagnitude < 60*60) {
                debris.body.AddForce(dir*collectForce);
            }

            /*if(isCollided) { //linger a bit before settling in
                t += Time.fixedDeltaTime;
                if(t >= collectContactDelay)
                    break;
            }
            else { //wait for collision to happen again
                t = 0f;
            }*/

            yield return fixWait;
        }

        //secure the debris
        //debris.body.Cast()

        //remove from debris and add to collection
        debris.collisionEnterCallback -= enterCall;
        debris.collisionExitCallback -= exitCall;

        debris.transform.SetParent(debrisHolder, true);
        Destroy(debris.body);
        debris.gameObject.layer = gameObject.layer;

        mDebris.Remove(debris.body);
        mDebrisCollected.Add(debris);

        //compute average center
        Vector2 debrisLocalPos = mTriggerColl.transform.InverseTransformPoint(debris.collOffset);
        mDebrisCollectedTotalLocalPos += debrisLocalPos;

        Vector2 debrisBoundMin = debrisLocalPos - (Vector2)debris.collBounds.extents;
        Vector2 debrisBoundMax = debrisLocalPos + (Vector2)debris.collBounds.extents;

        //compute bounds
        mDebrisCollectedLocalBoundMin = new Vector2(Mathf.Min(mDebrisCollectedLocalBoundMin.x, debrisBoundMin.x), Mathf.Min(mDebrisCollectedLocalBoundMin.y, debrisBoundMin.y));
        mDebrisCollectedLocalBoundMax = new Vector2(Mathf.Max(mDebrisCollectedLocalBoundMax.x, debrisBoundMax.x), Mathf.Min(mDebrisCollectedLocalBoundMax.y, debrisBoundMax.y));

        float total = mDebrisCollected.Count + 1; //+1 for base collision (mColl)
        Vector2 newTriggerLocalPos = mDebrisCollectedTotalLocalPos/total;

        //compute trigger pos/size
        mTriggerColl.offset = newTriggerLocalPos;
        mTriggerColl.radius = Mathf.Max(Mathf.Abs(mDebrisCollectedLocalBoundMax.x - mDebrisCollectedLocalBoundMin.x), Mathf.Abs(mDebrisCollectedLocalBoundMax.y - mDebrisCollectedLocalBoundMin.y))*0.5f + collectTriggerRadiusOfs;

        if(debrisCollectedCallback != null)
            debrisCollectedCallback(this, debris);
    }
}
