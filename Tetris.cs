//===============================================================================
// Copyright (c) Serhiy Perevoznyk.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Security.Permissions;
using System.Threading;
using System.Runtime.InteropServices;

namespace Karna.Windows.UI
{

    /// <summary>
    /// The classic Tetris game control
    /// </summary>
    [Docking(DockingBehavior.Never)]
    [ToolboxItem(true)]
    [Description("Tetris game")]
    [Designer(typeof(Karna.Windows.UI.Design.TetrisDesigner))]
    public class Tetris : Control
    {
        private Object thisLock = new Object();
        private int interval;

        private const int glassWidth = 10;
        private const int glassHeight = 23;
        private const int barWidth = 14;
        private const int barHeight = 14;

        private const int maxFigureNumber = 7;
        private const int maxCornerNumber = 4;
        private const int maxFigureSize = 4;
        private const int maxFigureColor = 7;

        #region Figures

        private static byte[,] Triada =
         {{0,1,0,0},
          {1,1,1,0},
          {0,0,0,0},
          {0,0,0,0}};

        private static byte[,] LCorner =
        {{1,1,1,0},
         {1,0,0,0},
         {0,0,0,0},
         {0,0,0,0}};

        private static byte[,] RCorner =
        {{1,1,1,0},
         {0,0,1,0},
         {0,0,0,0},
         {0,0,0,0}};

        private static byte[,] LZigzag =
        {{1,1,0,0},
         {0,1,1,0},
         {0,0,0,0},
         {0,0,0,0}};

        private static byte[,] RZigzag =
        {{0,1,1,0},
         {1,1,0,0},
         {0,0,0,0},
         {0,0,0,0}};

        private static byte[,] Stick =
        {{1,1,1,1},
         {0,0,0,0},
         {0,0,0,0},
         {0,0,0,0}};

        private static byte[,] Box =
        {{1,1,0,0},
         {1,1,0,0},
         {0,0,0,0},
         {0,0,0,0}};

        #endregion

        private byte[,] glassWorkSheet = new byte[glassHeight, glassWidth];
        private byte[,] oldGlassWorkSheet = new byte[glassHeight, glassWidth];

        private byte[,] FigureWorkSheet =
        {{0,0,0,0},
         {0,0,0,0},
         {0,0,0,0},
         {0,0,0,0}};

        private Color color1;
        private Color color2;
        private Color color3;
        private Color color4;
        private Color color5;
        private Color color6;
        private Color color7;
        private Color color8;

        private byte level;
        private int score;

        private bool FigureActive;
        private byte FigureType;
        private byte FigureX;
        private byte FigureY;
        private FigureCorner FigureCorner;
        private byte FirstColor;
        private byte SecondColor;
        private byte FirstFigure;
        private byte SecondFigure;
        private bool RedrawSheet;

        private bool active;
        private bool canRotate = true;

        /// <summary>
        /// Copy one array to another array
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        protected static void ArrayCopyTo(byte[,] source, byte[,] destination)
        {
            int xu;
            int yu;
            xu = source.GetUpperBound(0) + 1;
            yu = source.GetUpperBound(1) + 1;

            for (int i = 0; i < xu; i++)
            {
                for (int j = 0; j < yu; j++)
                {
                    destination[i, j] = source[i, j];
                }
            }
        }

        protected byte FigureXSize
        {
            get
            {
                byte k = 0;
                for (int j = 0; j < maxFigureSize; j++)
                    for (int i = 0; i < maxFigureSize; i++)
                        if (FigureWorkSheet[j, i] > 0)
                            if (k < i)
                                k = (byte)i;
                k++;
                return k;
            }

        }


        protected byte FigureYSize
        {
            get
            {
                byte k = 0;
                for (int j = 0; j < maxFigureSize; j++)
                    for (int i = 0; i < maxFigureSize; i++)
                        if (FigureWorkSheet[j, i] > 0)
                            if (k < j) k = (byte)j;
                k++;
                return k;

            }
        }


        internal void StartTimer(int speed)
        {
            if (this.Handle != IntPtr.Zero)
                NativeMethods.SetTimer(new HandleRef(this, this.Handle), new HandleRef(this, (IntPtr)1), speed, new HandleRef(this, IntPtr.Zero));
        }

