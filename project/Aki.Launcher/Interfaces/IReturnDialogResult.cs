/* IReturnDialogResult.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using System;

namespace Aki.Launcher.Interfaces
{
    public interface IReturnDialogResult
    {
        /// <summary>
        /// Event handler the dialog host will subscribe to. The dialog host will close once it has the results.
        /// </summary>
        public event EventHandler<object> ResultsReady;
    }
}
