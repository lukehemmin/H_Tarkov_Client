/* IUpdateSubProgress.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using Aki.Launcher.Models.Launcher;
using System;

namespace Aki.Launcher.Interfaces
{
    public interface IUpdateSubProgress
    {
        /// <summary>
        /// The <see cref="Custom_Controls.Dialogs.ProgressDialog"/> will subscribe to this event to update it's sub progress bar (bottom progress bar)
        /// </summary>
        /// <remarks>The sub progress bar is not visible if a class does not implement this interface</remarks>
        public event EventHandler<ProgressInfo> SubProgressChanged;
    }
}
