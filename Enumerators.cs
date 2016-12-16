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

namespace Karna.Windows.UI
{
    /// <summary>
    /// Tetris figure move direction
    /// </summary>
    public enum MoveDirection
    {
        /// <summary>
        /// Move the figure down
        /// </summary>
        Down,
        /// <summary>
        /// Move the figure to the left
        /// </summary>
        Left,
        /// <summary>
        /// Move the figure to the right
        /// </summary>
        Right
    };

    public enum FigureCorner
    {
        Corner00,
        Corner90,
        Corner180,
        Corner270
    }

    internal enum DialogCodes : int
    {
        DLGC_WANTARROWS = 1,
        DLGC_WANTTAB = 2,
        DLGC_WANTALLKEYS = 4,
        DLGC_WANTMESSAGE = 4,
        DLGC_HASSETSEL = 8,
        DLGC_DEFPUSHBUTTON = 0x10,
        DLGC_UNDEFPUSHBUTTON = 0x20,
        DLGC_RADIOBUTTON = 0x40,
        DLGC_WANTCHARS = 0x80,
        DLGC_STATIC = 0x100,
        DLGC_BUTTON = 0x2000
    }

}
