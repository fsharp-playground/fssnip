let NullableToOption (n : System.Nullable<_>) = 
   if n.HasValue 
   then Some n.Value 
   else None
