if (instance_exists(oRoboMineProj)) {
    hasExploded = true
}

if (hasExploded) {
    exit
}

if (!instance_exists(oRoboMine)) {
    global.event[313] = 1
}

alarm[0] = 3