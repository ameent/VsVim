﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vim;
using Moq;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;

namespace VimCoreTest.Utils
{
    internal static class MockObjectFactory
    {
        internal static Mock<IRegisterMap> CreateRegisterMap()
        {
            var mock = new Mock<IRegisterMap>();
            var reg = new Register('_');
            mock.Setup(x => x.DefaultRegisterName).Returns('_');
            mock.Setup(x => x.DefaultRegister).Returns(reg);
            return mock;
        }

        internal static Mock<ITrackingLineColumnService> CreateTrackingLineColumnService()
        {
            var mock = new Mock<ITrackingLineColumnService>(MockBehavior.Strict);
            return mock;
        }

        internal static Mock<IVim> CreateVim(
            IRegisterMap registerMap = null,
            MarkMap map = null,
            IVimGlobalSettings settings = null,
            IVimHost host = null,
            IKeyMap keyMap = null)
        {
            registerMap = registerMap ?? CreateRegisterMap().Object;
            map = map ?? new MarkMap(new TrackingLineColumnService());
            settings = settings ?? new GlobalSettings();
            host = host ?? new FakeVimHost();
            keyMap = keyMap ?? (new KeyMap());
            var mock = new Mock<IVim>(MockBehavior.Strict);
            mock.Setup(x => x.RegisterMap).Returns(registerMap);
            mock.Setup(x => x.MarkMap).Returns(map);
            mock.Setup(x => x.Settings).Returns(settings);
            mock.Setup(x => x.Host).Returns(host);
            mock.Setup(x => x.KeyMap).Returns(keyMap);
            return mock;
        }

        internal static Mock<IEditorOperations> CreateEditorOperations()
        {
            var mock = new Mock<IEditorOperations>(MockBehavior.Strict);
            return mock;
        }

        internal static Mock<IVimGlobalSettings> CreateGlobalSettings(
            bool? ignoreCase = null,
            int? shiftWidth = null)
        {
            var mock = new Mock<IVimGlobalSettings>(MockBehavior.Strict);
            if (ignoreCase.HasValue)
            {
                mock.SetupGet(x => x.IgnoreCase).Returns(ignoreCase.Value);
            }
            if (shiftWidth.HasValue)
            {
                mock.SetupGet(x => x.ShiftWidth).Returns(shiftWidth.Value);
            }

            mock.SetupGet(x => x.DisableCommand).Returns(GlobalSettings.DisableCommand);
            return mock;
        }

        internal static Mock<IVimLocalSettings> CreateLocalSettings(
            IVimGlobalSettings global = null)
        {
            global = global ?? CreateGlobalSettings().Object;
            var mock = new Mock<IVimLocalSettings>(MockBehavior.Strict);
            mock.SetupGet(x => x.GlobalSettings).Returns(global);
            return mock;
        }


        internal static Mock<IVimBuffer> CreateVimBuffer(
            IWpfTextView view,
            string name = null,
            IVim vim = null,
            IEditorOperations editorOperations = null,
            IJumpList jumpList = null,
            IVimLocalSettings settings = null )
        {
            name = name ?? "test";
            vim = vim ?? CreateVim().Object;
            editorOperations = editorOperations ?? CreateEditorOperations().Object;
            jumpList = jumpList ?? (new Mock<IJumpList>(MockBehavior.Strict)).Object;
            settings = settings ?? new LocalSettings(vim.Settings, view);
            var mock = new Mock<IVimBuffer>(MockBehavior.Strict);
            mock.SetupGet(x => x.TextView).Returns(view);
            mock.SetupGet(x => x.TextBuffer).Returns(() => view.TextBuffer);
            mock.SetupGet(x => x.TextSnapshot).Returns(() => view.TextSnapshot);
            mock.SetupGet(x => x.Name).Returns(name);
            mock.SetupGet(x => x.EditorOperations).Returns(editorOperations);
            mock.SetupGet(x => x.VimHost).Returns(vim.Host);
            mock.SetupGet(x => x.Settings).Returns(settings);
            mock.SetupGet(x => x.MarkMap).Returns(vim.MarkMap);
            mock.SetupGet(x => x.RegisterMap).Returns(vim.RegisterMap);
            mock.SetupGet(x => x.JumpList).Returns(jumpList);
            return mock;
        }

        internal static Mock<ITextCaret> CreateCaret()
        {
            return new Mock<ITextCaret>(MockBehavior.Strict);
        }

        internal static Mock<ITextSelection> CreateSelection()
        {
            return new Mock<ITextSelection>(MockBehavior.Strict);
        }

        internal static Mock<IWpfTextView> CreateWpfTextView(
            ITextBuffer buffer,
            ITextCaret caret = null,
            ITextSelection selection = null)
        {
            caret = caret ?? CreateCaret().Object;
            selection = selection ?? CreateSelection().Object;
            var view = new Mock<IWpfTextView>(MockBehavior.Strict);
            view.SetupGet(x => x.Caret).Returns(caret);
            view.SetupGet(x => x.Selection).Returns(selection);
            view.SetupGet(x => x.TextBuffer).Returns(buffer);
            view.SetupGet(x => x.TextSnapshot).Returns(() => buffer.CurrentSnapshot);
            return view;
        }

        internal static Tuple<Mock<IWpfTextView>, Mock<ITextCaret>, Mock<ITextSelection>> CreateWpfTextViewAll(ITextBuffer buffer)
        {
            var caret = CreateCaret();
            var selection = CreateSelection();
            var view = CreateWpfTextView(buffer, caret.Object, selection.Object);
            return Tuple.Create(view, caret, selection);
        }

        internal static Mock<ITextBuffer> CreateTextBuffer()
        {
            var mock = new Mock<ITextBuffer>(MockBehavior.Strict);
            return mock;
        }

        internal static Mock<ITextSnapshot> CreateTextSnapshot(
            int length,
            ITextBuffer buffer = null )
        {
            buffer = buffer ?? CreateTextBuffer().Object;
            var mock = new Mock<ITextSnapshot>(MockBehavior.Strict);
            mock.SetupGet(x => x.Length).Returns(length);
            mock.SetupGet(x => x.TextBuffer).Returns(buffer);
            return mock;
        }

        internal static SnapshotPoint CreateSnapshotPoint(
            int position,
            ITextSnapshot snapshot = null)
        {
            snapshot = snapshot ?? CreateTextSnapshot(position + 1).Object;
            return new SnapshotPoint(snapshot, position);
        }
    }
}
