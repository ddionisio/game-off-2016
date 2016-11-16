using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;
using DG.Tweening.Core.Easing;

public class DebrisCollector : MonoBehaviour {
    public M8.Auxiliary.AuxTrigger2D auxTrigger;
    public Transform debrisHolder;
    public Transform debrisBodyHolder; //this is where bodies are tossed out

    public float collectDelay = 0.15f;
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

    private RaycastHit2D[] mDebrisHitBuffer = new RaycastHit2D[8];

    void OnDestroy() {
        if(auxTrigger)
            auxTrigger.enterCallback -= AuxTriggerEnter;
    }

    void Awake() {
        auxTrigger.enterCallback += AuxTriggerEnter;

        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponent<Collider2D>();
        mTriggerColl = auxTrigger.GetComponent<CircleCollider2D>();
    }

    void AuxTriggerEnter(Collider2D coll) {
        var collBody = coll.attachedRigidbody;

        //already being processed
        if(collBody == null || mDebris.ContainsKey(collBody))
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
        yield return null; //wait for this to be in the debris hits

        //secure the debris
        Vector2 pos = triggerCollOffset;
        Vector2 debrisPos = debris.collOffset;
        Vector2 delta = pos - debrisPos;
        float dist = delta.magnitude;
        Vector2 dir = delta/dist;
                
        int hitCount = debris.body.Cast(dir, mDebrisHitBuffer, dist);
        if(hitCount <= 0) { //nothing hit??
            Debug.LogWarning("Nothing hit for: "+debris.name);            
            mDebris.Remove(debris.body);
            yield break;
        }

        //grab the hit from the ball
        int hitInd = -1;
        for(int i = 0; i < hitCount; i++) {
            if(mDebrisHitBuffer[i].collider == mTriggerColl)
                continue;

            if(mDebrisHitBuffer[i].collider.attachedRigidbody == mBody) {
                hitInd = i;
            }
        }

        if(hitInd == -1) { //must have hit something else
            Debug.LogWarning("Nothing hit for: "+debris.name);
            mDebris.Remove(debris.body);
            yield break;
        }
                
        //just take the first hit
        var toPos = debrisPos + dir*(dist*mDebrisHitBuffer[hitInd].fraction);
        
        //move - detach collider from body (or destroy body if it's from another game object)
        if(debris.body.gameObject != debris.coll.gameObject) {
            debris.body.transform.SetParent(debrisBodyHolder, true);
        }
        else {
            Destroy(debris.body);
        }
                
        var debrisCollTrans = debris.coll.transform;

        debrisCollTrans.SetParent(debrisHolder, true);
                
        var debrisPosLocal = debrisCollTrans.InverseTransformPoint(debrisPos);
        var toPosLocal = debrisCollTrans.InverseTransformPoint(toPos);

        if(debrisPosLocal != toPosLocal) {
            debris.coll.enabled = false;
                        
            float t = 0f;
            while(true) {
                t += Time.deltaTime;
                if(t > collectDelay)
                    break;

                float _t = EaseManager.Evaluate(Ease.InCirc, null, t, collectDelay, 0f, 0f);

                debrisCollTrans.localPosition = Vector2.Lerp(debrisPosLocal, toPosLocal, _t);

                yield return null;
            }
        }

        //attach to our collection
        debrisCollTrans.gameObject.layer = gameObject.layer;
        debrisCollTrans.localPosition = toPosLocal;

        debris.coll.enabled = true;

        mDebris.Remove(debris.body);
        mDebrisCollected.Add(debris);

        //compute new trigger radius
        var triggerDelta = toPos - triggerCollOffset;
        var triggerDeltaDistSqr = triggerDelta.sqrMagnitude;
        if(triggerDeltaDistSqr + collectTriggerRadiusOfs*collectTriggerRadiusOfs > mTriggerColl.radius*mTriggerColl.radius) {
            mTriggerColl.radius = Mathf.Sqrt(triggerDeltaDistSqr) + collectTriggerRadiusOfs;
        }
        
        if(debrisCollectedCallback != null)
            debrisCollectedCallback(this, debris);
    }
}
