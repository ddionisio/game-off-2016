using UnityEngine;
using System.Collections;

/// <summary>
/// This is the level's flow and sequence. Inherit from this for various behaviours
/// </summary>
public class LevelController : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string takeEnter = "enter";
    public string takeExit = "exit";

    /// <summary>
    /// Initialize level data here
    /// </summary>
    public virtual void Begin() { }

    /// <summary>
    /// Cleanup
    /// </summary>
    public virtual void End() { }

    /// <summary>
    /// After begin and enter animation, this is called.
    /// </summary>
    public virtual IEnumerator Enter() { yield return null; }

    /// <summary>
    /// While this yields, level is active.  Once this ends, the level ends; or LevelManager.Load is called.
    /// </summary>
    public virtual IEnumerator Run() { while(true) { yield return null; } }

    /// <summary>
    /// After Run ends and exit animation, this is called; or LevelManager.Load is called.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator Exit() { yield return null; }
}
