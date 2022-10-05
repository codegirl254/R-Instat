﻿' R- Instat
' Copyright (C) 2015-2017
'
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License 
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports instat
Imports instat.Translations
Imports RDotNet

Public Class dlgHideDataframes
    Public bFirstLoad As Boolean = True
    Private bReset As Boolean = True
    Private clsHideDataFramesFunction As New RFunction
    Private clsAppendToDataFrameFunction As New RFunction
    Private clsMappingFunction As New RFunction
    Private clsDataUnhideFunction As New ROperator
    Private clsTildeOperator As New ROperator

    Private clsDummyFunction As New RFunction

    Private Sub dlgHideDataframes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If bFirstLoad Then
            InitialiseDialog()
            bFirstLoad = False
        End If
        If bReset Then
            SetDefaults()
        End If
        SetRCodeForControls(bReset)
        'SetHiddenColumns()
        bReset = False
        autoTranslate(Me)
    End Sub

    Private Sub InitialiseDialog()
        ucrReceiverMultiple.SetParameter(New RParameter("data_names", 0))
        ucrReceiverMultiple.SetParameterIsString()
        ucrReceiverMultiple.Selector = ucrSelectorForDataFrames
        ucrReceiverMultiple.strSelectorHeading = "Data Frames"
        ucrReceiverMultiple.SetItemType("dataframe")
        'ucrReceiverMultiple.SetMeAsReceiver()

        ucrReceiverMultipleUnhide.SetParameter(New RParameter("data_names", 0))
        ucrReceiverMultipleUnhide.SetParameterIsString()
        ucrReceiverMultipleUnhide.Selector = ucrSelectorForDataFrames
        ucrReceiverMultipleUnhide.strSelectorHeading = "Data Frames"
        ucrReceiverMultipleUnhide.SetItemType("dataframe")
        'ucrReceiverMultipleUnhide.SetMeAsReceiver()

        ucrPnlHideUnhide.AddRadioButton(rdoHideDataFrame)
        ucrPnlHideUnhide.AddRadioButton(rdoUnhideDataFrame)
        ucrPnlHideUnhide.AddParameterValuesCondition(rdoHideDataFrame, "checked", "rdoHide")
        ucrPnlHideUnhide.AddParameterValuesCondition(rdoUnhideDataFrame, "checked", "rdoUnhide")


        ucrReceiverMultiple.SetLinkedDisplayControl(lblHiddenDataFrames)
        ucrReceiverMultipleUnhide.SetLinkedDisplayControl(lblUnhideDataFrame)

        ucrPnlHideUnhide.AddToLinkedControls(ucrReceiverMultiple, {rdoHideDataFrame}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlHideUnhide.AddToLinkedControls(ucrReceiverMultipleUnhide, {rdoUnhideDataFrame}, bNewLinkedHideIfParameterMissing:=True)

    End Sub

    Private Sub SetDefaults()
        clsHideDataFramesFunction = New RFunction
        clsAppendToDataFrameFunction = New RFunction
        clsMappingFunction = New RFunction
        clsDataUnhideFunction = New ROperator
        clsTildeOperator = New ROperator
        clsDummyFunction = New RFunction

        clsDummyFunction.AddParameter("checked", "rdoHide", iPosition:=0)

        clsMappingFunction.SetPackageName("purrr")
        clsMappingFunction.SetRCommand("map")
        clsMappingFunction.AddParameter(".x", clsROperatorParameter:=clsDataUnhideFunction, iPosition:=0)
        clsMappingFunction.AddParameter(".f", clsROperatorParameter:=clsTildeOperator, iPosition:=1)

        clsTildeOperator.SetOperation("~")
        clsTildeOperator.AddParameter("right", clsRFunctionParameter:=clsAppendToDataFrameFunction, iPosition:=1)
        clsTildeOperator.bForceIncludeOperation = True

        clsDataUnhideFunction.SetOperation("", bBracketsTemp:=False)
        clsDataUnhideFunction.SetAssignTo("data_to_unhide")
        'clsDataUnhideFunction.AddParameter("data", ucrReceiverMultipleUnhide.GetVariableNames(True), iPosition:=0, bIncludeArgumentName:=False)

        clsHideDataFramesFunction.SetRCommand(frmMain.clsRLink.strInstatDataObject & "$set_hidden_data_frames")
        clsAppendToDataFrameFunction.SetRCommand(frmMain.clsRLink.strInstatDataObject & "$append_to_dataframe_metadata")
        clsAppendToDataFrameFunction.AddParameter("data_name", ".x", iPosition:=0)
        clsAppendToDataFrameFunction.AddParameter("property", "is_hidden_label", iPosition:=1)
        clsAppendToDataFrameFunction.AddParameter("new_val", "FALSE", iPosition:=2)

        ucrBase.clsRsyntax.SetBaseRFunction(clsHideDataFramesFunction)
    End Sub

    Private Sub TestOKEnabled()
        ' You cannot hide all data frames. When the receiver is blank all data frames are unhidden so this is allowed.
        If ucrReceiverMultiple.lstSelectedVariables.Items.Count <> ucrSelectorForDataFrames.lstAvailableVariable.Items.Count Then
            ucrBase.OKEnabled(True)
        Else
            ucrBase.OKEnabled(False)
        End If
    End Sub

    Private Sub ucrBase_ClickReset(sender As Object, e As EventArgs) Handles ucrBase.ClickReset
        SetDefaults()
        SetRCodeForControls(True)
        TestOKEnabled()
    End Sub

    Public Sub SetRCodeForControls(bReset As Boolean)
        'SetRCode(Me, ucrBase.clsRsyntax.clsBaseFunction, bReset)

        ucrPnlHideUnhide.SetRCode(clsDummyFunction, bReset)
        ucrReceiverMultiple.SetRCode(clsHideDataFramesFunction, bReset)
        'ucrReceiverMultipleUnhide.SetRCode(clsMappingFunction, bReset)
    End Sub

    'Private Sub SetHiddenColumns()
    '    Dim expTemp As SymbolicExpression
    '    Dim chrHiddenColumns As CharacterVector
    '    Dim clsGetHiddenDataFrames As New RFunction

    '    clsGetHiddenDataFrames.SetRCommand(frmMain.clsRLink.strInstatDataObject & "$get_hidden_data_frames")

    '    ucrReceiverMultiple.Clear()
    '    expTemp = frmMain.clsRLink.RunInternalScriptGetValue(clsGetHiddenDataFrames.ToScript(), bSilent:=True)
    '    If expTemp IsNot Nothing AndAlso expTemp.Type <> Internals.SymbolicExpressionType.Null Then
    '        chrHiddenColumns = expTemp.AsCharacter
    '        For Each strDataFrame As String In chrHiddenColumns
    '            ucrReceiverMultiple.Add(strDataFrame)
    '        Next
    '    End If
    'End Sub

    Private Sub ucrReceiverMultiple_ControlContentsChanged(ucrChangedControl As ucrCore) Handles ucrReceiverMultiple.ControlContentsChanged
        TestOKEnabled()
    End Sub

    Private Sub ucrPnlHideUnhide_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrPnlHideUnhide.ControlValueChanged
        If rdoHideDataFrame.Checked Then
            ucrBase.clsRsyntax.SetBaseRFunction(clsHideDataFramesFunction)
            clsDummyFunction.AddParameter("checked", "rdoHide", iPosition:=0)
            ucrReceiverMultiple.SetMeAsReceiver()

        Else
            ucrBase.clsRsyntax.SetBaseRFunction(clsMappingFunction)
            clsDummyFunction.AddParameter("checked", "rdoUnhide", iPosition:=0)
            ucrReceiverMultipleUnhide.SetMeAsReceiver()

        End If
    End Sub

    Private Sub ucrReceiverMultipleUnhide_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrReceiverMultipleUnhide.ControlValueChanged
        clsDataUnhideFunction.AddParameter("data", ucrReceiverMultipleUnhide.GetVariableNames(True), iPosition:=0, bIncludeArgumentName:=False)
    End Sub
End Class