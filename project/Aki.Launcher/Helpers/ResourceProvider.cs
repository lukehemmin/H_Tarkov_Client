/* ResourceProvider.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */


using System;
using System.IO;

namespace Aki.Launcher.Helpers
{
    //Only really do it this way incase we want to extend this later. No idea why we would want to, but who knows *shrug.
    public static class ResourceProvider
    {
        public static string DefaultImagesFolderPath = Path.Join(Environment.CurrentDirectory, "Aki_Data", "Launcher", "Images");
        public static string BackgroundImagePath { get; } = Path.Join(DefaultImagesFolderPath, "bg.png");
    }
}
