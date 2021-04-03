﻿using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FollowCameraControls : FollowCamera
{
    [Header("Controls")]
    public bool updateRotation = true;
    public bool updateRotationX = false;
    public bool updateRotationY = false;
    public bool updateZoom = true;

    [Header("X Rotation")]
    public bool limitXRotation;
    [Range(-360, 360)]
    public float minXRotation = 0;
    [Range(-360, 360)]
    public float maxXRotation = 0;
    public bool smoothRotateX;
    public float rotateXSmoothing = 10.0f;

    [Header("Y Rotation")]
    public bool limitYRotation;
    [Range(-360, 360)]
    public float minYRotation = 0;
    [Range(-360, 360)]
    public float maxYRotation = 0;
    public bool smoothRotateY;
    public float rotateYSmoothing = 10.0f;

    [Header("General Rotation Settings")]
    public float startXRotation;
    public float startYRotation;
    public float rotationSpeed = 5;
    [Range(0.1f, 1f)]
    public float rotationSpeedScale = 1;

    [Header("Zoom")]
    public bool limitZoomDistance;
    public float minZoomDistance;
    public float maxZoomDistance;
    public bool smoothZoom;
    public float zoomSmoothing = 10.0f;

    [Header("General Zoom Settings")]
    public float startZoomDistance;
    public float zoomSpeed = 5;
    [Range(0.1f, 1f)]
    public float zoomSpeedScale = 1;

    [Header("Aim Assist")]
    public bool enableAimAssist = false;
    public bool enableAimAssistX = true;
    public bool enableAimAssistY = true;
    public float aimAssistRadius = 0.5f;
    public float aimAssistDistance = 10f;
    public LayerMask aimAssistLayerMask;
    public float aimAssistXSpeed = 10f;
    public float aimAssistYSpeed = 10f;
    [Range(0f, 360f)]
    public float aimAssistAngleLessThan = 360f;

    [Header("Save Camera")]
    public bool isSaveCamera;
    public string savePrefsPrefix = "GAMEPLAY";

    public IAimAssistAvoidanceListener AimAssistAvoidanceListener { get; set; }
    public float XRotationVelocity { get; set; }
    public float YRotationVelocity { get; set; }
    public float ZoomVelocity { get; set; }

    private float deltaTime;
    private RaycastHit aimAssistCastHit;

    private void Start()
    {
        xRotation = startXRotation;
        yRotation = startYRotation;
        zoomDistance = startZoomDistance;

        if (isSaveCamera)
        {
            xRotation = PlayerPrefs.GetFloat(savePrefsPrefix + "_XRotation", xRotation);
            yRotation = PlayerPrefs.GetFloat(savePrefsPrefix + "_YRotation", yRotation);
            zoomDistance = PlayerPrefs.GetFloat(savePrefsPrefix + "_ZoomDistance", zoomDistance);
        }
    }

    private void Update()
    {
        // Update delta time, this also being used in LateUpdate too.
        deltaTime = Time.deltaTime;

        if (isSaveCamera)
        {
            PlayerPrefs.SetFloat(savePrefsPrefix + "_XRotation", xRotation);
            PlayerPrefs.SetFloat(savePrefsPrefix + "_YRotation", yRotation);
            PlayerPrefs.SetFloat(savePrefsPrefix + "_ZoomDistance", zoomDistance);
            PlayerPrefs.Save();
        }

        // X rotation
        if (updateRotation || updateRotationX)
            XRotationVelocity += InputManager.GetAxis("Mouse Y", false) * rotationSpeed * rotationSpeedScale;
        xRotation -= XRotationVelocity;
        if (limitXRotation)
            xRotation = ClampAngleBetweenMinAndMax(xRotation, minXRotation, maxXRotation);
        else
            xRotation = ClampAngleBetweenMinAndMax(xRotation, -360, 360);

        // Y rotation
        if (updateRotation || updateRotationY)
            YRotationVelocity += InputManager.GetAxis("Mouse X", false) * rotationSpeed * rotationSpeedScale;
        yRotation += YRotationVelocity;
        if (limitYRotation)
            yRotation = ClampAngleBetweenMinAndMax(yRotation, minYRotation, maxYRotation);
        else
            yRotation = ClampAngleBetweenMinAndMax(yRotation, -360, 360);

        // Zoom
        if (updateZoom)
            ZoomVelocity += InputManager.GetAxis("Mouse ScrollWheel", false) * zoomSpeed * zoomSpeedScale;
        zoomDistance += ZoomVelocity;
        if (limitZoomDistance)
            zoomDistance = Mathf.Clamp(zoomDistance, minZoomDistance, maxZoomDistance);

        // X rotation smooth
        if (smoothRotateX)
            XRotationVelocity = Mathf.LerpAngle(XRotationVelocity, 0, deltaTime * rotateXSmoothing);
        else
            XRotationVelocity = 0f;

        // Y rotation smooth
        if (smoothRotateY)
            YRotationVelocity = Mathf.LerpAngle(YRotationVelocity, 0, deltaTime * rotateYSmoothing);
        else
            YRotationVelocity = 0f;

        // Zoom smooth
        if (smoothZoom)
            ZoomVelocity = Mathf.Lerp(ZoomVelocity, 0, deltaTime * zoomSmoothing);
        else
            ZoomVelocity = 0f;
    }

    protected override void LateUpdate()
    {
        if (enableAimAssist && Application.isPlaying)
        {
            RaycastHit[] hits = Physics.SphereCastAll(CacheCameraTransform.position, aimAssistRadius, CacheCameraTransform.forward, aimAssistDistance, aimAssistLayerMask);
            RaycastHit tempHit;
            Vector3 cameraDir = CacheCameraTransform.forward;
            Vector3 targetDir;
            for (int i = 0; i < hits.Length; ++i)
            {
                tempHit = hits[i];
                if (AimAssistAvoidanceListener != null && AimAssistAvoidanceListener.AvoidAimAssist(tempHit))
                    continue;
                targetDir = (tempHit.point - target.position).normalized;
                if (Vector3.Angle(cameraDir, targetDir) > aimAssistAngleLessThan)
                    continue;
                // Set `xRotation`, `yRotation` by hit object's position
                aimAssistCastHit = tempHit;
                Vector3 targetCenter = aimAssistCastHit.collider.bounds.center;
                Vector3 directionToTarget = (targetCenter - CacheCameraTransform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                if (enableAimAssistX)
                    xRotation = Mathf.MoveTowardsAngle(xRotation, lookRotation.eulerAngles.x, aimAssistXSpeed * deltaTime);
                if (enableAimAssistY)
                    yRotation = Mathf.MoveTowardsAngle(yRotation, lookRotation.eulerAngles.y, aimAssistYSpeed * deltaTime);
                break;
            }
        }
        base.LateUpdate();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
#if UNITY_EDITOR
        Gizmos.color = Color.green;
        Gizmos.DrawLine(CacheCameraTransform.position, CacheCameraTransform.position + CacheCameraTransform.forward * aimAssistCastHit.distance);
        Gizmos.DrawWireSphere(CacheCameraTransform.position + CacheCameraTransform.forward * aimAssistCastHit.distance, aimAssistRadius);
#endif
    }

    private float ClampAngleBetweenMinAndMax(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
