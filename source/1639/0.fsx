                if not(Seq.isEmpty trackers) then
                    "&tr=" + String.Join("&tr=", Array.ofSeq(trackers |> Seq.map(fun link -> Uri.EscapeDataString(link)))) + 
                                        "&xc=TorrentRTSearch"
                else
                    String.Empty