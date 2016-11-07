using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public PlayerEntity player { get { return mPlayer; } }

    private PlayerEntity mPlayer;

    private bool mIsFocus = true;

    void OnDestroy() {
        if(M8.InputManager.instance)
            M8.InputManager.instance.RemoveButtonCall(0, InputAction.Menu, OnInputMenu);

        if(M8.UIModal.Manager.instance)
            M8.UIModal.Manager.instance.activeCallback += OnUIModalActive;
    }

    void Awake() {
        mPlayer = GetComponentInChildren<PlayerEntity>();

        M8.UIModal.Manager.instance.activeCallback += OnUIModalActive;

        M8.InputManager.instance.AddButtonCall(0, InputAction.Menu, OnInputMenu);
    }

	IEnumerator Start () {
        yield return null;

        mPlayer.Activate();
	}

    void OnApplicationFocus(bool focus) {
        mIsFocus = focus;

        if(M8.UIModal.Manager.instance != null && M8.UIModal.Manager.instance.activeCount > 0) {
            Cursor.visible = true;
        }
        else {
            if(M8.SceneManager.instance != null) {
                if(mIsFocus) {
                    M8.SceneManager.instance.Resume();
                }
                else {
                    M8.SceneManager.instance.Pause();

                    //open pause menu
                }
            }

            Cursor.visible = !mIsFocus;
        }
    }

    void OnUIModalActive(bool active) {
        if(active) {
            Cursor.visible = true;

            mPlayer.inputEnabled = false;
        }
        else {
            if(mIsFocus)
                M8.SceneManager.instance.Resume();

            Cursor.visible = !mIsFocus;

            if(mPlayer.state == (int)EntityState.Normal)
                mPlayer.inputEnabled = true;
        }
    }
    
    void OnInputMenu(M8.InputManager.Info dat) {
        if(dat.state == M8.InputManager.State.Pressed) {
            if(M8.UIModal.Manager.instance != null && M8.UIModal.Manager.instance.activeCount == 0) {
                M8.SceneManager.instance.Pause();

                //open pause menu
            }
            //temp
            else if(M8.SceneManager.instance.isPaused && mIsFocus) {
                M8.SceneManager.instance.Resume();
                Cursor.visible = !mIsFocus;
            }
        }
    }
}
