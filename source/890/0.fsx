type StacitMemberCls() = 
    //A static filed should be defined first
    static let mutable staticFiled = ""  
      
    static member StaticProp
        with get() = staticFiled
        and set(v) = staticFiled <- v