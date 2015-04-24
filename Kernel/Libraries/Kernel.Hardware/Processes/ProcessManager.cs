﻿#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
#define PROCESSMANAGER_TRACE
#undef PROCESSMANAGER_TRACE

#define PROCESSMANAGER_SWITCH_TRACE
#undef PROCESSMANAGER_SWITCH_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes.Synchronisation;

namespace Kernel.Hardware.Processes
{
    public static unsafe class ProcessManager
    {
        public static List Processes = new List();
        public static Process CurrentProcess = null;
        public static Thread CurrentThread = null;
        public static ThreadState* CurrentThread_State = null;

        private static uint ProcessIdGenerator = 1;

        public static Process CreateProcess(ThreadStartMethod MainMethod, FOS_System.String Name, bool UserMode)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Creating process object...");
#endif
            return new Process(MainMethod, ProcessIdGenerator++, Name, UserMode);
        }
        public static void RegisterProcess(Process process, Scheduler.Priority priority)
        {
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Registering process...");
            BasicConsole.WriteLine("Disabling scheduler...");
#endif
            if (process == null)
            {
                return;
            }

            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}
#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Initialising process...");
#endif
            Scheduler.InitProcess(process, priority);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Adding process...");
#endif

            Processes.Add(process);

#if PROCESSMANAGER_TRACE
            BasicConsole.WriteLine("Enabling scheduler...");
#endif
            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
        }

        /// <remarks>
        /// Specifying threadId=-1 accepts any thread from the specified process.
        /// No guarantees are made about the thread chosen. This is used when you
        /// mainly want to switch process context and don't care about the specific
        /// thread context e.g. during an interrupt.
        /// </remarks>
        public static void SwitchProcess(uint processId, int threadId)
        {
            //Switch the current memory layout across.
            //  Don't touch register state etc, just the memory layout

            bool dontSwitchOutIn = false;

            if (CurrentProcess != null &&
                CurrentProcess.Id == processId)
            {
                if (CurrentThread != null &&
                    (CurrentThread.Id == threadId || threadId == -1))
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("No switch. (1)");
#endif
                    return;
                }
                else
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("No switch. (2)");
#endif
                    dontSwitchOutIn = true;
                }
            }

            if (!dontSwitchOutIn)
            {
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.WriteLine("Switching out: " + CurrentProcess.Name);
#endif
                CurrentProcess.UnloadMemLayout();

                CurrentProcess = null;
                
                for (int i = 0; i < Processes.Count; i++)
                {
                    if (((Process)Processes[i]).Id == processId)
                    {
                        CurrentProcess = ((Process)Processes[i]);
                        break;
                    }
                }

                // Process not found
                if (CurrentProcess == null)
                {
#if PROCESSMANAGER_SWITCH_TRACE
                    BasicConsole.WriteLine("Process not found.");
#endif
                    return;
                }
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.WriteLine("Process found. " + CurrentProcess.Name);
#endif
            }

            CurrentThread = null;
            CurrentThread_State = null;

            if (threadId == -1)
            {
                if (CurrentProcess.Threads.Count > 0)
                {
                    CurrentThread = (Thread)CurrentProcess.Threads[0];
                }
            }
            else
            {
                for (int i = 0; i < CurrentProcess.Threads.Count; i++)
                {
                    if (((Thread)CurrentProcess.Threads[i]).Id == threadId)
                    {
                        CurrentThread = (Thread)CurrentProcess.Threads[i];
                        break;
                    }
                }
            }

            // No threads in the process (?!) or process not found
            if (CurrentThread == null)
            {
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.WriteLine("Thread not found.");
#endif
                return;
            }
#if PROCESSMANAGER_SWITCH_TRACE
            BasicConsole.WriteLine("Thread found.");
#endif

            CurrentThread_State = CurrentThread.State;
#if PROCESSMANAGER_SWITCH_TRACE
            BasicConsole.WriteLine("Thread state updated.");
#endif
            
            if (!dontSwitchOutIn)
            {
#if PROCESSMANAGER_SWITCH_TRACE
                BasicConsole.WriteLine("Switching in: " + CurrentProcess.Name);
#endif
                CurrentProcess.LoadMemLayout();
            }
        }
    }
}
