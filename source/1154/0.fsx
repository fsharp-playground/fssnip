let Easter year =
    let a = year % 19
    let b = year / 100
    let c = year % 100
    let d =  b /4
    let e = b % 4
    let f = (b+8) / 25
    let g = (b - f + 1) /3
    let h = (19 * a + b - d - g + 15) % 30
    let i = c / 4
    let k = c % 4
    let l = ( 32 + 2 * e + 2 * i - h - k) % 7
    let m = (a + 11 * h + 22 * l ) / 451
    let n = ( h + l - 7 * m + 114) / 31
    let p = ( h + l - 7 * m + 114) % 31
    (p+1,n,year)