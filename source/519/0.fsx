
type EntityLevel = { 
    Name:string;                  // Current entity name
    Count:int;                    // number of entities to create at this level
    Values:(string * string) list;        
    RelationshipName:string;      // name of the relationship from this entity to its parent (if applicable)
    Relations : EntityLevel list; // All children
}