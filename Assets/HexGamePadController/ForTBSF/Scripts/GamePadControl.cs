

using UnityEngine;
using UnityEngine.EventSystems;
using TbsFramework.Grid;
using TbsFramework.Example1;

namespace HexGamePadController 
{
    public class GamePadControl : MonoBehaviour
    {

        //Inspector variables

        public GameObject unitsParent;
        public GameObject eventSystem;
        public CellGrid cellGrid;


        //optional Inspector variables (just make public)
        private float stickyHexDelay = 0.1f; //delay in cursor moving from one hex to another
        private float buttonSpeed = 0.35f; //delay before button presses are recognized again
        private float speed = 5f;    // cursor move speed
        public float cursorheight = 1.2f;  //height of the cursor above the map


        //cursor movement variables
        private float xPos;
        private float zPos;
        private int directionX;
        private int directionZ;
        private bool upDownMover = true;
        private bool shouldMove = true;




        //hex and unit interaction variables

        private GameObject currentHex, oldHex, currentUnit, oldUnit, selectedUnit;
        private bool buttonDelayer = false;

        private LayerMask layerMask;
        private RaycastHit hit;
        private Vector3 objPos;
        private int indexNumber = 0; // used for cycling between units



        //mouse and gamepad variables
        [HideInInspector]
        public static bool gamePadActive;

        //player variables
        [HideInInspector]
        public static int joystickNumber = 0;
        private bool[] gamePads;

        private void Awake()
        {
        EventSystemUpdater();
        }
        // Use this for initialization
        void Start()
        {

        //set initial cursor position based on first unit in list
        currentHex = cellGrid.transform.GetChild(0).GetComponent<MyHexagon>().gameObject;
        CursorSnapToObject(currentHex);
        oldHex = currentHex;
        gamePadActive = true;
        joystickNumber = cellGrid.CurrentPlayerNumber;
        GamePadMouseSettings();
        gamePads = new bool[4];  //initializes joystick number array
        InvokeRepeating("CheckJoysticks", 0.5f, 1f);

        }


        //check which joysticks are active
        private void CheckJoysticks()
        {
        for (int i = 0; i < Input.GetJoystickNames().Length; i++)
        {
            if (Input.GetJoystickNames()[i] == "")
            {
                gamePads[i] = false;
            }
            else
            {
                gamePads[i] = true;
            }
        }

        if (gamePads[cellGrid.CurrentPlayerNumber])
        {
            joystickNumber = cellGrid.CurrentPlayerNumber;
        }
        else
        {
            joystickNumber = 0;
        }
        EventSystemUpdater();
        }

        //updates the Event System to know which joystick is being used. Needed for menus.
        private void EventSystemUpdater()
        {

        var eventInput = eventSystem.GetComponent<StandaloneInputModule>();
        eventInput.verticalAxis = "LeftStickY" + joystickNumber;
        eventInput.horizontalAxis = "LeftStickX" + joystickNumber;
        eventInput.submitButton = "Submit" + joystickNumber;
        eventInput.cancelButton = "Cancel" + joystickNumber;

        }

        void LateUpdate()
        {

        if (Input.GetKeyDown("m"))
        {
            GamePadToggle();
        }
        if (gamePadActive)
        {
            UsingGamePad();
        }

        //check for button presses
        if (!buttonDelayer)
        {
            ButtonInput();
        }
        }

        //toggles mouse or gamepad
        public void GamePadToggle()
        {
        gamePadActive = !gamePadActive;
        GamePadMouseSettings();
        }

        //sets up game for mouse or gamepad as chosen
        public void GamePadMouseSettings()
        {
        if (gamePadActive)
        {
            GetComponentInChildren<MeshRenderer>().enabled = true;
            Cursor.visible = false;
        }
        else
        {
            GetComponentInChildren<MeshRenderer>().enabled = false;
            Cursor.visible = true;
        }
        }

        //get the current position of the gamepad or mouse
        public void GetMouseOrGamepadPosition()
        {
        if (!gamePadActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            layerMask = 1 << (int)Layer.Hex;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {

                if (hit.transform)
                {
                    objPos = hit.transform.position;
                }
            }
        }
        else
        {
            objPos = transform.position;
        }
        }

        //general-purpose raycaster, returns a gameobject
        public GameObject GenericRayCaster(Vector3 selectedPos, int layerMaskValue)
        {
        layerMask = 1 << layerMaskValue;

        var castX = selectedPos.x;
        var castY = selectedPos.y + 5f;
        var castZ = selectedPos.z;
        var castingPos = new Vector3(castX, castY, castZ);
        Physics.Raycast(castingPos, -Vector3.up, out hit, Mathf.Infinity, layerMask);


        if (hit.transform)
        {
            return hit.transform.gameObject;

        }
        else
        {
            return null;
        }


        }

