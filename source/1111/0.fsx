//------------------------------------------------------------------------------------------------
// Below crap will go away as soon as Visual Studio will be updated to support F# 3.0
//------------------------------------------------------------------------------------------------
member X.output     with get()  = _output;                      and set v   = _output       <- v
member X.listPos    with get() : Collection<Packet> = _listPos; and set v   = _listPos      <- v
member X.socket     with get()  = _socket;                      and set v   = _socket       <- v
member X.E          with get()  = _E;                           and set v   = _E            <- v
member X.CE         with get()  = _CE;                          and set v   = _CE            <- v
member X.blocker    with get()  = _blocker;                     and set v   = _blocker      <- v
member X.restore    with get()  = _restore;                     and set v   = _restore      <- v
member X.cdev       with get()  = _cdev;                        and set v   = _cdev         <- v
member X.canread    with get()  = _canread;                     and set v   = _canread      <- v
member X.hack       with get()  = _hack;                        and set v   = _hack         <- v
member X.hack2      with get()  = _hack2;                       and set v   = _hack2        <- v
member X.config     with get()  = _configBytes;                 and set v   = _configBytes  <- v
member X.devices    with get()  = _devices;                     and set v   = _devices      <- v
member X.arrayRead  with get()  = _bytes;                       and set v   = _bytes        <- v
member X.properties with get()  = _properties;
member X.archive    with get()  = _archive;
member X.instant    with get()  = _instant;