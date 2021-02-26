using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple debug free fly camera
/// </summary>
public class FreeCam : MonoBehaviour
{
    CameraState targetCameraState = new CameraState();
    CameraState interpolatingCameraState = new CameraState();

    public float boost = 3.5f;

    [Range(0.001f, 1f)] public float positionLerpTime = 0.2f;

    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Range(0.001f, 1f)] public float rotationLerpTime = 0.01f;

    public bool invertY = false;

    private void OnEnable()
    {
        targetCameraState.SetFromTransform(transform);
        interpolatingCameraState.SetFromTransform(transform);

        Cursor.visible = false;
    }

    private Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = new Vector3();

        // Don't do this kids, do Vector2 GetAxis Horizontal and Vertical...
        if(Input.GetKey(KeyCode.W)) { direction += Vector3.forward; }
        if(Input.GetKey(KeyCode.S)) { direction += Vector3.back; }
        if(Input.GetKey(KeyCode.A)) { direction += Vector3.left; }
        if(Input.GetKey(KeyCode.D)) { direction += Vector3.right; }

        if(Input.GetKey(KeyCode.LeftShift)) { direction += Vector3.down; }
        if(Input.GetKey(KeyCode.Space)) { direction += Vector3.up; }

        return direction;
    }

    private void Update()
    {
        // Hide and lock cursor when right mouse button pressed
        if(Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if(Input.GetMouseButtonUp(1)) // Unlock and show cursor when right mouse button released
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Rotation
        if(Input.GetMouseButton(1))
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            targetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            targetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        var translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement
        if(Input.GetKey(KeyCode.LeftControl)) { translation *= 10.0f; }

        // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
        boost += Input.mouseScrollDelta.y * 0.2f;
        translation *= Mathf.Pow(2.0f, boost);

        targetCameraState.Translate(translation);

        // Framerate independent interp
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

        interpolatingCameraState.LerpTowards(targetCameraState, positionLerpPct, rotationLerpPct);

        interpolatingCameraState.UpdateTransform(transform);
    }
}