        //send a raycast to ID the hex
        private void HexRayCaster()
        {
        var hexFound = GenericRayCaster(transform.position, (int)Layer.Hex);

        if (hexFound)
        {

            currentHex = hexFound;
            xPos = currentHex.transform.position.x;
            zPos = currentHex.transform.position.z;
        }
        else
        {
            currentHex = oldHex;
        }

        }

        //send a raycast to ID the unit
        private void UnitRayCaster()
        {


        int[] layers = new int[2];
        layers[0] = (int)Layer.Friendly;
        layers[1] = (int)Layer.Enemy;


        GetMouseOrGamepadPosition();

        foreach (int layer in layers)
        {
            var unitFound = GenericRayCaster(objPos, layer);


            if (unitFound)
            {
                currentUnit = unitFound;
            }

        }

        }

        //game code for using the gamepad
        private void UsingGamePad()
        {
        oldUnit = currentUnit;
        oldHex = currentHex;
        currentUnit = null;
        currentHex = null;
        HexRayCaster();
        UnitRayCaster();
        Move();
        }

        //move the cursor
        private void Move()
        {
        //needed to make upDownMover work properly
        directionX = 0;
        directionZ = 0;

        if (shouldMove)
        {
            //determines the direction of input

            directionX = Mathf.RoundToInt(Input.GetAxis("LeftStickX" + joystickNumber));
            directionZ = Mathf.RoundToInt(Input.GetAxis("LeftStickY" + joystickNumber));

            // if input stops, snap the cursor to the hex it was moving toward
            if (directionX == 0 && directionZ == 0)
            {
                SnapToTargetHex();

            }
            //if the joystick is being moved
            else if (directionX != 0 || directionZ != 0)
            {

                ////move the cursor as long as it's in the same hex it started in
                if (oldHex.transform.position == currentHex.transform.position)
                {

                    //handles purely horizontal direction for flat top hex grids
                    if (directionX != 0 && directionZ == 0)
                    {

                        if (upDownMover)
                        {
                            directionZ = 1;
                        }
                        else
                        {
                            directionZ = -1;
                        }
                    }
                    //Looks ahead to see if there's a null hex coming up.  Avoids rubberbanding at the edges.
                    GamePadEdgeDetection();


                    //move the cursor
                    float step = speed * Time.deltaTime;
                    transform.Translate(new Vector3(directionX, 0, directionZ) * step);

                }
                //snap the cursor to the hex it was moving toward, then wait a bit to give it a "sticky" feel
                else
                {
                    shouldMove = false;
                    transform.position = new Vector3(xPos, cursorheight, zPos);

                    oldHex.GetComponent<MyHexagon>().GamePadExit();
                    currentHex.GetComponent<MyHexagon>().GamePadEnter();

                    oldHex = currentHex;
                    Invoke("HexStop", stickyHexDelay);
                }
            }
        }
        }
        //look for map edge
        private void GamePadEdgeDetection()
        {
        var newXPos = (xPos + directionX);
        var newZPos = (zPos + directionZ * 1.5f);

        Vector3 lookAheadPosition = new Vector3(newXPos, cursorheight, newZPos);

        var hexFound = GenericRayCaster(lookAheadPosition, (int)Layer.Hex);

        if (!hexFound)
        {
            shouldMove = false;
            transform.position = new Vector3(xPos, cursorheight, zPos);
            Invoke("HexStop", stickyHexDelay);
        }


        }

        //snap cursor to the hex it's moving to
        private void SnapToTargetHex()
        {
        var newXPos = (xPos + directionX);
        var newZPos = (zPos + directionZ * 1.5f);
        shouldMove = false;
        transform.position = new Vector3(newXPos, cursorheight, newZPos);
        //centers cursor in the new hex
        HexRayCaster();
        transform.position = new Vector3(xPos, cursorheight, zPos);

        HexStop();
        }

        //switches some bools when movement stops
        private void HexStop()
        {
        upDownMover = !upDownMover;
        shouldMove = true;
        }

