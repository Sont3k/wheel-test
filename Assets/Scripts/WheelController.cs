using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WheelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform linesParent;
    [SerializeField] private Transform PickerWheelTransform;
    [SerializeField] private Transform wheelCircle;
    [SerializeField] private PieceView wheelPiecePrefab;
    [SerializeField] private Transform wheelPiecesParent;
    [SerializeField] private Button _spinButton;

    [Header("Wheel Settings")]
    [Range(1, 20)] public int spinDuration = 8;
    [SerializeField] [Range(.2f, 2f)] 
    private float wheelSize = 1f;

    [Header("Pieces")]
    public PieceModel[] wheelPieces;

    // Events
    private Action _onSpinStartEvent;
    private Action<PieceModel> _onSpinEndEvent;

    private bool _isSpinning;

    private readonly Vector2 _pieceMinSize = new(81f, 146f);
    private readonly Vector2 _pieceMaxSize = new(144f, 213f);
    private readonly int _piecesMin = 2;
    private readonly int _piecesMax = 12;

    private float _pieceAngle;
    private float _halfPieceAngle;
    private float _halfPieceAngleWithPaddings;

    private double _accumulatedWeight;
    private readonly System.Random _rand = new();

    private readonly List<int> _nonZeroChancesIndices = new();

    private void OnValidate()
    {
        if (PickerWheelTransform != null)
        {
            PickerWheelTransform.localScale = new Vector3(wheelSize, wheelSize, 1f);
        }

        if (wheelPieces.Length > _piecesMax || wheelPieces.Length < _piecesMin)
        {
            Debug.LogError("[ PickerWheelwheel ]  pieces length must be between " + _piecesMin + " and " +
                           _piecesMax);
        }
    }

    private void OnEnable()
    {
        _spinButton.onClick.AddListener(Spin);
    }

    private void Start()
    {
        _pieceAngle = 360f / wheelPieces.Length;
        _halfPieceAngle = _pieceAngle / 2f;
        _halfPieceAngleWithPaddings = _halfPieceAngle - (_halfPieceAngle / 4f);

        Generate();

        CalculateWeightsAndIndices();
        if (_nonZeroChancesIndices.Count == 0)
            Debug.LogError("You can't set all pieces chance to zero");
    }

    private void OnDisable()
    {
        _spinButton.onClick.RemoveAllListeners();
    }

    private void Generate()
    {
        wheelPiecePrefab = InstantiatePiece();

        var rt = wheelPiecePrefab.PieceHolder.GetComponent<RectTransform>();
        var pieceWidth = Mathf.Lerp(_pieceMinSize.x, _pieceMaxSize.x,
            1f - Mathf.InverseLerp(_piecesMin, _piecesMax, wheelPieces.Length));
        var pieceHeight = Mathf.Lerp(_pieceMinSize.y, _pieceMaxSize.y,
            1f - Mathf.InverseLerp(_piecesMin, _piecesMax, wheelPieces.Length));
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);

        for (var i = 0; i < wheelPieces.Length; i++)
        {
            DrawPiece(i);
        }

        Destroy(wheelPiecePrefab.gameObject);
    }

    private void DrawPiece(int index)
    {
        var pieceData = wheelPieces[index];
        var instantiatedPiece = InstantiatePiece();
        instantiatedPiece.SetData(pieceData.Icon, pieceData.Label, pieceData.Amount.ToString());

        DrawLine(index, instantiatedPiece.PieceHolder);
    }

    private void DrawLine(int index, Transform pieceHolder)
    {
        var line = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent);
        
        line.transform.RotateAround(wheelPiecesParent.position, Vector3.back, (_pieceAngle * index) + _halfPieceAngle);
        pieceHolder.RotateAround(wheelPiecesParent.position, Vector3.back, _pieceAngle * index);
    }

    private PieceView InstantiatePiece()
    {
        return Instantiate(wheelPiecePrefab, wheelPiecesParent.position, Quaternion.identity, wheelPiecesParent);
    }

    private void Spin()
    {
        if (_isSpinning) return;
        
        _isSpinning = true;
        _onSpinStartEvent?.Invoke();

        var index = GetRandomPieceIndex();
        var piece = wheelPieces[index];

        if (piece.Chance == 0 && _nonZeroChancesIndices.Count != 0)
        {
            index = _nonZeroChancesIndices[Random.Range(0, _nonZeroChancesIndices.Count)];
            piece = wheelPieces[index];
        }

        var angle = -(_pieceAngle * index);

        var rightOffset = (angle - _halfPieceAngleWithPaddings) % 360;
        var leftOffset = (angle + _halfPieceAngleWithPaddings) % 360;

        var randomAngle = Random.Range(leftOffset, rightOffset);

        var targetRotation = Vector3.back * (randomAngle + 2 * 360 * spinDuration);

        var currentAngle = wheelCircle.eulerAngles.z;
        var prevAngle = wheelCircle.eulerAngles.z;

        var isIndicatorOnTheLine = false;

        wheelCircle
            .DORotate(targetRotation, spinDuration)
            .SetEase(Ease.InOutQuart)
            .OnUpdate(() =>
            {
                var diff = Mathf.Abs(prevAngle - currentAngle);
                if (diff >= _halfPieceAngle)
                {
                    prevAngle = currentAngle;
                    isIndicatorOnTheLine = !isIndicatorOnTheLine;
                }

                currentAngle = wheelCircle.eulerAngles.z;
            })
            .OnComplete(() =>
            {
                _isSpinning = false;
                _onSpinEndEvent?.Invoke(piece);

                _onSpinStartEvent = null;
                _onSpinEndEvent = null;
            });
    }

    private int GetRandomPieceIndex()
    {
        var r = _rand.NextDouble() * _accumulatedWeight;

        for (var i = 0; i < wheelPieces.Length; i++)
        {
            if (wheelPieces[i].Weight >= r)
            {
                return i;
            }
        }

        return 0;
    }

    private void CalculateWeightsAndIndices()
    {
        for (var i = 0; i < wheelPieces.Length; i++)
        {
            var piece = wheelPieces[i];

            //add weights:
            _accumulatedWeight += piece.Chance;
            piece.Weight = _accumulatedWeight;

            //add index :
            piece.Index = i;

            //save non zero chance indices:
            if (piece.Chance > 0)
            {
                _nonZeroChancesIndices.Add(i);
            }
        }
    }
}