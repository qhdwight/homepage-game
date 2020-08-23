using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class FroggerBehavior : MonoBehaviour
{
    private Animator m_Animator;

    private void Start() => m_Animator = GetComponent<Animator>();

    [UsedImplicitly]
    public void OnMove(InputValue input)
    {
        Vector3 move = input.Get<Vector2>();
        if (move.sqrMagnitude < Mathf.Epsilon) return;

        Transform t = transform;
        t.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(-move.x, move.y), Vector3.forward);
        t.Translate(move, Space.World);
        m_Animator.Play("Hop");
    }
}