        //button input with a built-in delay so the button doesn't click too quickly
        private void ButtonInput()
        {

        //select button
        if (Input.GetAxis("Submit" + joystickNumber) > 0 && gamePadActive)
        {
            UnitRayCaster();
            HexRayCaster();
            buttonDelayer = true;

            // if there's a unit, select it.  Otherwise, select the hex.
            if (currentUnit)
            {
                currentUnit.GetComponent<MyUnit>().GamePadDown();
                selectedUnit = currentUnit;
                currentUnit.GetComponent<MyUnit>().GamePadExit();
            }
            else
            {
                currentHex.GetComponent<MyHexagon>().GamePadDown();
            }
            Invoke("ChangeButtonClicked", buttonSpeed);
        }
        //cancel selection button
        if (Input.GetAxis("Cancel" + joystickNumber) > 0 && gamePadActive)
        {
            if (selectedUnit)
            {
                buttonDelayer = true;

                //note: this code is TBTK-specific. 

                selectedUnit.GetComponent<MyUnit>().Cell.IsTaken = true;
                selectedUnit.GetComponent<MyUnit>().Cell.GetComponent<MyHexagon>().GamePadDown();
                selectedUnit.GetComponent<MyUnit>().Cell.IsTaken = false;
                selectedUnit = null;

                Invoke("ChangeButtonClicked", buttonSpeed);
            }
        }

        //end turn button
        if (Input.GetAxis("EndTurn" + joystickNumber) > 0 && gamePadActive)
        {
            buttonDelayer = true;
            cellGrid.EndTurn();
            Invoke("ChangeButtonClicked", buttonSpeed);
        }



        // cycle to next unit button
        if (Input.GetAxis("RB" + joystickNumber) > 0)
        {
            buttonDelayer = true;
            FindNextIdleUnit();

            Invoke("ChangeButtonClicked", buttonSpeed);
        }


        // cycle to previous unit button
        if (Input.GetAxis("LB" + joystickNumber) > 0)
        {

            buttonDelayer = true;
            FindPreviousIdleUnit();
            Invoke("ChangeButtonClicked", buttonSpeed);
        }


        }

        //avoids button press being registered too many times
        private void ChangeButtonClicked()
        {
        buttonDelayer = false;
        }


        //finds units in the Unit list, going up
        private void FindNextIdleUnit()
        {

        GameObject startUnit = currentUnit;

        //start looking at either the current unit or, if no unit selected, the top of the unit list
        if (!currentUnit)
        {
            indexNumber = 0;
        }
        else
        {
            indexNumber = currentUnit.transform.GetSiblingIndex();
        }

        //start looking from the currentUnit sibling position
        for (int i = indexNumber; i < unitsParent.transform.childCount; i++)
        {
            IdleUnitSelector(i);
            if (startUnit != currentUnit)
            {
                return;
            }
        }

        //if nothing found, start looking from the top of the list
        indexNumber = 0;

        for (int i = indexNumber; i < unitsParent.transform.childCount; i++)
        {
            IdleUnitSelector(i);
            if (startUnit != currentUnit)
            {
                return;
            }
        }

        Debug.Log("No unit found!");
        }

        //finds units in the Unit list, going down
        private void FindPreviousIdleUnit()
        {
        GameObject startUnit = currentUnit;

        //start looking at either the current unit or, if no unit selected, the top of the unit list
        if (!currentUnit)
        {
            indexNumber = unitsParent.transform.childCount - 1;
        }
        else
        {
            indexNumber = currentUnit.transform.GetSiblingIndex();
        }

        //start looking from the currentUnit sibling position
        for (int i = indexNumber; i > -1; i--)
        {
            IdleUnitSelector(i);
            if (startUnit != currentUnit)
            {
                return;
            }
        }

        //if nothing found, start looking from the bottom of the list
        indexNumber = unitsParent.transform.childCount - 1;

        for (int i = indexNumber; i > -1; i--)
        {

            IdleUnitSelector(i);
            if (startUnit != currentUnit)
            {
                return;
            }
        }


        }

        //selects the unit found in previous two methods
        private void IdleUnitSelector(int i)
        {
        var unit = unitsParent.transform.GetChild(i).GetComponent<MyUnit>();
        if (unit != null)
        {
            //check if the unit is friendly, then if so snap to it
            if (unit.gameObject.layer == 9)
            {
                currentUnit = unit.gameObject;
                CursorSnapToObject(currentUnit);
                currentUnit.GetComponent<MyUnit>().GamePadDown();
            }
        }
        }

        // snap camera and cursor to x position of unit selected
        public void CursorSnapToObject(GameObject obj)
        {
        var objX = obj.transform.position.x;
        var objZ = obj.transform.position.z;
        transform.position = new Vector3(objX, cursorheight, objZ);
        HexRayCaster();
        }


    }
}












