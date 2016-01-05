let instantiateTransformerAndCallMethod() = 
    // c# object I need to use
    let transformer = Transformer1()

    // c# event object I need to create 
    let event = Event1(1, "aString")

    // invoking c# method passing event in as an argument
    let transformResult = transformer.Transform(event)

    // Is there any way I can call the Transform() method above using an "anonymous" construct e.g. a record instead of having to instantiates Event1 instance?
    // It is not allowing me to do this - Im assuming it is because f# is strongly typed?