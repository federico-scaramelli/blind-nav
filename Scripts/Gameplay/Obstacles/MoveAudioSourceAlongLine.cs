using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAudioSourceAlongLine : MonoBehaviour
{
    [SerializeField] private Transform audioSourceTransform;
    [SerializeField] private Transform playerTransform;
    
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform endTransform;

    private Vector2 _startPoint;
    private Vector2 _endPoint;
    private Vector2 _lineVector;
    private float _lineLength;
    private float _dotProduct;
    private Vector2 _newPosition;

    private void Awake()
    {
        playerTransform = FindObjectOfType<PlayerLevelInteraction>().transform;
    }

    void Start()
    {
        var startPosition = startTransform.position;
        _startPoint = new Vector2(startPosition.x, startPosition.z);
        var endPosition = endTransform.position;
        _endPoint = new Vector2(endPosition.x, endPosition.z);
        _lineVector = _endPoint - _startPoint;
        _lineLength = _lineVector.magnitude;
        audioSourceTransform.position = startPosition;
    }

    void Update()
    {
        var playerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);
        _dotProduct = Vector2.Dot(playerPos - _startPoint, _lineVector.normalized);

        _dotProduct = Mathf.Clamp(_dotProduct, 0, _lineLength);

        _newPosition = _startPoint + _lineVector.normalized * _dotProduct;
        audioSourceTransform.position = new Vector3(_newPosition.x, audioSourceTransform.position.y, _newPosition.y);
    }
}
