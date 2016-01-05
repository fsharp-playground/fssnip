open System

let toGrid groupSize items =
    let noOfItems = items |> Seq.length
    let noOfGroups = int(Math.Ceiling(float(noOfItems) / float(groupSize)))
    seq { for groupNo in 0..noOfGroups-1 do
            yield seq {
                for tableNoInGroup in 0..groupSize-1 do
                    let absoluteIndex = int((groupNo * groupSize) + tableNoInGroup)
                    if (absoluteIndex < noOfItems) then
                        yield items |> Seq.nth absoluteIndex
            }
        }

// Examples:


let eleven = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11]  

// seq [seq [1; 2]; seq [3; 4]; seq [5; 6]; seq [7; 8]; ... seq [11]
let test2 = eleven |> toGrid 2

// seq [seq [1; 2; 3; 4; 5; 6]; seq [7; 8; 9; 10; 11]]
let test6 = eleven |> toGrid 6

// ASP.NET MVC3 example - produces a two-way grid of charts:

(*
    @model IEnumerable<IEnumerable<myGroupedItems>>

    @{
        Layout = null;
    }
    <table>
        @foreach (var group in Model) {
            <tr>
                @foreach (var item in group) {
                        <td>
                            @item.name
                            <br />
                            <img src= "@(Url.Action("MyChart", "MyCharts", 
                              new { thingName = item.name, height = 100 }))" alt="chart"/>
                        </td>
                }
            </tr>
    }    
    </table>

*)
