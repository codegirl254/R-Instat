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

Public Class dlgRecodeFactor
    Private bFirstLoad As Boolean = True
    Private clsFctRecodeFunction, clsFctOtherFunction As New RFunction
    Private clsFctLowFreqFunction, clsFctLumpPropFunction As New RFunction
    Private clsCFunction As New RFunction
    Private bReset As Boolean = True
    Private Sub dlgRecodeFactor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If bFirstLoad Then
            InitialiseDialog()
            bFirstLoad = False
        End If
        If bReset Then
            SetDefaults()
        End If
        SetRCodeforControls(bReset)
        bReset = False
        autoTranslate(Me)
        TestOKEnabled()
    End Sub

    Private Sub InitialiseDialog()
        ucrBase.iHelpTopicID = 37

        ucrPnlOptions.AddRadioButton(rdoRecode)
        ucrPnlOptions.AddRadioButton(rdoOther)
        ucrPnlOptions.AddRadioButton(rdoLump)

        ucrPnlOptions.AddFunctionNamesCondition(rdoRecode, "fct_recode")
        ucrPnlOptions.AddFunctionNamesCondition(rdoOther, "fct_other")
        ucrPnlOptions.AddFunctionNamesCondition(rdoLump, "fct_lump")

        ucrPnlMethods.AddRadioButton(rdoKeep)
        ucrPnlMethods.AddRadioButton(rdoDrop)

        ucrPnlKeep.AddRadioButton(rdoLevels)
        ucrPnlKeep.AddRadioButton(rdoCommonValues)
        ucrPnlKeep.AddRadioButton(rdoFrequentValues)
        ucrPnlKeep.AddRadioButton(rdoMore)

        ucrNudLevels.SetParameter(New RParameter(""))
        ucrNudLevels.SetMinMax(0, Integer.MaxValue)
        ucrNudLevels.Increment = 1

        ucrNudCommonValues.SetParameter(New RParameter(""))
        ucrNudCommonValues.SetMinMax(Integer.MinValue, Integer.MaxValue)

        ucrNudFrequentValues.SetParameter(New RParameter("prop", 1))
        ucrNudFrequentValues.SetMinMax(-1, 1)
        ucrNudFrequentValues.Increment = 0.01

        ucrReceiverFactor.SetParameter(New RParameter(".f", 0))
        ucrReceiverFactor.Selector = ucrSelectorForRecode
        ucrReceiverFactor.SetIncludedDataTypes({"factor"}, bStrict:=True)
        ucrReceiverFactor.strSelectorHeading = "Factors"
        ucrReceiverFactor.SetMeAsReceiver()
        ucrReceiverFactor.SetParameterIsRFunction()
        ucrReceiverFactor.bUseFilteredData = False

        ucrFactorGrid.SetReceiver(ucrReceiverFactor)
        ucrFactorGrid.SetAsViewerOnly()
        ucrFactorGrid.bIncludeCopyOfLevels = True
        ucrFactorGrid.AddEditableColumns({"New Label"})

        ucrInputOther.SetText("Other")

        ucrSaveNewColumn.SetSaveTypeAsColumn()
        ucrSaveNewColumn.SetDataFrameSelector(ucrSelectorForRecode.ucrAvailableDataFrames)
        ucrSaveNewColumn.SetIsComboBox()
        ucrSaveNewColumn.SetLabelText("New Column Name:")
        ucrSaveNewColumn.setLinkedReceiver(ucrReceiverFactor)

        ucrPnlOptions.AddToLinkedControls({ucrFactorGrid}, {rdoRecode, rdoOther}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlOptions.AddToLinkedControls({ucrInputOther}, {rdoOther, rdoLump}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlOptions.AddToLinkedControls(ucrPnlMethods, {rdoOther}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlOptions.AddToLinkedControls(ucrPnlKeep, {rdoLump}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlMethods.SetLinkedDisplayControl(grpSelectedValues)
        ucrPnlKeep.SetLinkedDisplayControl(grpKeep)
        ucrInputOther.SetLinkedDisplayControl(lblOther)

        ucrPnlMethods.AddToLinkedControls(ucrNudLevels, {rdoLevels}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlMethods.AddToLinkedControls(ucrNudCommonValues, {rdoCommonValues}, bNewLinkedHideIfParameterMissing:=True)
        ucrPnlMethods.AddToLinkedControls(ucrNudFrequentValues, {rdoFrequentValues}, bNewLinkedHideIfParameterMissing:=True, bNewLinkedChangeParameterValue:=True, objNewDefaultState:=0.1)

    End Sub

    Private Sub SetDefaults()
        clsFctRecodeFunction = New RFunction
        clsFctOtherFunction = New RFunction
        clsFctLowFreqFunction = New RFunction
        clsFctLumpPropFunction = New RFunction
        clsCFunction = New RFunction

        ucrSelectorForRecode.Reset()
        ucrSelectorForRecode.Focus()
        ucrFactorGrid.ResetText()
        ucrSaveNewColumn.Reset()

        rdoRecode.Checked = True
        rdoKeep.Checked = True

        clsCFunction.SetRCommand("c")

        clsFctLumpPropFunction.SetPackageName("forcats")
        clsFctLumpPropFunction.SetRCommand("fct_lumn_prop")

        clsFctLowFreqFunction.SetPackageName("forcats")
        clsFctLowFreqFunction.SetRCommand("fct_lump_lowfreq")

        clsFctOtherFunction.SetPackageName("forcats")
        clsFctOtherFunction.SetRCommand("fct_other")
        clsFctOtherFunction.AddParameter("keep", clsRFunctionParameter:=clsCFunction, iPosition:=1)

        clsFctRecodeFunction.SetPackageName("forcats")
        clsFctRecodeFunction.SetRCommand("fct_recode")
        clsFctRecodeFunction.SetAssignTo(strTemp:=ucrSaveNewColumn.GetText(), strTempDataframe:=ucrSelectorForRecode.ucrAvailableDataFrames.cboAvailableDataFrames.Text, strTempColumn:=ucrSaveNewColumn.GetText())

        ucrBase.clsRsyntax.SetBaseRFunction(clsFctRecodeFunction)
    End Sub

    Private Sub SetRCodeforControls(bReset As Boolean)
        ucrReceiverFactor.AddAdditionalCodeParameterPair(clsFctOtherFunction, New RParameter("f", 0), iAdditionalPairNo:=1)
        ucrReceiverFactor.AddAdditionalCodeParameterPair(clsFctLumpPropFunction, New RParameter("f", 0), iAdditionalPairNo:=2)
        ucrReceiverFactor.AddAdditionalCodeParameterPair(clsFctLowFreqFunction, New RParameter("f", 0), iAdditionalPairNo:=3)
        ucrSaveNewColumn.AddAdditionalRCode(clsFctLumpPropFunction, iAdditionalPairNo:=1)
        ucrSaveNewColumn.AddAdditionalRCode(clsFctLowFreqFunction, iAdditionalPairNo:=2)
        ucrSaveNewColumn.AddAdditionalRCode(clsFctOtherFunction, bReset)
        ucrReceiverFactor.SetRCode(clsFctRecodeFunction, bReset)
        ucrSaveNewColumn.SetRCode(clsFctRecodeFunction, bReset)
        ucrNudFrequentValues.SetRCode(clsFctLumpPropFunction, bReset)
    End Sub

    Private Sub TestOKEnabled()
        If Not ucrReceiverFactor.IsEmpty AndAlso ucrSaveNewColumn.IsComplete AndAlso ucrFactorGrid.IsColumnComplete("New Label") Then
            ucrBase.OKEnabled(True)
        Else
            ucrBase.OKEnabled(False)
        End If
    End Sub

    Private Sub ucrBase_ClickReset(sender As Object, e As EventArgs) Handles ucrBase.ClickReset
        SetDefaults()
        SetRCodeforControls(True)
        TestOKEnabled()
    End Sub

    Private Sub DefaultNewName()
        If ((Not ucrSaveNewColumn.bUserTyped) AndAlso (Not ucrReceiverFactor.IsEmpty)) Then
            ucrSaveNewColumn.SetPrefix(ucrReceiverFactor.GetVariableNames(bWithQuotes:=False) & "_recoded")
        End If
    End Sub

    Private Sub ucrFactorGrid_GridContentChanged() Handles ucrFactorGrid.GridContentChanged
        Dim strCurrentLabels As List(Of String)
        Dim strNewLabels As List(Of String)

        strCurrentLabels = ucrFactorGrid.GetColumnAsList(ucrFactorGrid.strLabelsName, False)
        strNewLabels = ucrFactorGrid.GetColumnAsList("New Label", True)

        If ucrFactorGrid.IsColumnComplete("New Label") AndAlso strCurrentLabels.Count = strNewLabels.Count Then
            For i = 0 To strCurrentLabels.Count - 1
                ' Backtick needed for names of the vector incase the levels are not valid R names
                clsFctRecodeFunction.AddParameter(Chr(96) & strCurrentLabels(i) & Chr(96), strNewLabels(i))
                clsCFunction.AddParameter(Chr(96) & strCurrentLabels(i) & Chr(96), strNewLabels(i))
            Next
            'ucrBase.clsRsyntax.AddParameter("replace", clsRFunctionParameter:=clsReplaceFunction)
            ' Else
            '    ucrBase.clsRsyntax.RemoveParameter("replace")
        End If
        TestOKEnabled()
    End Sub

    Private Sub ucrPnlOptions_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrPnlOptions.ControlValueChanged, ucrPnlMethods.ControlValueChanged, ucrPnlKeep.ControlValueChanged, ucrInputOther.ControlValueChanged
        If rdoRecode.Checked Then
            ucrBase.clsRsyntax.SetBaseRFunction(clsFctRecodeFunction)
        ElseIf rdoOther.Checked OrElse rdoLump.Checked Then
            If Not ucrInputOther.bUserTyped OrElse ucrInputOther.GetText = "Other" Then
                clsFctOtherFunction.AddParameter("other_level", Chr(34) & "Other" & Chr(34), iPosition:=2)
                clsFctLowFreqFunction.AddParameter("other_level", Chr(34) & "Other" & Chr(34), iPosition:=2)
                clsFctLumpPropFunction.AddParameter("other_level", Chr(34) & "Other" & Chr(34), iPosition:=2)
            Else
                clsFctOtherFunction.AddParameter("other_level", Chr(34) & ucrInputOther.GetText() & Chr(34), iPosition:=2)
                clsFctLowFreqFunction.AddParameter("other_level", Chr(34) & ucrInputOther.GetText() & Chr(34), iPosition:=2)
                clsFctLumpPropFunction.AddParameter("other_level", Chr(34) & ucrInputOther.GetText() & Chr(34), iPosition:=2)
            End If
            If rdoOther.Checked Then
                clsFctOtherFunction.RemoveParameterByName("keep")
                clsFctOtherFunction.RemoveParameterByName("drop")
                If rdoKeep.Checked Then
                    clsFctOtherFunction.AddParameter("keep", clsRFunctionParameter:=clsCFunction, iPosition:=1)
                Else
                    clsFctOtherFunction.AddParameter("drop", clsRFunctionParameter:=clsCFunction, iPosition:=1)
                End If
                ucrBase.clsRsyntax.SetBaseRFunction(clsFctOtherFunction)
            ElseIf rdoLump.Checked Then
                If rdoFrequentValues.Checked Then
                    ucrBase.clsRsyntax.SetBaseRFunction(clsFctLumpPropFunction)
                ElseIf rdoMore.Checked Then
                    ucrBase.clsRsyntax.SetBaseRFunction(clsFctLowFreqFunction)
                End If
            End If
        End If
    End Sub

    Private Sub ucrReceiverFactor_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrReceiverFactor.ControlValueChanged
        DefaultNewName()
    End Sub

    Private Sub ucrReceiverFactor_ControlContentsChanged(ucrChangedControl As ucrCore) Handles ucrReceiverFactor.ControlContentsChanged, ucrSaveNewColumn.ControlContentsChanged
        TestOKEnabled()
    End Sub
End Class