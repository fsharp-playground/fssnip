open System.Collections.Generic

// Base class for all UndoableCommand Objects
[<AbstractClass>]
type UndoableCommand(description:string) = 
    member this.Description = description
    abstract Execute : unit->unit
    abstract Undo : unit->unit

// Class that can handle property changes. Note we use reference cells to directly 
// manipulate the underlying values
type PropertyChangedUndoableCommand<'a>(description, fieldRef, newValue:'a) =
    inherit UndoableCommand(description) 
    let oldValue = !fieldRef
    override this.Execute() = fieldRef:=newValue
    override this.Undo() = fieldRef:=oldValue
        
// Class that executes actions to "do" and "undo"
// Obviously undo should actually undo what "do" does, but we cant enforce it
type DelegateUndoableCommand(description, doAction, undoAction) = 
    inherit UndoableCommand(description)
    override this.Execute() = doAction()
    override this.Undo() = undoAction()

//Document contains an example undo/redo stack
type Document() =
    let undoStack = Stack()
    let redoStack = Stack()

    let execute (command : UndoableCommand) =
        redoStack.Clear() //as we are executing a command any existing redo is invalidated
        undoStack.Push(command)
        command.Execute()

    let undo() = 
        if undoStack.Count > 0 then
            let command = undoStack.Pop()
            redoStack.Push(command)
            command.Undo()
            
    let redo() = 
        if redoStack.Count> 0 then
            let command = redoStack.Pop()
            undoStack.Push(command)
            command.Execute()
            
    member this.ExecuteCommand command = execute command
    member this.Undo() = undo()
    member this.Redo() = redo()
    member this.CanUndo = undoStack.Count > 0
    member this.CanRedo = redoStack.Count > 0
    
//Example implementation
type SomeObject(document:Document) = 
    let undoableProperty = ref 50 //Because of the command we cant use mutable

    member this.UndoableProperty with get() = !undoableProperty
                                 and set(value) =
                                    //instead of directly setting the property we create a command and 
                                    //execute it on the document.   
                                    let command = PropertyChangedUndoableCommand("Changed", undoableProperty, value)
                                    document.ExecuteCommand(command)
                           
let doc = Document() //Document that will hold our doings and undoings
let someObject = SomeObject(doc)

printf "Initial Value %d\n" someObject.UndoableProperty //50
someObject.UndoableProperty <- 100
printf "Updated Value %d\n" someObject.UndoableProperty //100
someObject.UndoableProperty <- 1000
printf "Updated Value %d\n" someObject.UndoableProperty //1000
doc.Undo()
printf "Undone Value %d\n" someObject.UndoableProperty // 100
doc.Undo()
printf "Undone Value %d\n" someObject.UndoableProperty // 50
doc.Undo()
printf "Undone Value %d\n" someObject.UndoableProperty // 50
doc.Undo()
printf "Undone Value %d\n" someObject.UndoableProperty // 50
doc.Redo()
printf "Redone Value %d\n" someObject.UndoableProperty // 100
doc.Redo()
printf "Redone Value %d\n" someObject.UndoableProperty // 1000
doc.Redo()
printf "Redone Value %d\n" someObject.UndoableProperty // 1000
System.Console.ReadLine()|>ignore
