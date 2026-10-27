# frk.cheat

menu cheat for the escapists 2, made with bepinex. press insert in game and the menu opens.

## install

1. download `installer.exe` from [releases](../../releases)
2. run it, it finds the game through steam by itself. if not, pick the folder with `TheEscapists2.exe`
3. press install, start the game, press insert

bepinex is built into the installer, so you don't need to download anything else. the remove button deletes only the plugin and leaves bepinex in place.

## features

| tab | what's there |
| --- | --- |
| player | no clip, clear heat, infinite health and energy, no attack cooldown, max stats, daily routine, auto-complete tasks, player speed, win game |
| items | infinite item durability, set money, spam items on floor, all keys |
| world | disable cameras and contraband scanners, fast desk opening, game speed, remove all walls, flood all toilets, safety level |
| fun | chat spam, swap hair and accessories |
| visual | version watermark and other visual stuff |
| menu | hide videos on menu buttons, copy menu hierarchy |
| zones | zone list |
| editor | zone requirements, zones on zones, items on items, random block replace, run checklist |
| configs | save, load and switch configs |

there are also unlock all cosmetics, dlc and prisons, achievements, 100% progress and stats editing (favours, knockouts, time, days in prison, minimap zoom).

configs are stored in `BepInEx/config/frkcheat`.

## build

you need the game installed and bepinex in its folder.

1. open `frkcheat.csproj` and set `gamedir` to your game folder
2. build the plugin
3. put the built plugin dll and `BepInEx_win_x64_5_4_23_6.zip` into `installer/`
4. build the installer

```
dotnet build -c Release
```

plugin targets net35, installer targets net48 and needs windows.

## notes

- tested only on the steam version, 32 bit
- use at your own risk, better not in online lobbies
- if something broke after a game update, open an issue
