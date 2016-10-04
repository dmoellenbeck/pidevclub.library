// Copyright 2016 OSIsoft, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;

namespace PIDevClub.Library.WinForm
{
    public static class FormExtensions
    {
        /// <summary>
        /// Changes the style of a <see cref="Font"/>.  Useful for bolding or unbolding.  If the <see cref="FontFamily"/> does not support the
        /// requested <see cref="FontStyle"/>, the original <see cref="Font"/> is returned as is.
        /// </summary>
        /// <param name="font">A System.Drawing.Font</param>
        /// <param name="style">A System.Drawing.FontStyle</param>
        /// <returns>Font with the requested style, if possible.  Otherwise the orignal font is returned.</returns>
        public static Font WithStyle(this Font font, FontStyle style)
        {
            if (!font.FontFamily.IsStyleAvailable(style)) return font;
            return new Font(font, style);
        }

        /// <summary>
        /// Returns a modified <see cref="DataGridViewCellStyle"/> with the requested background color, foreground color, and font bolding.
        /// If the <see cref="FontFamily"/> does not support bolding, the original font is returned as is.
        /// </summary>
        /// <param name="cellStyle"></param>
        /// <param name="backcolor"></param>
        /// <param name="forecolor"></param>
        /// <param name="bolded"></param>
        /// <returns></returns>
        public static DataGridViewCellStyle ModifyCellStyle(this DataGridViewCellStyle cellStyle, Color backcolor, Color forecolor, bool bolded)
        {
            return ModifyCellStyle(cellStyle, backcolor, forecolor, bolded ? FontStyle.Bold : FontStyle.Regular);
        }

        /// <summary>
        /// Returns a modified <see cref="DataGridViewCellStyle"/> with the requested background color, foreground color, and <see cref="FontStyle"/>.
        /// If the <see cref="FontFamily"/> does not support the requested <see cref="FontStyle"/>, the original font is returned as is.
        /// </summary>
        /// <param name="cellStyle"></param>
        /// <param name="backcolor"></param>
        /// <param name="forecolor"></param>
        /// <param name="bolded"></param>
        /// <returns></returns>
        public static DataGridViewCellStyle ModifyCellStyle(this DataGridViewCellStyle cellStyle, Color backcolor, Color forecolor, FontStyle fontStyle)
        {
            cellStyle.BackColor = backcolor;
            cellStyle.ForeColor = forecolor;
            cellStyle.Font = cellStyle.Font.WithStyle(fontStyle);
            return cellStyle;
        }

        /// <summary>
        /// A sample method of drawing row numbers in a <see cref="DataGridView"/>'s row header.
        /// Intended to be called from the <see cref="RowPostPaint"/> event handler.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="e"></param>
        public static void PostPaintRowNumber(this DataGridView grid, DataGridViewRowPostPaintEventArgs e)
        {
            // How to show row number in the row header
            // http://stackoverflow.com/questions/9581626/show-row-number-in-row-header-of-a-datagridview

            if (grid == null || e == null || e.RowIndex < 0)
                return;
            
            var id = (e.RowIndex + 1).ToString("N0");
            var font = new Font("Tahoma", 8.0f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

            var centerFormat = new StringFormat();
            centerFormat.Alignment = StringAlignment.Far;
            centerFormat.LineAlignment = StringAlignment.Center;

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth - 4, e.RowBounds.Height);

            e.Graphics.DrawString(id, font, SystemBrushes.InactiveCaptionText, headerBounds, centerFormat);
        }
    }

}
