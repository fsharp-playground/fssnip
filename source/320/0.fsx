let beast = quest {
    let player = new Player()

    // this is the beginning, we require player to talk with an npc using specified dialog node
    do! talk player "Rudolf" "KillBeast", kill player "Beast"
    player.StartQuest("Beast") // and then, the rest will be called when player does this

    do! kill player "Beast"
    if player.KillCount("Beast") > 2 then
        player.GiveXP(200)
}