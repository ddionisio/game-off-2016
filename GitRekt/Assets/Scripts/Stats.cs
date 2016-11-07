using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {
    public delegate void ChangeCallback(Stats stat, int delta);

    [SerializeField]
    int _maxHP;
    [SerializeField]
    int _score;

    public event ChangeCallback changeHPCallback;
    public event ChangeCallback changeScoreCallback;

    private int mCurHP;

    public int curHP {
        get { return mCurHP; }
        set {
            int v = Mathf.Clamp(value, 0, _maxHP);
            if(mCurHP != v) {
                int prev = mCurHP;
                mCurHP = v;

                if(changeHPCallback != null)
                    changeHPCallback(this, mCurHP - prev);
            }
        }
    }

    public int score {
        get { return _score; }
        set {
            if(_score != value) {
                int prev = _score;
                _score = value;

                if(changeScoreCallback != null)
                    changeScoreCallback(this, _score - prev);
            }
        }
    }

    public virtual void Reset() {
        curHP = _maxHP;
    }

    protected virtual void OnDestroy() {
        changeHPCallback = null;
        changeScoreCallback = null;
    }

    protected virtual void Awake() {
        mCurHP = _maxHP;
    }
}
