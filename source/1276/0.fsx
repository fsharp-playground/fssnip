namespace GoFormz

open System
open System.Drawing
open MonoTouch.Foundation
open MonoTouch.UIKit
open MonoTouch.Dialog
open System.Linq

[<Register ("MasterViewController")>]
type MasterViewController (window:UIWindow) as this =
    inherit DialogViewController (new RootElement("Items"))

    do this.Root.Add [new Section(Elements = new ResizeArray<Element>( [yield new StringElement("Section1") :> Element;
                                                                        for i in 1..10 -> new StringElement("num"+i.ToString()) :> Element]));
                      new Section(Elements = new ResizeArray<Element>( [yield new StringElement("Section1") :> Element;
                                                                        for i in 1..10 -> new StringElement("num"+i.ToString()) :> Element]));
                      new Section(Elements = new ResizeArray<Element>( [yield new StringElement("Section1") :> Element;
                                                                        for i in 1..10 -> new StringElement("num"+i.ToString()) :> Element]))];

    override this.ShouldAutorotateToInterfaceOrientation toInterfaceOrientation =
        true