using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DOOR_TYPE { SWING, SLIDING }

public class Door : MonoBehaviour
{
    [Header("Impostazioni Porta")]
    [SerializeField] private DOOR_TYPE _doorType = DOOR_TYPE.SWING; // scegli dal Inspector
    [SerializeField] private float _animDuration = 0.5f;

    [Header("Normal Door Setting")]
    [SerializeField] private float _openAngle = 90f; // rotazione Y

    [Header("Sliding Door Settings")]
    [SerializeField] private Vector3 _slidingOffset = new Vector3(2f, 0, 0); // direzione/apertura

    private bool _isOpen = false;
    private Coroutine _animCoroutine;
    //public Animator animator;

    #region Door States
    private Quaternion _closedRot;
    private Quaternion _openRot;

    private Vector3 _closedPos;
    private Vector3 _openPos;
    #endregion

    void Start()
    {
        _closedRot = transform.rotation;
        _openRot = transform.rotation * Quaternion.Euler(0, _openAngle, 0);

        _closedPos = transform.position;
        _openPos = transform.position + _slidingOffset;
    }

    #region Door Behaviour
    public void ToggleDoor()
    {
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);

        Vector3 targetPos = _isOpen ? _closedPos : _openPos;
        Quaternion targetRot = _isOpen ? _closedRot : _openRot;

        //if (animator != null) StopCoroutine(animCoroutine);

        //transform.position += isOpen ? new Vector3(0, 0, 3) : new Vector3(0, 0, -3);

        //Destroy(gameObject);

        if (_doorType == DOOR_TYPE.SWING)
        {
            _animCoroutine = StartCoroutine(RotateDoor(targetRot));
        }
        else if (_doorType == DOOR_TYPE.SLIDING)
        {
            _animCoroutine = StartCoroutine(SlideDoor(targetPos));
        }

        _isOpen = !_isOpen;
    }

    private IEnumerator RotateDoor(Quaternion targetRot)
    {
        Quaternion startRot = transform.rotation;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / _animDuration;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot; // snap finale
    }

    private IEnumerator SlideDoor(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / _animDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos; // snap finale
    }
    #endregion
}
