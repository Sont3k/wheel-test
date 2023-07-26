using System;
using _App.Scripts.Piece;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _App.Scripts
{
    public class WheelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private Transform linesParent;
        [SerializeField] private Transform wheelCircle;
        [SerializeField] private PieceView wheelPiecePrefab;
        [SerializeField] private Transform wheelPiecesParent;

        [Header("Wheel Settings")]
        [Range(1, 20)] public int spinDuration = 8;
        [SerializeField] private bool _isIndicatorOnTop;

        [Header("Pieces")]
        public PieceModel[] wheelPieces;

        public bool IsSpinning { get; private set; }

        private readonly Vector2 _pieceMinSize = new(81f, 146f);
        private readonly Vector2 _pieceMaxSize = new(144f, 213f);
        private readonly int _piecesMin = 2;
        private readonly int _piecesMax = 12;

        private float _pieceAngle;
        private float _halfPieceAngle;
        private float _halfPieceAngleWithPaddings;

        private void OnValidate()
        {
            HandlePiecesSizeInEditor();
        }

        private void Start()
        {
            _pieceAngle = 360f / wheelPieces.Length;
            _halfPieceAngle = _pieceAngle / 2f;
            _halfPieceAngleWithPaddings = _halfPieceAngle - (_halfPieceAngle / 4f);

            Generate();
            AssignIndexes();
        }
        
        private void HandlePiecesSizeInEditor()
        {
            if (wheelPieces.Length < _piecesMin)
            {
                Debug.LogError($"Pieces array should be more than {_piecesMin}");
            }

            if (wheelPieces.Length > _piecesMax)
            {
                Debug.LogError($"Pieces array should be less or equal to {_piecesMax}");
            }
        }

        private void Generate()
        {
            wheelPiecePrefab = Instantiate(wheelPiecePrefab, wheelPiecesParent);

            var rt = wheelPiecePrefab.GetComponent<RectTransform>();
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
            var instantiatedPiece = Instantiate(wheelPiecePrefab, wheelPiecesParent);
            instantiatedPiece.SetData(pieceData, OnPieceClick);

            DrawLine(index, instantiatedPiece.transform);
        }

        private void DrawLine(int index, Transform pieceTransform)
        {
            var line = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent);
        
            line.transform.RotateAround(wheelPiecesParent.position, Vector3.back, (_pieceAngle * index) + _halfPieceAngle);
            pieceTransform.RotateAround(wheelPiecesParent.position, Vector3.back, _pieceAngle * index);
        }

        private void Spin(int pieceIndex)
        {
            if (IsSpinning) return;
        
            IsSpinning = true;

            var angle = GetPieceAngle(pieceIndex);

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
                    IsSpinning = false;
                });
        }

        private float GetPieceAngle(int pieceIndex)
        {
            float angle;
            
            if (_isIndicatorOnTop)
            {
                angle = -(_pieceAngle * pieceIndex);
            }
            else
            {
                angle = -(_pieceAngle * pieceIndex) + 180;
            }
            
            return angle;
        }
        
        private void AssignIndexes()
        {
            for (var i = 0; i < wheelPieces.Length; i++)
            {
                var piece = wheelPieces[i];
                piece.Index = i;
            }
        }
        
        private void OnPieceClick(PieceView pieceView)
        {
            if (IsSpinning) return;
            pieceView.DisablePiece();
            Spin(pieceView.Data.Index);
        }
    }
}