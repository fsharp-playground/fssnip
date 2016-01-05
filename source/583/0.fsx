let products = 
  [ 1, "Tea", 2.3M; 
    2, "Coffee", 5.0M; 
    3, "Lemonade", 1.5M ]


[<View>]
let footer () = 
  h.contentTemplate {
    h?hr
    h?div.set("id", "footer") {
      h.content
    }
  }

[<View>]
let item (id, name, price) str = 
  h.template {
    h?li { 
      h?strong { 
        html.ActionLink(name, "details", route [ "id", id ])
      }
      !> " - Price: %s%f" str price 
    }
  }

[<View>]
let index () = 
  let title = "Product Listing"
  h?html {
    h?head {
      h?title { 
        !> "%s - View Engine Sample" title
      }
      h?link.set("typ", "text/css").set("rel", "stylesheet").set("href", "/Content/Site.css")
    }
    h?div.set("id", "main") {
      h?h1 { title }
      h?hr.set("cssclass", "heading")

      h?div { 
        h?ul.set("cssclass", "listing") {
          for prod in products do
            item prod "$"
        }
      }

      footer() {
        "This is an example of using "
        h?a.set("href", "http://fsharp.net") { 
          "the amazing F# language" 
        }
        @" for writing a simple an elegant view engine 
            for the ASP.NET MVC framework"
      }
    }
  }