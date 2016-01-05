open System.Windows
open System.Windows.Controls

// Example ported from http://www.silverlightshow.net/items/Attached-Properties-in-Silverlight.aspx
type TabPanel() = 
   inherit StackPanel()  
   /// Register the attached property
   static member TabStopProperty = 
       DependencyProperty.RegisterAttached("TabStop", typeof<bool>, typeof<TabPanel>, null) 
   /// Set the property
   static member SetTabStop (element:UIElement) (value:obj) =
       let _, boolValue = bool.TryParse (string value)
       element.SetValue(TabPanel.TabStopProperty, boolValue)
   /// Get the property
   static member GetTabStop (element:UIElement) = 
       element.GetValue TabPanel.TabStopProperty                                            