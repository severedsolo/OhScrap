#!/bin/bash
xed /home/martin/Documents/GitHub/OhScrap/GameData/Severedsolo/OhScrap/Changelog.cfg
xed /home/martin/Documents/GitHub/OhScrap/GameData/Severedsolo/OhScrap/OhScrap.version
cp /home/martin/Documents/GitHub/OhScrap/OhScrap/bin/Release/OhScrap.dll /home/martin/Documents/GitHub/OhScrap/GameData/Severedsolo/OhScrap
zip -r OhScrap.zip GameData
notify-send "Oh Scrap build has finised"
