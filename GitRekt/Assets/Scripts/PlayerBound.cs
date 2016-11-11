using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PlayerBound : MonoBehaviour {

    /// <summary>
    /// Note: pos = world position
    /// </summary>
    protected abstract Vector2 UpdatePosition(Vector2 pos, float radius);

    protected virtual void OnEnable() {
        PlayerController.instance.player.AddUpdatePosition(UpdatePosition);
    }

    protected virtual void OnDisable() {
        if(PlayerController.instance)
            PlayerController.instance.player.RemoveUpdatePosition(UpdatePosition);
    }

}
