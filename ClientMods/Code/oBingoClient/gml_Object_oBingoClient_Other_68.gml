var type = ds_map_find_value(async_load, "type")
var newline = "
";

switch type {
    case network_type_connect:
        popup_text("Bingo connected")
        exit
    case network_type_disconnect:
        popup_text("Bingo disconnected")
        exit
}

// We have received network data, which is the client's way of saying it wants data
var sendMap = ds_map_create()

if (global.playingGame) {
    ds_map_add(sendMap, "inGame", "true")
    var metroidMap = ds_map_create()
    for (var i = 0; i < array_length_1d(global.metdead); i++) {
        if (global.metdead[i]) {
            ds_map_add(metroidMap, string(i), true)
        }
    }
    ds_map_add_map(sendMap, "metroids", metroidMap)

    var itemMap = ds_map_create()
    for (var i = 0; i < array_length_1d(global.item); i++) {
        if (global.item[i]) {
            ds_map_add(itemMap, string(i), true)
        }
    }
    ds_map_add_map(sendMap, "items", itemMap)

    var eventMap = ds_map_create()
    for (var i = 0; i < array_length_1d(global.event); i++) {
        if (i == 101) {
            continue
        }
        if (global.event[i] != 0) {
            ds_map_add(eventMap, string(i), true)
        }
    }

    // This is the turbine event, it is 4 when all turbines are destroyed
    if (global.event[101] == 4) {
        ds_map_add(eventMap, "101", true)
    }

    ds_map_add_map(sendMap, "events", eventMap)

    var dMapMap = ds_map_create()
    for (var i = 0; i < array_height_2d(global.dmap); i++) {
        for (var j = 0; j < array_length_2d(global.dmap, i); j++) {
            if (global.dmap[i, j]) {
                ds_map_add(dMapMap, string(i) + ":" + string(j), true)
            }
        }
    }
    ds_map_add_map(sendMap, "mapTiles", dMapMap)

    var trooperLogMap = ds_map_create()
    for (var i = 0; i < 8; i++) {
        if (global.trooperlog[i] != 0) {
            ds_map_add(trooperLogMap, string(global.trooperlog[i]), true)
        }
    }
    ds_map_add_map(sendMap, "trooperLogs", trooperLogMap)

    var logMap = ds_map_create()
    for (var i = 0; i < array_length_1d(global.log); i++) {
        if (global.log[i]) {
            ds_map_add(logMap, string(i), true)
        }
    }
    ds_map_add_map(sendMap, "logs", logMap)

    var itemLocationMap = ds_map_create()
    for (var i = 0; i < array_length_1d(global.item); i++) {
        if (global.item[scr_itemchange(i)]) {
            ds_map_add(itemLocationMap, string(i), true)
        }
    }
    ds_map_add_map(sendMap, "itemLocations", itemLocationMap)
} else {
    ds_map_add(sendMap, "inGame", "false");
}

var json = json_encode(sendMap)
var buffer = buffer_create(1024, buffer_grow, 1)
buffer_write(buffer, buffer_text, json)
buffer_write(buffer, buffer_text, newline)

var socket = ds_map_find_value(async_load, "id")

network_send_raw(socket, buffer, buffer_tell(buffer))
ds_map_destroy(sendMap)
buffer_delete(buffer)