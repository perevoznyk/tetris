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
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Karna.Windows.UI.Design
{
    /// <summary>
    /// Designer for Tetris control
    /// </summary>
    internal class TetrisDesigner : ControlDesigner
    {
        public override SelectionRules SelectionRules
        {
            get
            {
                SelectionRules rules = SelectionRules.Moveable | SelectionRules.Visible;
                return rules;
            }
        }

        private void DrawBorder(Graphics graphics)
        {
            Control component = (Control)base.Component;
            if ((component != null) && component.Visible)
            {
                Pen borderPen = this.BorderPen;
                Rectangle clientRectangle = this.Control.ClientRectangle;
                clientRectangle.Width--;
                clientRectangle.Height--;
                graphics.DrawRectangle(borderPen, clientRectangle);
                borderPen.Dispose();
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            this.DrawBorder(pe.Graphics);
            base.OnPaintAdornments(pe);
        }

        // Properties
        private Pen BorderPen
        {
            get
            {
                Color color = (this.Control.BackColor.GetBrightness() < 0.5) ? ControlPaint.Light(this.Control.BackColor) : ControlPaint.Dark(this.Control.BackColor);
                Pen pen = new Pen(color);
                pen.DashStyle = DashStyle.Dash;
                return pen;
            }
        }

        protected override void PostFilterProperties(System.Collections.IDictionary properties)
        {
            base.PostFilterProperties(properties);

            properties.Remove("MaximumSize");
            properties.Remove("MinimumSize");
            properties.Remove("Size");
            properties.Remove("Dock");
            properties.Remove("RightToLeft");
            properties.Remove("Text");
            properties.Remove("ImeMode");
            properties.Remove("Font");
            properties.Remove("ForeColor");
        }

        protected override void PostFilterEvents(System.Collections.IDictionary events)
        {
            base.PostFilterEvents(events);

            events.Remove("ImeModeChanged");
            events.Remove("TextChanged");
            events.Remove("Resize");
            events.Remove("ClientSizeChanged");
            events.Remove("FontChanged");
            events.Remove("BackColorChanged");
            events.Remove("RightToLeftChanged");
        }
    }
}
