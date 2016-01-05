open System.Net
open System.Windows.Forms
open System.Drawing

/// Download a picture of an unpunched punched card from the internet
let punchCard = 
  let wc = new WebClient()
  use sr = wc.OpenRead("http://www.columbia.edu/cu/computinghistory/card05.gif")
  Bitmap.FromStream(sr)

/// Display an image in a newly opened windows form
let show (bmp:Image) =
  new Form(BackgroundImage=bmp, Visible=true, 
           ClientSize=Size(bmp.Width, bmp.Height))

// Specifies where the places to punch are on the punched card
let offsX, offsY = 0.0f, 12.0f
let spaceX, spaceY = 7.15f, 20.4f
let widthX, widthY = 3.2f, 8.0f

/// Create a punched card showing the specified text
let punch text = 
  /// Generate a bitmap that contains the text in a big font
  let textLayer = 
    let bmp = new Bitmap(punchCard.Width, punchCard.Height)
    use gr = Graphics.FromImage(bmp)
    gr.DrawString(text, new Font("Consolas", 160.0f), 
                  Brushes.Black, PointF(-40.0f, 30.0f))
    bmp

  /// Calculate if the specified rectangle contains some part
  /// of the text (in the 'textLayer' bitmap)
  let mostlyBlack (rect:RectangleF) = 
    let l = int rect.X
    let t = int rect.Y
    // Average the 'A' component over the given rectangle
    let avg =
      seq { for x in 0 .. int rect.Width do
              for y in 0 .. int rect.Height do
                let rx a = max 0 (min (textLayer.Width - 1) a)
                let ry a = max 0 (min (textLayer.Height - 1) a)
                let pix = textLayer.GetPixel(rx (l + x), ry (t + y))
                yield (float pix.A) / 255.0 }
      |> Seq.average
    avg > 0.1

  /// Create a new layer to represent punches in the card
  let punchLayer = new Bitmap(punchCard.Width, punchCard.Height)
  use gr = Graphics.FromImage(punchLayer)

  /// Iterate over the possible holes in the punched card
  for x in 0 .. punchCard.Width / int spaceX do
    for y in 0 .. punchCard.Height / int spaceY do
      // A rectangle representing the hole location
      let rect = 
        RectangleF
          (offsX + (float32 x) * spaceX, 
           offsY + (float32 y) * spaceY, widthX, widthY)

      // If the rectangle overlaps with text, then punch it!
      if mostlyBlack rect then
        rect.Inflate(0.3f, 2.0f)
        gr.FillRectangle(Brushes.Black, rect)

  // Generate composed bitmap and return it
  let img = new Bitmap(punchCard.Width, punchCard.Height)
  use gr = Graphics.FromImage(img)
  gr.DrawImage(punchCard, Point(0, 0))
  gr.DrawImage(punchLayer, Point(0, 0))
  img

// Enterprise punched card
show (punch "<?xml")