﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vim;
using System.Windows.Input;
using EnvDTE;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition;
using System.Windows;

namespace VsVim
{
    /// <summary>
    /// Responsible for dealing with the conflicting key bindings inside of Visual Studio
    /// </summary>
    [Export(typeof(KeyBindingService))]
    public sealed class KeyBindingService
    {
        private bool _hasChecked;

        public void OneTimeCheckForConflictingKeyBindings(_DTE dte, IVimBuffer buffer)
        {
            if (dte == null)
            {
                throw new ArgumentNullException("dte");
            }

            if (_hasChecked)
            {
                return;
            }
            _hasChecked = true;
            CheckForConflictingKeyBindings(dte, buffer);
        }

        /// <summary>
        /// Check for and remove conflicting key bindings
        /// </summary>
        private void CheckForConflictingKeyBindings(_DTE dte, IVimBuffer buffer)
        {
            var hashSet = new HashSet<KeyInput>(
                buffer.AllModes.Select(x => x.Commands).SelectMany(x => x));
            hashSet.Add(buffer.Settings.GlobalSettings.DisableCommand);
            var commands = dte.Commands.GetCommands();
            var list = FindConflictingCommands(commands, hashSet);
            if (list.Count > 0)
            {
                var msg = new StringBuilder();
                msg.AppendLine("Conflicting key bindings found.  Remove?");
                foreach (var item in list)
                {
                    const int maxLen = 50;
                    var name = item.Name.Length > maxLen ? item.Name.Substring(0, maxLen) + "..." : item.Name;
                    msg.AppendFormat("\t{0}", name);
                    msg.AppendLine();
                }

                var res = MessageBox.Show(
                    caption: "VsVim: Remove Conflicting Key Bindings",
                    messageBoxText: msg.ToString(),
                    button: MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    list.ForEach(x => x.SafeResetBindings());
                }
            }
        }

        /// <summary>
        /// Find all of the Command instances which have conflicting key bindings
        /// </summary>
        public static List<Command> FindConflictingCommands(
            IEnumerable<Command> commands,
            HashSet<KeyInput> neededInputs)
        {
            var list = new List<Command>();
            foreach (var cmd in commands.ToList())
            {
                foreach (var binding in cmd.GetKeyBindings())
                {
                    if (ShouldSkip(binding))
                    {
                        continue;
                    }

                    var input = binding.KeyBinding.FirstKeyInput;
                    if (neededInputs.Contains(input))
                    {
                        list.Add(cmd);
                        break;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Should this be skipped when removing conflicting bindings?
        /// </summary>
        public static bool ShouldSkip(CommandKeyBinding binding)
        {
            if (!IsImportantScope(binding.KeyBinding.Scope))
            {
                return true;
            }

            if (!binding.KeyBinding.KeyInputs.Any())
            {
                return true;
            }

            var first = binding.KeyBinding.FirstKeyInput;

            // Don't want to remove the arrow key bindings because it breaks items like
            // moving in the intellisense window
            if (first.IsArrowKey)
            {
                return true;
            }

            return false;
        }

        public static bool IsImportantScope(string scope)
        {
            var comp = StringComparer.OrdinalIgnoreCase;
            if (comp.Equals("Global", scope))
            {
                return true;
            }

            if (comp.Equals("Text Editor", scope))
            {
                return true;
            }

            if (comp.Equals(String.Empty, scope))
            {
                return true;
            }

            return false;
        }
    }
}
