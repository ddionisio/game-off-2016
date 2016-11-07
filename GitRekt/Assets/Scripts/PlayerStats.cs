using UnityEngine;
using System.Collections;

[System.Serializable]
public struct BallDragData {
    public float min;
    public float max;
    public float accel;

    public float playerSpeedThreshold;
}

public class PlayerStats : Stats {
    public float lineMaxLength = 1.5f;
    public float lineDanger = 0.2f;
        
    [SerializeField]
    int _maxLives;

    public BallDragData ballDrag;

    public event ChangeCallback changeLivesCallback;

    private int mCurLives;
    private int mLastScore;    
    
    public int curLives {
        get { return mCurLives; }
        set {
            int v = Mathf.Clamp(value, 0, _maxLives);
            if(mCurLives != v) {
                int prev = mCurLives;
                mCurLives = v;

                if(changeLivesCallback != null)
                    changeLivesCallback(this, mCurLives - prev);
            }
        }
    }

    public void LastScoreSetFromCurrent() {
        mLastScore = score;
    }

    public void LastScoreReset() {
        mLastScore = 0;
    }

    public override void Reset() {
        base.Reset();

        score = mLastScore;
    }
}
