﻿#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents an IO exception.
    /// </summary>
    public class IOException : FOS_System.Exception
    {
        /// <summary>
        /// Initializes a new IO exception.
        /// </summary>
        /// <param name="aMessage">The IO exception message.</param>
        public IOException(string aMessage)
            : base(aMessage)
        {
        }
    }
}