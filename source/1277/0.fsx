    do this.Root.Add [createSection [yield createStringElement "Section1";
                                     for i in 1..10 -> createStringElement ("num"+i.ToString())];
                      createSection [yield createStringElement "Section2";
                                     for i in 1..10 -> createStringElement ("num"+i.ToString())];
                      createSection [yield createStringElement "Section3";
                                     for i in 1..10 -> createStringElement ("num"+i.ToString())]]