        internal void StopTimer()
        {
            if (this.Handle != IntPtr.Zero)
                NativeMethods.KillTimer(new HandleRef(this, this.Handle), new HandleRef(null, (IntPtr)1));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics Canvas = e.Graphics;
            int X1, Y1, X2, Y2;
            Rectangle NewRect;
            SolidBrush brush;

            if (RedrawSheet)
                ClearOldGlass();

            for (int i = 0; i < glassHeight; i++)
                for (int j = 0; j < glassWidth; j++)
                {
                    if (glassWorkSheet[i, j] == oldGlassWorkSheet[i, j])
                        continue;

                    X1 = j * barWidth;
                    X2 = X1 + barWidth;
                    Y1 = i * barHeight;
                    Y2 = Y1 + barHeight;
                    switch (glassWorkSheet[i, j])
                    {
                        case 1: brush = new SolidBrush(Color1);
                            break;
                        case 2: brush = new SolidBrush(Color2);
                            break;
                        case 3: brush = new SolidBrush(Color3);
                            break;
                        case 4: brush = new SolidBrush(Color4);
                            break;
                        case 5: brush = new SolidBrush(Color5);
                            break;
                        case 6: brush = new SolidBrush(Color6);
                            break;
                        case 7: brush = new SolidBrush(Color7);
                            break;
                        case 8: brush = new SolidBrush(Color8);
                            break;
                        default:
                            brush = new SolidBrush(Color.Black);
                            break;
                    }

                    if (glassWorkSheet[i, j] > 0)
                    {
                        NewRect = new Rectangle(X1, Y1, X2 - X1, Y2 - Y1);
                        Canvas.FillRectangle(brush, NewRect);

                        Canvas.DrawLine(Pens.Gray, new Point(X1, Y1), new Point(X1, Y2 - 1));
                        Canvas.DrawLine(Pens.Gray, new Point(X1, Y2 - 1), new Point(X2 - 1, Y2 - 1));
                        Canvas.DrawLine(Pens.White, new Point(X2 - 1, Y2 - 1), new Point(X2 - 1, Y1));
                        Canvas.DrawLine(Pens.White, new Point(X2 - 1, Y1), new Point(X1, Y1));
                    }

                    if (brush != null)
                    {
                        brush.Dispose();
                        brush = null;
                    }

                }

            ArrayCopyTo(glassWorkSheet, oldGlassWorkSheet);
        }

        protected void GenerateNewFigure()
        {
            Random rnd = new Random();
            StopTimer();
            SecondFigure = FirstFigure;
            if (SecondColor == FirstColor)
                SecondColor = (byte)(rnd.Next(maxFigureColor) + 1);
            else
                SecondColor = FirstColor;

            FigureType = SecondFigure;
            FigureX = 5;
            FigureY = 0;
            FigureCorner = FigureCorner.Corner270;
            Array.Clear(FigureWorkSheet, 0, FigureWorkSheet.Length);

            switch (FigureType)
            {
                case 0:
                    ArrayCopyTo(Triada, FigureWorkSheet);
                    break;
                case 1:
                    ArrayCopyTo(LCorner, FigureWorkSheet);
                    break;
                case 2:
                    ArrayCopyTo(RCorner, FigureWorkSheet);
                    break;
                case 3:
                    ArrayCopyTo(LZigzag, FigureWorkSheet);
                    break;
                case 4:
                    ArrayCopyTo(RZigzag, FigureWorkSheet);
                    break;
                case 5:
                    ArrayCopyTo(Stick, FigureWorkSheet);
                    break;
                case 6:
                    ArrayCopyTo(Box, FigureWorkSheet);
                    break;
                default: break;
            }

            SetFigureColor();
            FirstFigure = (byte)rnd.Next(maxFigureNumber);
            FirstColor = (byte)(rnd.Next(maxFigureColor) + 1);
            StartTimer(interval);
        }


        protected void ClearFigureIntoGlass()
        {
            for (int j = 0; j < FigureYSize; j++)
                for (int i = 0; i < FigureXSize; i++)
                    if (FigureWorkSheet[j, i] > 0)
                    {
                        if ((FigureX + i < glassWidth) && (FigureY + j < glassHeight))
                            glassWorkSheet[FigureY + j, FigureX + i] = 0;
                    }
        }

