let eval = 
    let dt = new System.Data.DataTable()
    fun expr -> System.Convert.ToDouble(dt.Compute(expr,""))

// usage (FSI)          
// > eval "(1+5)*7/((3+(2-1))/(7-3))";;
// val it : float = 42.0