using UnityEngine;
using System.Collections;

public class LevelManager : M8.SingletonBehaviour<LevelManager> {

    public bool isLoading { get { return mLoadRout != null; } }

    public Level level { get { return mCurLevel; } }

    private Level mCurLevel;

    private string mLoadLevelName;
    private Coroutine mLoadRout;
        
    public void Load(string levelName) {
        if(mLoadRout != null) {
            Debug.LogWarning("Still loading: "+mLoadLevelName);
            return;
        }

        mLoadLevelName = levelName;

        mLoadRout = StartCoroutine(DoLoad());
    }

    protected override void OnInstanceInit() {
        
    }

    protected override void OnInstanceDeinit() {

    }

    IEnumerator Start() {
        yield return null;

        //for testing purpose, check if there's already a level on scene

    }

    IEnumerator DoLoad() {
        yield return null;

        mLoadRout = null;
    }
}
