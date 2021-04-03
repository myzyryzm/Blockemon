
using UnityEngine;

namespace HexGamePadController
{
    public class CameraControl : MonoBehaviour

    {
        //note that all variables for keyboard also apply to gamepads

        private Transform camTrans;
        public float mouseEdgeSpeed = 3f; //how fast the screen scrolls when the mouse hits the sides of the screen
        public float mouseScreenEdgeBorder = 25f; //how big the zone is that triggers mouse screen scrolling
        public float followingSpeed = 20f; //speed when following a target. Bigger is smoother.
        public float gamepadRotationSpeed = 3f; // how fast the keyboard/gamepad rotates the view
        public float gamepadMovementSpeed = 5f; // how fast the keyboard/gamepad moves the camera
        public float mousePanSpeed = 10f; //how fast the mouse pans around
        public float mouseRotationSpeed = 10f; //how fast the mouse rotates the view
        public float cameraAngle = 45f; //the angle of view of the camera.  
        public float minHeight = 7f; //minimum height of camera above map
        public float maxZoom = 20f; //maximum zoom limit
        public float minZoom = 5f; //minimum zoom limit
        public float gamepadZoomSpeed = 10f;//how fast the keyboard/gamepad zooms
        public float MouseZoomSpeed = 10f; //how fast the mouse zooms
        public Transform target; //target to follow
        public Vector3 targetOffset;//the offset of the camera from the target.  Setting z=-12 is recommended.


        private float zoomSpeed;
        private Vector3 velocity = Vector3.zero;

        private KeyCode mousePanButton = KeyCode.Mouse2;

        private KeyCode mouseRotationButton = KeyCode.Mouse1;

        private Vector2 MouseInput
        {
        get { return Input.mousePosition; }
        }

        private Vector2 MouseAxis
        {
        get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
        }

        private int RotationDirection
        {
        get
        {

            return Mathf.RoundToInt(Input.GetAxis("RightStickX" + GamePadControl.joystickNumber));

        }
        }

        private int zoom;
        private Vector3 lastCamPosition;
        private RaycastHit hit;
        LayerMask layerMask = 1 << (int)Layer.Hex;


        private void Start()
        {
        camTrans = transform;
        //set camera angle
        transform.localEulerAngles = new Vector3(cameraAngle, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        private void Update()
        {
        if (GamePadControl.gamePadActive)
        {
            FollowTarget();
        }
        else
        {
            MouseScreenPan();
        }

        Zoom();
        CamForwardBack();
        Rotate();
        }

        // move camera forward and back (reverts back to target after)
        private void CamForwardBack()
        {


        Vector3 desiredMove = new Vector3(0, 0, (-Mathf.RoundToInt(Input.GetAxis("RightStickY" + GamePadControl.joystickNumber))));

        desiredMove *= gamepadMovementSpeed;
        desiredMove *= Time.deltaTime;
        desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
        desiredMove = camTrans.InverseTransformDirection(desiredMove);

        camTrans.Translate(desiredMove, Space.Self);

        }

        //mouse controls
        private void MouseScreenPan()
        {


        Vector3 mouseMovement = new Vector3();

        Rect leftRect = new Rect(0, 0, mouseScreenEdgeBorder, Screen.height);
        Rect rightRect = new Rect(Screen.width - mouseScreenEdgeBorder, 0, mouseScreenEdgeBorder, Screen.height);
        Rect upRect = new Rect(0, Screen.height - mouseScreenEdgeBorder, Screen.width, mouseScreenEdgeBorder);
        Rect downRect = new Rect(0, 0, Screen.width, mouseScreenEdgeBorder);

        mouseMovement.x = leftRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
        mouseMovement.z = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

        mouseMovement *= mouseEdgeSpeed;
        mouseMovement *= Time.deltaTime;
        mouseMovement = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * mouseMovement;
        mouseMovement = camTrans.InverseTransformDirection(mouseMovement);



        lastCamPosition = transform.position;
        camTrans.Translate(mouseMovement, Space.Self);
        MouseEdgeDetection();


        if (Input.GetKey(mousePanButton) && MouseAxis != Vector2.zero)
        {
            Vector3 desiredPanMove = new Vector3(-MouseAxis.x, 0, -MouseAxis.y);

            desiredPanMove *= mousePanSpeed;
            desiredPanMove *= Time.deltaTime;
            desiredPanMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredPanMove;
            desiredPanMove = camTrans.InverseTransformDirection(desiredPanMove);
            lastCamPosition = transform.position;
            camTrans.Translate(desiredPanMove, Space.Self);
            MouseEdgeDetection();
        }
        }

        //rotate the camera
        private void Rotate()
        {

        transform.Rotate(Vector3.up, RotationDirection * Time.deltaTime * gamepadRotationSpeed, Space.World);

        if (Input.GetKey(mouseRotationButton))
            camTrans.Rotate(Vector3.up, -MouseAxis.x * Time.deltaTime * mouseRotationSpeed, Space.World);
        }

        //follow the cursor with the camera
        private void FollowTarget()
        {
        Vector3 targetPos = new Vector3(target.position.x, camTrans.position.y, target.position.z) + targetOffset;


        camTrans.position = Vector3.SmoothDamp(camTrans.position, targetPos, ref velocity, Time.deltaTime * followingSpeed);
        transform.localEulerAngles = new Vector3(cameraAngle, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        //EdgeDetection for mouse so the camera doesn't go offscreen
        private void MouseEdgeDetection()
        {

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out hit))
        {
            Camera.main.transform.position = lastCamPosition;


        }



        }

        //zooms the camera in and out
        private void Zoom()
        {
        Vector3 dir = target.transform.position - Camera.main.transform.position;

        //raycast from the main camera to the target



        if (Physics.Raycast(camTrans.position, dir, out hit))
        {
            if (hit.transform)
            {
                var normalHeading = hit.transform.position - Camera.main.transform.position;
                var distance = normalHeading.magnitude;

                //determine if using mouse or gamepad
                if (GamePadControl.gamePadActive == true)
                {
                    zoom = Mathf.RoundToInt(Input.GetAxis("LTRT" + GamePadControl.joystickNumber));
                    zoomSpeed = gamepadZoomSpeed * Time.deltaTime;
                }
                else
                {

                    zoom = Mathf.RoundToInt(Input.GetAxis("ScrollWheel"));
                    zoomSpeed = MouseZoomSpeed * Time.deltaTime;
                }

                //stop zooming if too far


                if (distance > maxZoom && zoom > 0)
                {
                    return;
                }

                //stop zooming if too close
                if (distance < minZoom && zoom < 0)
                {
                    return;
                }

                //zoom in

                if (zoom < 0 && distance >= minZoom && Camera.main.transform.position.y > minHeight)
                {
                    Camera.main.transform.position += (normalHeading.normalized * zoomSpeed);
                }

                //zoom out

                if (zoom > 0 && distance <= maxZoom)
                {
                    Camera.main.transform.position -= (normalHeading.normalized * zoomSpeed);
                }
            }

        }


        }
    }
}




