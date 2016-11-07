using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEntity : M8.EntityBase {
    public const int maxAvgSpeedCount = 50;

    public LayerMask harmLayerMask;

    public M8.Animator.AnimatorData animator;

    public PlayerStats stats { get { return mStats; } }

    public Vector2 velocity { get { return mCurVel; } }
    public float speed { get { return mCurSpeed; } }
    public float speedAverage { get { return mAvgSpeed; } }
    public Vector2 dir { get { return mCurDir; } }
                    
    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                if(mInputEnabled) {
                    mLastInputTime = Time.realtimeSinceStartup;
                    mInputDampVel = Vector2.zero;
                }
                else {
                }
            }
        }
    }

    public Rigidbody2D body { get { return mBody; } }
    public CircleCollider2D circleCollider { get { return mCircleColl; } }
    public SpringJoint2D joint { get { return mJoint; } }
    public GameObject ball { get { return mBall; } }
    public Rigidbody2D ballBody { get { return mBallBody; } }

    private PlayerStats mStats;
        
    private bool mInputEnabled = false;

    private Vector2 mCurDir = Vector2.zero;
    private float mCurSpeed = 0.0f;
    private Vector2 mCurVel = Vector2.zero;
    private Vector2 mInputAxis = Vector2.zero;
    private Vector2 mInputDampVel;

    private float mLastInputTime;

    private Rigidbody2D mBody;
    private CircleCollider2D mCircleColl;
    private SpringJoint2D mJoint;
    private GameObject mBall;
    private Rigidbody2D mBallBody;

    private float mBallCurDrag;
    private float mBallCurDragSpd;

    private Queue<float> mAvgSpeedCache = new Queue<float>(maxAvgSpeedCount);
    private float mAvgSpeed;

    protected override void StateChanged() {
        switch((EntityState)prevState) {
            case EntityState.Normal:
                break;
        }

        switch((EntityState)state) {
            case EntityState.Spawn:
            case EntityState.Invalid:
                inputEnabled = false;
                break;

            case EntityState.Normal:
                inputEnabled = true;

                mAvgSpeedCache.Clear();
                mAvgSpeed = 0f;
                break;

            case EntityState.Dead:
                inputEnabled = false;
                
                //shake hud
                //mHUD.SetLinePercent(0.0f);

                //save stuff
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        state = StateInvalid;
    }

    protected override void OnSpawned() {
        //populate data/state for ai, player control, etc.
        state = (int)EntityState.Spawn;
    }

    protected override void SpawnStart() {
        //start ai, player control, etc
        state = (int)EntityState.Normal;
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }
        
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mStats = GetComponent<PlayerStats>();
        mBody = GetComponentInChildren<Rigidbody2D>();
        mCircleColl = GetComponentInChildren<CircleCollider2D>();
        mJoint = GetComponentInChildren<SpringJoint2D>();

        mBallBody = mJoint.connectedBody;
        if(mBallBody)
            mBall = mJoint.connectedBody.gameObject;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void OnTriggerEnter2D(Collider2D coll) {
        //death
        if(((1 << coll.gameObject.layer) & harmLayerMask) != 0) {
            //state = (int)EntityState.Dead;
        }
    }

    void Update() {
        switch((EntityState)state) {
            case EntityState.Normal:
                

                if(mInputEnabled) {
                    Vector2 pos = transform.position;

                    float dt = Time.realtimeSinceStartup - mLastInputTime;
                    mLastInputTime = Time.realtimeSinceStartup;

                    var input = M8.InputManager.instance;

                    mInputAxis.x = input.GetAxis(0, InputAction.Horizontal);
                    mInputAxis.y = input.GetAxis(0, InputAction.Vertical);

                    if(mInputAxis.x != 0f || mInputAxis.y != 0f) {
                        var toPos = Vector2.SmoothDamp(pos, pos + mInputAxis, ref mInputDampVel, dt);

                        var finalPos = UpdatePosition(toPos);

                        mCurVel = (finalPos - pos)/dt;
                        mCurSpeed = mCurVel.magnitude;
                        if(mCurSpeed > 0f)
                            mCurDir = mCurVel / mCurSpeed;

                        //Debug.Log("speed: "+mCurSpeed);
                    }
                    else {
                        mCurVel = Vector2.zero;
                        mCurSpeed = 0.0f;
                    }

                    //mAvgSpeed = (mAvgSpeed + mCurSpeed*avgSpeedWeight)/(avgSpeedWeight + 1.0f);

                    //compute average
                    if(mAvgSpeedCache.Count >= maxAvgSpeedCount)
                        mAvgSpeedCache.Dequeue();
                    mAvgSpeedCache.Enqueue(mCurSpeed);

                    mAvgSpeed = 0f;
                    foreach(var spd in mAvgSpeedCache)
                        mAvgSpeed += spd;

                    mAvgSpeed /= mAvgSpeedCache.Count;

                    //Debug.Log("ball avg speed: "+mAvgSpeed);
                }
                else {
                    mInputAxis = Vector2.zero;
                    mCurVel = Vector2.zero;
                    mCurSpeed = 0.0f;
                }
                break;
        }

        UpdateBall();
    }

    private void UpdateBall() {
        if(!mBallBody)
            return;

        if(mAvgSpeed > stats.ballDrag.playerSpeedThreshold) {
            mBallCurDrag = stats.ballDrag.min;
            mBallCurDragSpd = 0f;
        }
        else {
            if(mBallCurDrag < stats.ballDrag.max) {
                mBallCurDragSpd += stats.ballDrag.accel * Time.deltaTime;
                mBallCurDrag += mBallCurDragSpd * Time.deltaTime;
                if(mBallCurDrag > stats.ballDrag.max)
                    mBallCurDrag = stats.ballDrag.max;
            }
        }

        mBallBody.drag = mBallCurDrag;
    }

    private Vector2 UpdatePosition(Vector2 pos) {
        //check bound
        M8.Camera2D cam2D = M8.Camera2D.main;
        Rect screen = cam2D.screenExtent;
        Vector3 cam2DPos = cam2D.transform.position;

        float hW = screen.width*0.5f;
        float hH = screen.height*0.5f;

        if(pos.x < cam2DPos.x - hW)
            pos.x = cam2DPos.x - hW;
        else if(pos.x > cam2DPos.y + hW)
            pos.x = cam2DPos.y + hW;

        if(pos.y < cam2DPos.y - hH)
            pos.y = cam2DPos.y - hH;
        else if(pos.y > cam2DPos.y + hH)
            pos.y = cam2DPos.y + hH;

        mBody.MovePosition(new Vector2(pos.x, pos.y));

        return pos;
    }
}
