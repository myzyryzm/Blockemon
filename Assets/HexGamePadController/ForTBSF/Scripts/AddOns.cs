/*You will need to make some additions to the following existing "Turn-Based Strategy Framework" scripts:
 * 
 * Cell
 * MyHexagon
 * Unit
 * MyUnit
 * 
    ===============================================================================
    ===============================================================================
    Cell:
 
     Add the following code to the Cell.cs script (NOTE: You will also need to change the access level of 
     the OnMouseDown method in Cell.cs(line 53) to "protected virtual void"):  


    protected virtual void CursorEnter()
    {

        if (CellHighlighted != null)
            CellHighlighted.Invoke(this, new EventArgs());

    }
    protected virtual void CursorExit()
    {
        if (CellDehighlighted != null)
            CellDehighlighted.Invoke(this, new EventArgs());


    }

    protected virtual void CursorDown()
    {

        if (CellClicked != null)
            CellClicked.Invoke(this, new EventArgs());
    }

    ==============================================================================
    ==============================================================================

    MyHexagon:

    Add the following code to the MyHexagon.cs script:


    protected override void OnMouseDown()
    {
        if (!GamePadControl.gamePadActive)
        {
            base.OnMouseDown();
        }

    }

    protected override void OnMouseEnter()
    {

        if (!GamePadControl.gamePadActive)
        {


            base.OnMouseEnter();
        }
    }

    protected override void OnMouseExit()
    {
        if (!GamePadControl.gamePadActive)
        {
            base.OnMouseExit();
        }
    }

    public void GamePadDown()
    {
        if (GamePadControl.gamePadActive)
        {
            CursorDown();
        }
    }

    protected override void CursorDown()
    {
        base.CursorDown();
    }

    public void GamePadEnter()
    {

        if (GamePadControl.gamePadActive)
        {
            CursorEnter();
        }


    }
    protected override void CursorEnter()
    {
        base.CursorEnter();
    }
    public void GamePadExit()
    {
        if (GamePadControl.gamePadActive)
        {
            CursorExit();
        }
    }

    protected override void CursorExit()
    {
        base.CursorExit();
    }


   ====================================================================
   ====================================================================

   Unit:

   Add the following code to the Unit.cs script:

   
    protected virtual void CursorDown()
    {
        if (UnitClicked != null)
            UnitClicked.Invoke(this, new EventArgs());

    }

   

    protected virtual void CursorEnter()
    {
        if (UnitHighlighted != null)
            UnitHighlighted.Invoke(this, new EventArgs());

    }


 

    protected virtual void CursorExit()
    {

        if (UnitDehighlighted != null)
            UnitDehighlighted.Invoke(this, new EventArgs());
    }



    ===================================================================
    ===================================================================

    MyUnit:

    Add the following code to the MyUnit.cs script:

    
    protected override void OnMouseDown()
    {
        if (!GamePadControl.gamePadActive)
        {
            base.OnMouseDown();
        }

    }

    protected override void OnMouseEnter()
    {

        if (!GamePadControl.gamePadActive)
        {
            base.OnMouseEnter();
        }
    }

    protected override void OnMouseExit()
    {
        if (!GamePadControl.gamePadActive)
        {
            base.OnMouseExit();
        }
    }


    public void GamePadDown()
    {
        if (GamePadControl.gamePadActive)
        {
            CursorDown();
        }
    }

    protected override void CursorDown()
    {
        base.CursorDown();
    }

    public void GamePadOver()
    {

        if (GamePadControl.gamePadActive)
        {
            CursorEnter();
        }


    }
    protected override void CursorEnter()
    {
        base.CursorEnter();
    }
    public void GamePadExit()
    {
        if (GamePadControl.gamePadActive)
        {
            CursorExit();
        }
    }

    protected override void CursorExit()
    {
        base.CursorExit();
    }


    ======================================================
    ======================================================
    */
