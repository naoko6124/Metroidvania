using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform characterTransform;
    [SerializeField] private float flipYRotationTime;
    private CharacterMovement characterMovement;
    private float facingDirection;

    private void Awake()
    {
        characterMovement = characterTransform.GetComponent<CharacterMovement>();
        facingDirection = characterMovement.facingDirection;
    }

    private void Update()
    {
        transform.position = characterTransform.position;
        if (facingDirection != characterMovement.facingDirection)
        {
            facingDirection = characterMovement.facingDirection;
            LeanTween.rotateY(gameObject, 90f - facingDirection * 90f, flipYRotationTime).setEaseInOutSine();
        }
    }
}
