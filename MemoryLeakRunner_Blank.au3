#include <GUIConstantsEx.au3>
#include <StaticConstants.au3>
#include <WindowsConstants.au3>
#include <MsgBoxConstants.au3>
#include <GuiTreeView.au3>

;Customizable variables
Global $windowName = ""
Local $waitTimeout = 10

#Region Config and Init --Non-Editable: This logic should not need to change for new tests
;Configure script hotkey mappings
HotKeySet("{PAUSE}", "TogglePause")
HotKeySet("{END}", "Terminate")

;Configure global variables for scripts -- these should not be edited
Global $Paused, $hWnd = 0, $InitMemory
Global $statusLabel, $countLabel, $initMemoryLabel, $currMemoryLabel, $highMemoryLabel, $lastMemoryDeltaLabel, $highMemoryDeltaLabel

;Ensure that the configured window is present
While $hWnd == 0
	$hWnd = WinWait($windowName, "", $waitTimeout)

	If $hWnd == 0 Then
		MsgBox($MB_SYSTEMMODAL, "Error", "Window with title '" & $windowName & "' was not found within '" & $waitTimeout & "' seconds. Ensure the window is open and try again")
		$windowName = ""
	EndIf
WEnd

;Open the overlay and start the test
InitOverlay()
DoWork()
#EndRegion

Func DoWork()
	Local $LoopCount = 0, $highMemoryDelta, $highMemory, $prevMemory = $InitMemory
	While 1

		#Region Loop Processing 	--Non-Editable: This logic should not need to change for new tests--
		;Ensure user didn't change windows while running test
		IF NOT WinActive($windowName) Then
			WinActivate($windowName)
			WinWaitActive($windowName)
		EndIf

		;This region is responsible for handling updating the overlay and performing any processing to drive the iteration
		$LoopCount = $LoopCount + 1
		GUICtrlSetData($countLabel, $LoopCount)

		$CurrMemory = GetMemoryInMB($hWnd)

		GUICtrlSetData($currMemoryLabel, $CurrMemory & "MB")

		If $CurrMemory > $highMemory Then
			$highMemory = $CurrMemory
			GUICtrlSetData($highMemoryLabel, $highMemory & "MB")
		EndIf

		$MemoryDelta =  Round( $CurrMemory - $prevMemory, 2)

		GUICtrlSetData($lastMemoryDeltaLabel, $MemoryDelta & "MB")

		If $MemoryDelta > 0 Then
			GUICtrlSetColor($lastMemoryDeltaLabel, 0xFF0000)
		Else
			GUICtrlSetColor($lastMemoryDeltaLabel, 0x3399FF)
		EndIf

		If $MemoryDelta > $highMemoryDelta Then
			$highMemoryDelta = $MemoryDelta
			GUICtrlSetData($highMemoryDeltaLabel, $highMemoryDelta & "MB")

			If $highMemoryDelta > 0 Then
				GUICtrlSetColor($highMemoryDeltaLabel, 0xFF0000)
			Else
				GUICtrlSetColor($highMemoryDeltaLabel, 0x3399FF)
			EndIf
		EndIf

		$prevMemory = $CurrMemory

		#EndRegion

		#Region Loop Body 			--Editable Region: Test body goes here--
		

		#EndRegion
	WEnd
EndFunc

#Region Common Functions 	--Non-Editable: This logic should not need to change for new tests--

Func InitOverlay()
	Local $overlayHeight = 140, $overlayWidth = 200

	$InitMemory = GetMemoryInMB($hWnd)

	$hGUI = GUICreate("", $overlayWidth, $overlayHeight, @DesktopWidth - $overlayWidth, @DesktopHeight - $overlayHeight, $WS_POPUP, BitOr($WS_EX_TOOLWINDOW,$WS_EX_TOPMOST,$WS_EX_TRANSPARENT))

	CreateTitleLabel("Script Status:", 1)
	$statusLabel = CreateValueLabel("Running",1)

	CreateTitleLabel("# Of Runs:", 2)
	$countLabel = CreateValueLabel("0",2)

	CreateTitleLabel("Initial Memory:", 3)
	$initMemoryLabel = CreateValueLabel($InitMemory & "MB", 3)

	CreateTitleLabel("Current Memory:", 4)
	$currMemoryLabel = CreateValueLabel($InitMemory & "MB", 4)

	CreateTitleLabel("Highest Memory:", 5)
	$highMemoryLabel = CreateValueLabel($InitMemory & "MB", 5)

	CreateTitleLabel("Last Delta:", 6)
	$lastMemoryDeltaLabel = CreateValueLabel("0MB", 6)

	CreateTitleLabel("Highest Delta:", 7)
	$highMemoryDeltaLabel = CreateValueLabel("0MB", 7)

	GUICtrlSetColor($statusLabel, 0x3399FF)

	GUISetState(@SW_SHOW, $hGUI)
	WinSetTrans($hGUI, "", 220)
EndFunc

Func TogglePause()
		$Paused = NOT $Paused
		If $Paused THEN
			GUICtrlSetData($statusLabel, "Paused")
			GUICtrlSetColor($statusLabel, 0xFF0000)
		Else
			GUICtrlSetData($statusLabel, "Running")
			GUICtrlSetColor($statusLabel, 0x3399FF)
		EndIf
		While $Paused
			sleep(100)
		WEnd
EndFunc

Func Terminate()
	Exit 0
EndFunc

Func GetMemoryInMB($WindowHandle)
	$ProcessId = WinGetProcess($WindowHandle)
	$memoryInfo = ProcessGetStats($ProcessId)
	If IsArray($memoryInfo) Then
		Return Round($memoryInfo[0] / 1024 / 1024, 2)
	Else
		Return 0
	EndIf
EndFunc

Func CreateTitleLabel($TEXT, $ROWNUM)
	If Not IsDeclared("ROWHEIGHT") Then
			Local $ROWHEIGHT = 18
	EndIf

	$Label = GUICTRLCreateLabel($TEXT, 5, ($ROWNUM * $ROWHEIGHT) - $ROWHEIGHT + 3, 95, 15, $SS_RIGHT)
	GUICtrlSetFont($Label,8,800)
EndFunc

Func CreateValueLabel($VALUE, $ROWNUM)
	If Not IsDeclared("ROWHEIGHT") Then
			$ROWHEIGHT = 18
	EndIf

	Return GUICtrlCreateLabel($VALUE, 105, ($ROWNUM * $ROWHEIGHT) - $ROWHEIGHT + 3, 95, 15)
EndFunc

#EndRegion