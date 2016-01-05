let beast = quest {
    let player = new Player()

    do! talk player "Rudolf" "KillBeast" // this is the beginning, we require player to talk with an npc using specified dialog node
    player.StartQuest("Beast") // and then, the rest will be called when player does this
    
    do! kill player "Beast" // so, when he talks to that npc, this piece is executed, and it attaches the rest, as the handler for kill event
    player.GiveXP(100) // and then, when monster is killed, this is executed
}