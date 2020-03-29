#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.
#SingleInstance force


!d::

Run D:\misc programming\TheBigBrawl\Build\TheBigBrawl.exe
WinWait, TheBigBrawl Configuration
Sleep, 500
ControlClick, Button3, TheBigBrawl Configuration

Sleep, 500

Run D:\misc programming\TheBigBrawl\Build\TheBigBrawl.exe
WinWait, TheBigBrawl Configuration
Sleep, 500
ControlClick, Button3, TheBigBrawl Configuration

Sleep, 5000

SetTitleMatchMode, 3 ; exact match 
winget, myList, list, TheBigBrawl
Loop, %myList%
{
	if (A_Index = 1) {
		WinActivate, % "ahk_id" myList%A_Index%

		; move window 
		CoordMode, Mouse, Screen
		MouseMove, 957, 237
		Click down
		MouseMove, 400, 240
		Click up
		CoordMode, Mouse, Window

		WinActivate, % "ahk_id" myList%A_Index%
		Click, 120, 80 ;LAN HOST 
	}

	if (A_Index = 2) {
		WinActivate, % "ahk_id" myList%A_Index%

		; move window 
		CoordMode, Mouse, Screen
		MouseMove, 957, 237
		Click down
		MouseMove, 1200, 240
		Click up
		CoordMode, Mouse, Window

		WinActivate, % "ahk_id" myList%A_Index%
		Click, 73, 107 ; LAN client
	}

	sleep, 1000
}



return


!Space::ExitApp