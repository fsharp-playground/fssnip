open System
open System.Drawing
open System.Windows.Forms

// Create form, button and add button to form
let form = new Form(Text = "Hello world!")
let btn = new Button(Text = "Click here")
form.Controls.Add(btn)

// Register event handler for button click event
btn.Click.Add(fun _ ->
  // Generate random color and set it as background
  let rnd = new Random()
  let r, g, b = rnd.Next(256), rnd.Next(256), rnd.Next(256)
  form.BackColor <- Color.FromArgb(r, g, b) )

// Show the form (in F# Interactive)
form.Show()
// Run the application (in compiled application)
Application.Run(form)