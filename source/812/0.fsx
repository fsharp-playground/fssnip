type List<'a> = 
| Empty 
| Cons of 'a * List<'a>