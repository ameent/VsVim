﻿#light

namespace Vim
open Microsoft.VisualStudio.Text
open Microsoft.VisualStudio.Text.Editor;

type internal MotionResult = 
    | Complete of (SnapshotSpan * MotionKind * OperationKind )
    | NeedMoreInput of (KeyInput -> MotionResult)
    | InvalidMotion of string * (KeyInput -> MotionResult) 
    | Error of string
    | Cancel
    
module internal MotionCapture = 
    val ProcessView : ITextView -> KeyInput -> int -> MotionResult
    val ProcessInput : SnapshotPoint -> KeyInput -> int -> MotionResult
      
    
    