        protected void ScanFillLines()
        {
            lock (thisLock)
            {
                int k = 0;
                ClearFigureIntoGlass();
                for (int i = 0; i < glassHeight; i++)
                {
                    k = 0;
                    for (int j = 0; j < glassWidth; j++)
                        if (glassWorkSheet[i, j] > 0)
                            k++;

                    if (k == glassWidth - 1)
                    {
                        for (int l = i; l >= 0; l--)
                            for (int j = 0; j < glassWidth; j++)
                                if (l > 0)
                                    glassWorkSheet[l, j] = glassWorkSheet[l - 1, j];
                    }
                }
                PutFigureIntoGlass(MoveDirection.Down);
            }
        }

        protected void SetFigureColor()
        {
            for (int i = 0; i < maxFigureSize; i++)
                for (int j = 0; j < maxFigureSize; j++)
                    if (FigureWorkSheet[i, j] > 0)
                        FigureWorkSheet[i, j] = SecondColor;
        }

        /// <summary>
        /// Tries to rotate the current figure. If figure is connected with any other figure at the bottom
        /// then rotation fails
        /// </summary>
        /// <returns>True, if figure can be rotated, otherwise returns false</returns>
        protected bool TryRotate()
        {
            if (FigureY + FigureYSize >= glassHeight - 1)
            {
                return false;
            }

            for (int y = 0; y < FigureYSize; y++)
            {
                for (int x = 0; x < FigureXSize; x++)
                {
                    if ((FigureWorkSheet[y, x] > 0) && (glassWorkSheet[FigureY + y + 1, FigureX + x] > 0))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected bool PutFigureIntoGlass(MoveDirection moveDirection)
        {
            bool result = true;

            if ((FigureY + FigureYSize >= glassHeight) && (moveDirection == MoveDirection.Down))
            {
                FigureY--;
                result = false;
                return result;
            }
            else
            {
                while ((FigureX + FigureXSize >= glassWidth) && (moveDirection == MoveDirection.Down))
                    FigureX--;

                for (int j = 0; j < FigureYSize; j++)
                {
                    for (int i = 0; i < FigureXSize; i++)
                    {
                        if ((FigureWorkSheet[j, i] > 0) && (glassWorkSheet[FigureY + j, FigureX + i] > 0))
                        {
                            result = false;
                            switch (moveDirection)
                            {
                                case MoveDirection.Down:
                                    FigureY--;
                                    break;
                                case MoveDirection.Right:
                                    FigureX--;
                                    break;
                                case MoveDirection.Left:
                                    FigureX++;
                                    break;
                                default:
                                    break;
                            }

                            return result;
                        }
                    }
                }

                for (int j = 0; j < FigureYSize; j++)
                {
                    for (int i = 0; i < FigureXSize; i++)
                    {
                        if (FigureWorkSheet[j, i] > 0)
                        {
                            glassWorkSheet[FigureY + j, FigureX + i] = FigureWorkSheet[j, i];
                        }
                    }
                }

                RedrawSheet = false;
                Invalidate();
                RedrawSheet = true;

                return result;
            }
        }

        protected void RotateFigureWorksheet(byte[,] currentSheet)
        {
            byte VertFlag;
            byte HorizFlag;

            Array.Clear(FigureWorkSheet, 0, FigureWorkSheet.Length);

            switch (FigureType)
            {
                case 0:
                    ArrayCopyTo(Triada, FigureWorkSheet);
                    break;
                case 1:
                    ArrayCopyTo(LCorner, FigureWorkSheet);
                    break;
                case 2:
                    ArrayCopyTo(RCorner, FigureWorkSheet);
                    break;
                case 3:
                    ArrayCopyTo(LZigzag, FigureWorkSheet);
                    break;
                case 4:
                    ArrayCopyTo(RZigzag, FigureWorkSheet);
                    break;
                case 5:
                    ArrayCopyTo(Stick, FigureWorkSheet);
                    break;
                case 6:
                    ArrayCopyTo(Box, FigureWorkSheet);
                    break;
                default: break;
            }

            Array.Clear(currentSheet, 0, currentSheet.Length);

            for (int k = 0; k <= (int)FigureCorner; k++)
            {
                for (int i = 0; i < maxFigureSize; i++)
                    for (int j = 0; j < maxFigureSize; j++)
                        currentSheet[j, i] = FigureWorkSheet[maxFigureSize - i - 1, j];
                ArrayCopyTo(currentSheet, FigureWorkSheet);
            }

            SetFigureColor();
            HorizFlag = 0;
            while (HorizFlag == 0)
            {
                for (int i = 0; i < maxFigureSize; i++)
                    if (FigureWorkSheet[0, i] > 0)
                        HorizFlag = 1;
                if (HorizFlag == 0)
                {
                    for (int j = 0; j < maxFigureSize - 1; j++)
                        for (int i = 0; i < maxFigureSize; i++)
                            FigureWorkSheet[j, i] = FigureWorkSheet[j + 1, i];
                    for (int j = 0; j < maxFigureSize; j++)
                        FigureWorkSheet[maxFigureSize - 1, j] = 0;
                }
            }

            VertFlag = 0;
            while (VertFlag == 0)
            {
                for (int j = 0; j < maxFigureSize; j++)
                    if (FigureWorkSheet[j, 0] > 0)
                        VertFlag = 1;
                if (VertFlag == 0)
                {
                    for (int j = 0; j < maxFigureSize; j++)
                        for (int i = 0; i < maxFigureSize - 1; i++)
                            FigureWorkSheet[j, i] = FigureWorkSheet[j, i + 1];
                    for (int j = 0; j < maxFigureSize; j++)
                        FigureWorkSheet[j, maxFigureSize - 1] = 0;
                }
            }

        }

        protected void RotateFigure()
        {
            FigureCorner OldFigureCorner;
            byte[,] CurSheet = new byte[maxFigureSize, maxFigureSize];

            ClearFigureIntoGlass();

            if (TryRotate())
            {
                OldFigureCorner = FigureCorner;

                if (FigureCorner > FigureCorner.Corner00)
                    FigureCorner--;
                else
                    FigureCorner = FigureCorner.Corner270;

                RotateFigureWorksheet(CurSheet);

                if (!PutFigureIntoGlass(MoveDirection.Down))
                {
                    FigureCorner = OldFigureCorner;
                    RotateFigureWorksheet(CurSheet);
                    PutFigureIntoGlass(MoveDirection.Down);
                }
            }
            else
            {
                PutFigureIntoGlass(MoveDirection.Down);
            }

        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Tetris"/> game is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Active
        {
            get { return active; }
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void Start()
        {
            StopTimer();
            canRotate = true;
            ClearFigureIntoGlass();
            FigureActive = false;
            level = 1;
            score = 0;

            Array.Clear(glassWorkSheet, 0, glassWorkSheet.Length);

            RedrawSheet = false;
            Invalidate();
            RedrawSheet = true;

            active = true;

            StartTimer(interval);
        }

        /// <summary>
        /// Stops the game
        /// </summary>
        public void Stop()
        {
            StopTimer();
            active = false;
        }

        /// <summary>
        /// Moves the current figure to the left.
        /// </summary>
        public void MoveLeft()
        {
            lock (thisLock)
            {
                if (FigureX > 0)
                {
                    ClearFigureIntoGlass();
                    FigureX--;
                    if (!PutFigureIntoGlass(MoveDirection.Left))
                        PutFigureIntoGlass(MoveDirection.Left);
                }
            }
        }

        /// <summary>
        /// Moves the current figure to the right.
        /// </summary>
        public void MoveRight()
        {
            lock (thisLock)
            {
                if (FigureX + FigureXSize < glassWidth - 1)
                {
                    ClearFigureIntoGlass();
                    FigureX++;
                    if (!PutFigureIntoGlass(MoveDirection.Right))
                        PutFigureIntoGlass(MoveDirection.Right);
                }
            }
        }

        /// <summary>
        /// Moves down the current figure
        /// </summary>
        public void MoveDown()
        {
            lock (thisLock)
            {
                StopTimer();
                ClearFigureIntoGlass();
                FigureY++;
                PutFigureIntoGlass(MoveDirection.Down);
                interval = 10;
                StartTimer(interval);
                score += 1;
            }
        }

        /// <summary>
        /// Rotates the current figure
        /// </summary>
        public void Rotate()
        {
            lock (thisLock)
            {
                RotateFigure();
            }
        }

        [DefaultValue(1)]
        [Description("The game level")]
        [Browsable(false)]
        public byte Level
        {
            get { return level; }
        }

        [DefaultValue(0)]
        [Description("The score of the player")]
        [Browsable(false)]
        public int Score
        {
            get { return score; }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Silver")]
        public Color Color1
        {
            get { return color1; }
            set { color1 = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Red")]
        public Color Color2
        {
            get { return color2; }
            set { color2 = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Lime")]
        public Color Color3
        {
            get { return color3; }
            set { color3 = value; Invalidate(); }
        }

        [DefaultValue(typeof(Color), "Blue")]
        [Category("Appearance")]
        public Color Color4
        {
            get { return color4; }
            set { color4 = value; Invalidate(); }
        }

        [DefaultValue(typeof(Color), "Fuchsia")]
        [Category("Appearance")]
        public Color Color5
        {
            get { return color5; }
            set { color5 = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Aqua")]
        public Color Color6
        {
            get { return color6; }
            set { color6 = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Yellow")]
        public Color Color7
        {
            get { return color7; }
            set { color7 = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color Color8
        {
            get { return color8; }
            set { color8 = value; Invalidate(); }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (active)
                NativeMethods.PostMessage(this.Handle, NativeMethods.CM_ROTATE, IntPtr.Zero, IntPtr.Zero);

        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (active)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.X < (Width / 2))
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_LEFT, IntPtr.Zero, IntPtr.Zero);
                    else
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_RIGHT, IntPtr.Zero, IntPtr.Zero);
                }
                else
                    if (e.Button == MouseButtons.Middle)
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_DOWN, IntPtr.Zero, IntPtr.Zero);

            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> and its child controls and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            StopTimer();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tetris"/> class.
        /// </summary>
        public Tetris()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.FixedHeight, true);
            SetStyle(ControlStyles.FixedWidth, true);
            SetStyle(ControlStyles.UserMouse, true);

            base.TabStop = true;

            Random rnd = new Random();

            color1 = Color.Silver;
            color2 = Color.Red;
            color3 = Color.Lime;
            color4 = Color.Blue;
            color5 = Color.Fuchsia;
            color6 = Color.Aqua;
            color7 = Color.Yellow;
            color8 = Color.White;

            Width = (glassWidth - 1) * barWidth;
            Height = (glassHeight - 1) * barHeight;
            BackColor = Color.Navy;

            ClearOldGlass();

            RedrawSheet = true;
            level = 1;
            interval = 650;

            FirstFigure = (byte)rnd.Next(maxFigureNumber);
            FirstColor = (byte)(rnd.Next(maxFigureColor) + 1);

        }

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left"/> property value of the control.</param>
        /// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top"/> property value of the control.</param>
        /// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width"/> property value of the control.</param>
        /// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height"/> property value of the control.</param>
        /// <param name="specified">A bitwise combination of the <see cref="T:System.Windows.Forms.BoundsSpecified"/> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, (glassWidth - 1) * barWidth, (glassHeight - 1) * barHeight, specified);
        }

        private void ClearOldGlass()
        {
            for (int i = 0; i < glassHeight; i++)
                for (int j = 0; j < glassWidth; j++)
                    oldGlassWorkSheet[i, j] = 255;

        }

        /// <summary>
        /// Occurs when the game is over.
        /// </summary>
        [Description("Occurs when the game is over")]
        public event EventHandler GameOver;

        /// <summary>
        /// Occurs when the score and the game level changes
        /// </summary>
        [Description("Occurs when the score and the game level changes")]
        public event EventHandler Change;

        /// <summary>
        /// Raises the <see cref="E:GameOver"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnGameOver(EventArgs e)
        {
            if (GameOver != null)
                GameOver(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Change"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnChange(EventArgs e)
        {
            if (Change != null)
                Change(this, e);
        }


        private void CMRotate()
        {
            if (canRotate)
                Rotate();
            canRotate = false;
        }

        private void CMDown()
        {
            MoveDown();
            canRotate = true;
        }

        private void CMLeft()
        {
            MoveLeft();
            canRotate = true;
        }

        private void CMRight()
        {
            MoveRight();
            canRotate = true;
        }

        protected void ProcessCommandKeys(Keys keys)
        {
            lock (thisLock)
            {

                switch (keys)
                {
                    case Keys.Up:
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_ROTATE, IntPtr.Zero, IntPtr.Zero);
                        break;
                    case Keys.Down:
                    case Keys.Space:
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_DOWN, IntPtr.Zero, IntPtr.Zero);
                        break;
                    case Keys.Left:
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_LEFT, IntPtr.Zero, IntPtr.Zero);
                        break;
                    case Keys.Right:
                        NativeMethods.PostMessage(this.Handle, NativeMethods.CM_RIGHT, IntPtr.Zero, IntPtr.Zero);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the window message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>
        /// true if the character was processed by the control; otherwise, false.
        /// </returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            bool turn = false;

            if (!active)
                return base.ProcessCmdKey(ref msg, keyData);
            else
            {
                if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
                {
                    switch (keyData)
                    {
                        case Keys.Down:
                        case Keys.Up:
                        case Keys.Left:
                        case Keys.Right:
                            turn = true;
                            ProcessCommandKeys(keyData);
                            break;
                    }
                }


                if (!turn)
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }
                else
                    return true;
            }
        }

        private static void WmGetDlgCode(ref Message m)
        {
            m.Result = (IntPtr)DialogCodes.DLGC_WANTALLKEYS;
        }

        private void ProcessTimer()
        {
            lock (thisLock)
            {
                if (!FigureActive)
                {
                    GenerateNewFigure();

                    if (!PutFigureIntoGlass(MoveDirection.Down))
                    {
                        StopTimer();
                        active = false;
                        OnGameOver(EventArgs.Empty);
                        ClearOldGlass();
                        ClearFigureIntoGlass();
                        FigureActive = false;
                        level = 1;
                        score = 0;

                        Array.Clear(glassWorkSheet, 0, glassWorkSheet.Length);

                        RedrawSheet = false;
                        Invalidate();
                        RedrawSheet = true;
                    }
                    FigureActive = true;
                }
                else
                {
                    ClearFigureIntoGlass();
                    FigureY++;

                    if (!PutFigureIntoGlass(MoveDirection.Down))
                    {
                        switch (FigureType)
                        {
                            case 0: score += 10;
                                break;
                            case 1: score += 30;
                                break;
                            case 2: score += 30;
                                break;
                            case 3: score += 25;
                                break;
                            case 4: score += 25;
                                break;
                            case 5: score += 15;
                                break;
                            case 6: score += 20;
                                break;
                        }

                        if (Score <= 300)
                        {
                            interval = 650;
                        }

                        if (Score > 300)
                        {
                            level = 2;
                            interval = 500;
                        }

                        if (Score > 700)
                        {
                            level = 3;
                            interval = 400;
                        }

                        if (Score > 1300)
                        {
                            level = 4;
                            interval = 350;
                        }

                        if (Score > 2000)
                        {
                            level = 5;
                            interval = 300;
                        }

                        if (Score > 3000)
                        {
                            level = 6;
                            interval = 200;
                        }

                        if (Score > 5000)
                        {
                            level = 7;
                            interval = 150;
                        }

                        FigureActive = false;
                    }

                }
                ScanFillLines();
                OnChange(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            const int WM_GETDLGCODE = 0x0087;
            const int WM_KEYUP = 0x0101;
            const int WM_SYSKEYUP = 0x0105;

            switch (m.Msg)
            {
                case NativeMethods.WM_TIMER:
                    NativeMethods.PostMessage(this.Handle, NativeMethods.CM_TIMER, IntPtr.Zero, IntPtr.Zero);
                    return;
                case WM_GETDLGCODE:
                    WmGetDlgCode(ref m);
                    return;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    canRotate = true;
                    break;
                case NativeMethods.CM_RIGHT:
                    CMRight();
                    break;
                case NativeMethods.CM_LEFT:
                    CMLeft();
                    break;
                case NativeMethods.CM_DOWN:
                    CMDown();
                    break;
                case NativeMethods.CM_ROTATE:
                    CMRotate();
                    break;
                case NativeMethods.CM_TIMER:
                    ProcessTimer();
                    break;
                default:
                    break;
            }


            base.WndProc(ref m);
        }
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!active)
                return;
            ProcessCommandKeys(e.KeyData);

        }

    }
}
