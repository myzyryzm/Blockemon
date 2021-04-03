using TbsFramework.Cells;
using UnityEngine;
using HexGamePadController;

namespace TbsFramework.Example1
{
    class MyHexagon : Hexagon
    {
        private Renderer hexagonRenderer;
        private Renderer outlineRenderer;

        private Vector3 dimensions = new Vector3(2.2f, 1.9f, 1.1f);

        public void Awake()
        {
            hexagonRenderer = GetComponent<Renderer>();

            var outline = transform.Find("Outline");
            outlineRenderer = outline.GetComponent<Renderer>();

            SetColor(hexagonRenderer, Color.white);
            SetColor(outlineRenderer, Color.black);
        }

        public override Vector3 GetCellDimensions()
        {
            return dimensions;
        }

        public override void MarkAsReachable()
        {
            SetColor(hexagonRenderer, Color.yellow);
        }
        public override void MarkAsPath()
        {
            SetColor(hexagonRenderer, Color.green); ;
        }
        public override void MarkAsHighlighted()
        {
            SetColor(outlineRenderer, Color.blue);
        }
        public override void UnMark()
        {
            SetColor(hexagonRenderer, Color.white);
            SetColor(outlineRenderer, Color.black);
        }

        private void SetColor(Renderer renderer, Color color)
        {
            renderer.material.color = color;
        }

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
    }
}
