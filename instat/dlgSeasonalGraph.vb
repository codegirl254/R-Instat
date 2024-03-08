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

Imports instat.Translations

Public Class dlgSeasonalGraph
    Private clsRaesFunction As New RFunction
    Private clsBaseOperator As New ROperator
    Private bFirstLoad As Boolean = True
    Private bReset As Boolean = True
    Private bResetSubdialog As Boolean = True
    Private clsRggplotFunction As New RFunction
    Private clsDummyFunction As New RFunction
    Private clsFacetFunction As New RFunction
    Private clsFacetOperator As New ROperator
    Private clsFacetRowOp As New ROperator
    Private clsFacetColOp As New ROperator
    Private clsGroupByFunction As New RFunction
    Private clsPipeOperator As New ROperator
    Private clsThemeFunction As New RFunction
    Private clsNumericFunction As New RFunction
    Private clsLabsFunction As New RFunction
    Private clsXlabsFunction As New RFunction
    Private clsYlabFunction As New RFunction
    Private clsXScalecontinuousFunction As New RFunction
    Private clsYScalecontinuousFunction As New RFunction
    Private clsRFacetFunction As New RFunction
    Private dctThemeFunctions As New Dictionary(Of String, RFunction)
    Private clsCoordPolarFunction As New RFunction
    Private clsCoordPolarStartOperator As New ROperator
    Private clsXScaleDateFunction As New RFunction
    Private clsYScaleDateFunction As New RFunction
    Private clsScaleFillViridisFunction As New RFunction
    Private clsScaleColourViridisFunction As New RFunction
    Private clsAnnotateFunction As New RFunction
    Private clsScalefillidentityFunction As New RFunction
    Private clsScalecolouridentityFunction As New RFunction
    Private bUpdatingParameters As Boolean = False
    Private ReadOnly strFacetWrap As String = "Facet Wrap"
    Private ReadOnly strFacetRow As String = "Facet Row"
    Private ReadOnly strFacetCol As String = "Facet Column"
    Private ReadOnly strNone As String = "None"
    Private bUpdateComboOptions As Boolean = True

    'Parameter names for geoms
    Private strFirstParameterName As String = "geomLine"
    Private strGeompointParameterName As String = "geom_point"
    Private strgeomRibbonParameterName As String = "geom_ribbon"
    Private strgeomRibbonParameterName0 As String = "geom_ribbon"
    Private strGeomParameterNames() As String = {strFirstParameterName, strgeomRibbonParameterName, strgeomRibbonParameterName0, strGeompointParameterName}

    Private Sub dlgSeasonalGraph_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If bFirstLoad Then
            InitialiseDialog()
            bFirstLoad = False
        End If
        If bReset Then
            SetDefaults()
        End If
        SetRCodeForControls(bReset)
        bReset = False
        autoTranslate(Me)
        TestOkEnabled()
    End Sub

    Private Sub InitialiseDialog()
        Dim dctLegendPosition As New Dictionary(Of String, String)
        Dim dctColour As New Dictionary(Of String, String)

        ucrBase.iHelpTopicID = 522

        ucrSelectorForSeasonalGraph.SetParameter(New RParameter("data", 0))
        ucrSelectorForSeasonalGraph.SetParameterIsrfunction()

        ucrPnlOptions.AddRadioButton(rdoLine)
        ucrPnlOptions.AddRadioButton(rdoBar)

        ucrPnlOptions.AddParameterValuesCondition(rdoLine, "checked", "geom_line")
        ucrPnlOptions.AddParameterValuesCondition(rdoBar, "checked", "geom_Bar")

        ucrReceiverLines.Selector = ucrSelectorForSeasonalGraph
        ucrReceiverLines.strSelectorHeading = "Variables"
        ucrReceiverLines.SetParameterIsString()
        ucrReceiverLines.SetLinkedDisplayControl(lblLines)
        ucrReceiverLines.bWithQuotes = False

        ucrReceiverX.Selector = ucrSelectorForSeasonalGraph
        ucrReceiverX.SetParameter(New RParameter("x", 2))
        ucrReceiverX.SetParameterIsString()
        ucrReceiverX.bWithQuotes = False
        ucrReceiverX.strSelectorHeading = "Variables"
        ucrReceiverX.SetLinkedDisplayControl(lblXvariable)

        ucrChkRibbons.SetText("Ribbon(s):")
        ucrChkRibbons.AddParameterPresentCondition(True, "geom_ribbon")
        ucrChkRibbons.AddParameterPresentCondition(False, "geom_ribbon", False)
        ucrChkRibbons.AddToLinkedControls(ucrReceiverRibbons, {True}, bNewLinkedAddRemoveParameter:=True, bNewLinkedHideIfParameterMissing:=True)

        ucrReceiverRibbons.SetParameter(New RParameter("y", 1))
        ucrReceiverRibbons.Selector = ucrSelectorForSeasonalGraph
        ucrReceiverRibbons.strSelectorHeading = "Variables"
        ucrReceiverRibbons.SetParameterIsString()
        ucrReceiverRibbons.bWithQuotes = False
        ucrReceiverRibbons.iMaxItems = 4

        ucrReceiverFacetBy.SetParameter(New RParameter(""))
        ucrReceiverFacetBy.Selector = ucrSelectorForSeasonalGraph
        ucrReceiverFacetBy.SetIncludedDataTypes({"factor"})
        ucrReceiverFacetBy.strSelectorHeading = "Factors"
        ucrReceiverFacetBy.bWithQuotes = False
        ucrReceiverFacetBy.SetParameterIsString()
        ucrReceiverFacetBy.SetValuesToIgnore({"."})

        ucrInputStation.SetItems({strFacetWrap, strFacetRow, strFacetCol, strNone})
        ucrInputStation.SetDropDownStyleAsNonEditable()

        ucrChkLegend.SetText("Legend:")
        ucrChkLegend.AddToLinkedControls({ucrInputLegendPosition}, {True}, bNewLinkedAddRemoveParameter:=True, bNewLinkedHideIfParameterMissing:=True, bNewLinkedChangeToDefaultState:=True, objNewDefaultState:="None")
        ucrInputLegendPosition.SetDropDownStyleAsNonEditable()
        ucrInputLegendPosition.SetParameter(New RParameter("legend.position"))
        dctLegendPosition.Add("None", Chr(34) & "none" & Chr(34))
        dctLegendPosition.Add("Left", Chr(34) & "left" & Chr(34))
        dctLegendPosition.Add("Right", Chr(34) & "right" & Chr(34))
        dctLegendPosition.Add("Top", Chr(34) & "top" & Chr(34))
        dctLegendPosition.Add("Bottom", Chr(34) & "bottom" & Chr(34))
        ucrInputLegendPosition.SetItems(dctLegendPosition)
        ucrChkLegend.AddParameterPresentCondition(True, "legend.position")
        ucrChkLegend.AddParameterPresentCondition(False, "legend.position", False)

        ucrChkAddPoint.SetText("Add point")
        ucrChkAddPoint.AddParameterValuesCondition(True, "checked", "True")
        ucrChkAddPoint.AddParameterValuesCondition(False, "checked", "False")
        ucrChkAddPoint.AddToLinkedControls(ucrReceiverAddPoint, {True}, bNewLinkedHideIfParameterMissing:=True)

        ucrReceiverAddPoint.SetParameter(New RParameter("y", 6))
        ucrReceiverAddPoint.Selector = ucrSelectorForSeasonalGraph
        ucrReceiverAddPoint.strSelectorHeading = "Variables"
        ucrReceiverAddPoint.SetParameterIsString()
        ucrReceiverAddPoint.bWithQuotes = False

        ucrChkFill.SetText("Fill Identity:")
        ucrChkFill.AddParameterValuesCondition(True, "checked", "True")
        ucrChkFill.AddParameterValuesCondition(False, "checked", "False")
        ucrChkFill.AddToLinkedControls(ucrInputFill, {True}, bNewLinkedHideIfParameterMissing:=True)
        ucrInputFill.SetValidationTypeAsList()
        ucrInputFill.SetLinkedDisplayControl(lblFill)

        ucrChkColour.SetText("Colour Identity:")
        ucrChkColour.AddParameterValuesCondition(True, "checked", "True")
        ucrChkColour.AddParameterValuesCondition(False, "checked", "False")
        ucrChkColour.AddToLinkedControls(ucrInputColour, {True}, bNewLinkedHideIfParameterMissing:=True)
        ucrInputColour.SetValidationTypeAsList()
        ucrInputColour.SetLinkedDisplayControl(lblColour)

        ucrSave.SetPrefix("Seasonal_Graph")
        ucrSave.SetIsComboBox()
        ucrSave.SetSaveTypeAsGraph()
        ucrSave.SetCheckBoxText("Save Graph")
        ucrSave.SetDataFrameSelector(ucrSelectorForSeasonalGraph.ucrAvailableDataFrames)
        ucrSave.SetAssignToIfUncheckedValue("last_graph")
    End Sub

    Private Sub SetDefaults()
        clsRggplotFunction = New RFunction
        clsRaesFunction = New RFunction
        clsBaseOperator = New ROperator
        clsDummyFunction = New RFunction
        clsFacetFunction = New RFunction
        clsScalecolouridentityFunction = New RFunction
        clsScalefillidentityFunction = New RFunction
        clsNumericFunction = New RFunction
        clsFacetOperator = New ROperator
        clsFacetRowOp = New ROperator
        clsFacetColOp = New ROperator
        clsPipeOperator = New ROperator

        clsThemeFunction = GgplotDefaults.clsDefaultThemeFunction

        ucrSelectorForSeasonalGraph.Reset()
        ucrSelectorForSeasonalGraph.SetGgplotFunction(clsBaseOperator)
        ucrSave.Reset()
        ucrReceiverLines.SetMeAsReceiver()
        bResetSubdialog = True
        clsDummyFunction.AddParameter("checked", "geom_line", iPosition:=0)

        clsBaseOperator.SetOperation("+")
        clsBaseOperator.AddParameter("ggplot", clsRFunctionParameter:=clsRggplotFunction, iPosition:=0)

        clsRggplotFunction.SetPackageName("ggplot2")
        clsRggplotFunction.SetRCommand("ggplot")
        clsRggplotFunction.AddParameter("mapping", clsRFunctionParameter:=clsRaesFunction, iPosition:=1)

        clsRaesFunction.SetPackageName("ggplot2")
        clsRaesFunction.SetRCommand("aes")

        clsScalecolouridentityFunction.SetRCommand("scale_colour_identity")
        clsScalecolouridentityFunction.AddParameter("name", "NULL", iPosition:=0)
        clsScalecolouridentityFunction.AddParameter("guide", Chr(34) & "legend" & Chr(34), iPosition:=1)

        clsScalefillidentityFunction.SetRCommand("scale_fill_identity")
        clsScalefillidentityFunction.AddParameter("name", "NULL", iPosition:=0)
        clsScalefillidentityFunction.AddParameter("guide", Chr(34) & "legend" & Chr(34), iPosition:=1)

        clsFacetFunction.SetPackageName("ggplot2")
        clsFacetRowOp.SetOperation("+")
        clsFacetRowOp.bBrackets = False
        clsFacetColOp.SetOperation("+")
        clsFacetColOp.bBrackets = False
        clsFacetOperator.SetOperation("~")
        clsFacetOperator.bForceIncludeOperation = True
        clsFacetOperator.bBrackets = False
        clsFacetFunction.AddParameter("facets", clsROperatorParameter:=clsFacetOperator, iPosition:=0)

        clsNumericFunction.SetRCommand("as.numeric")

        clsPipeOperator.SetOperation("%>%")
        SetPipeAssignTo()

        ucrInputStation.SetName(strFacetWrap)
        ucrInputStation.bUpdateRCodeFromControl = True

        clsGroupByFunction.SetPackageName("dplyr")
        clsGroupByFunction.SetRCommand("group_by")

        clsThemeFunction = GgplotDefaults.clsDefaultThemeFunction
        clsXlabsFunction = GgplotDefaults.clsXlabTitleFunction.Clone()
        clsYlabFunction = GgplotDefaults.clsYlabTitleFunction.Clone
        clsLabsFunction = GgplotDefaults.clsDefaultLabs.Clone()
        clsXScalecontinuousFunction = GgplotDefaults.clsXScalecontinuousFunction.Clone()
        clsYScalecontinuousFunction = GgplotDefaults.clsYScalecontinuousFunction.Clone()
        clsRFacetFunction = GgplotDefaults.clsFacetFunction.Clone()
        clsCoordPolarStartOperator = GgplotDefaults.clsCoordPolarStartOperator.Clone()
        clsCoordPolarFunction = GgplotDefaults.clsCoordPolarFunction.Clone()
        dctThemeFunctions = New Dictionary(Of String, RFunction)(GgplotDefaults.dctThemeFunctions)
        clsXScaleDateFunction = GgplotDefaults.clsXScaleDateFunction.Clone()
        clsYScaleDateFunction = GgplotDefaults.clsYScaleDateFunction.Clone()
        clsScaleFillViridisFunction = GgplotDefaults.clsScaleFillViridisFunction
        clsScaleColourViridisFunction = GgplotDefaults.clsScaleColorViridisFunction
        clsAnnotateFunction = GgplotDefaults.clsAnnotateFunction

        clsBaseOperator.SetAssignTo("last_graph", strTempDataframe:=ucrSelectorForSeasonalGraph.ucrAvailableDataFrames.cboAvailableDataFrames.Text, strTempGraph:="last_graph")
        ucrBase.clsRsyntax.SetBaseROperator(clsBaseOperator)
    End Sub

    Private Sub SetRCodeForControls(bReset As Boolean)
        ucrReceiverX.AddAdditionalCodeParameterPair(clsNumericFunction, New RParameter("x", 2), iAdditionalPairNo:=1)

        ucrPnlOptions.SetRCode(clsDummyFunction, bReset)
        ucrSelectorForSeasonalGraph.SetRCode(clsRggplotFunction, bReset)
        ucrSave.SetRCode(clsBaseOperator, bReset)
        ucrChkLegend.SetRCode(clsThemeFunction, bReset, bCloneIfNeeded:=True)
        ucrInputLegendPosition.SetRCode(clsThemeFunction, bReset, bCloneIfNeeded:=True)
        If bReset Then
            ucrReceiverX.SetRCode(clsRaesFunction, bReset)
            ucrChkColour.SetRCode(clsScalecolouridentityFunction, bReset)
            ucrInputColour.SetRCode(clsScalecolouridentityFunction, bReset)
            ucrChkFill.SetRCode(clsScalefillidentityFunction, bReset)
            ucrInputFill.SetRCode(clsScalefillidentityFunction, bReset)
            ucrChkRibbons.SetRCode(clsBaseOperator, bReset)
            ucrChkAddPoint.SetRCode(clsBaseOperator, bReset)
            AutoFacetStation()
            AutoFillmonth()
        End If
    End Sub

    Private Sub TestOkEnabled()
        If Not ucrSave.IsComplete OrElse (ucrReceiverLines.IsEmpty AndAlso Not ucrChkRibbons.Checked) Then
            ucrBase.OKEnabled(False)
        ElseIf Not ucrSave.IsComplete OrElse (ucrReceiverRibbons.IsEmpty AndAlso ucrChkRibbons.Checked) Then
            ucrBase.OKEnabled(False)
        ElseIf ucrChkRibbons.Checked AndAlso ucrReceiverRibbons.lstSelectedVariables.Items.Count = 1 Then
            ucrBase.OKEnabled(False)
        ElseIf ucrChkRibbons.Checked AndAlso ucrReceiverRibbons.lstSelectedVariables.Items.Count = 3 Then
            ucrBase.OKEnabled(False)
        ElseIf ucrReceiverX.IsEmpty Then
            ucrBase.OKEnabled(False)
        Else
            ucrBase.OKEnabled(True)
        End If
    End Sub

    Private Sub ListGeomLine()
        ' Clear parameters before adding new ones
        clsBaseOperator.ClearParameters()

        Dim i As Integer

        ' Add geom_line functions for ucrReceiverLines.lstSelectedVariables
        For i = 0 To ucrReceiverLines.lstSelectedVariables.Items.Count - 1
            Dim clsRaeslineFunction As New RFunction
            Dim ColourArguments As New List(Of String) From {"grey", "grey", "black", "grey"}
            Dim LinewidthArguments As New List(Of String) From {"1.0", "1.0", "2.0", "1.0"}
            Dim GroupArguments As New List(Of String) From {"grey", "grey", "grey", "grey"}

            clsRaeslineFunction.SetPackageName("ggplot2")
            clsRaeslineFunction.SetRCommand("aes")
            clsRaeslineFunction.AddParameter("y", ucrReceiverLines.lstSelectedVariables.Items(i).Text, iPosition:=0)
            clsRaeslineFunction.AddParameter("colour", Chr(34) & ColourArguments(i) & Chr(34), iPosition:=1)

            Dim clsGeomLineFunction As New RFunction
            clsGeomLineFunction.SetPackageName("ggplot2")
            clsGeomLineFunction.SetRCommand("geom_line")
            clsGeomLineFunction.AddParameter("mapping", clsRFunctionParameter:=clsRaeslineFunction, iPosition:=1)
            clsGeomLineFunction.AddParameter("linewidth", LinewidthArguments(i), iPosition:=2)
            clsGeomLineFunction.AddParameter("group", Chr(34) & GroupArguments(i) & Chr(34), iPosition:=3)

            ' Add geom_line function to the base operator
            clsBaseOperator.AddParameter(GgplotDefaults.clsDefaultThemeParameter.Clone())
            clsBaseOperator.AddParameter("ggplot", clsRFunctionParameter:=clsRggplotFunction, iPosition:=0)
            clsBaseOperator.AddParameter(strFirstParameterName & i, clsRFunctionParameter:=clsGeomLineFunction, iPosition:=3)
        Next

        ' Add geom_point functions for ucrReceiverAddPoint.lstSelectedVariables
        For i = 0 To ucrReceiverAddPoint.lstSelectedVariables.Items.Count - 1
            Dim clsAesGeompointFunction As New RFunction
            clsAesGeompointFunction.SetPackageName("ggplot2")
            clsAesGeompointFunction.SetRCommand("aes")
            clsAesGeompointFunction.AddParameter("y", ucrReceiverAddPoint.lstSelectedVariables.Items(i).Text, iPosition:=0)

            Dim clsGeomPointFunction As New RFunction
            clsGeomPointFunction.SetPackageName("ggplot2")
            clsGeomPointFunction.SetRCommand("geom_point")
            clsGeomPointFunction.AddParameter("mapping", clsRFunctionParameter:=clsAesGeompointFunction, iPosition:=1)

            ' Add geom_point function to the base operator
            clsBaseOperator.AddParameter(strGeompointParameterName & i, clsRFunctionParameter:=clsGeomPointFunction, iPosition:=8)
        Next

        ' Add geom_ribbon functions for ucrReceiverRibbons.lstSelectedVariables
        For i = 0 To ucrReceiverRibbons.lstSelectedVariables.Items.Count - 1 Step 2
            ' Get current variable
            Dim var1 = ucrReceiverRibbons.lstSelectedVariables.Items(i).Text
            ' Get the next variable in the pair if available
            Dim var2 As String = If(i + 1 < ucrReceiverRibbons.lstSelectedVariables.Items.Count, ucrReceiverRibbons.lstSelectedVariables.Items(i + 1).Text, "")
            Dim fillArguments As New List(Of String) From {"#3270C1", "#A3A3A3", "#4DB0F1", "#FF5733"}

            If Not String.IsNullOrEmpty(var2) Then
                Dim clsRaesRibFunction As New RFunction
                clsRaesRibFunction.SetPackageName("ggplot2")
                clsRaesRibFunction.SetRCommand("aes")
                clsRaesRibFunction.AddParameter("ymax", var1, iPosition:=0)
                clsRaesRibFunction.AddParameter("ymin", var2, iPosition:=1)
                clsRaesRibFunction.AddParameter("fill", Chr(34) & fillArguments(i) & Chr(34), iPosition:=2)

                Dim clsRibFunction As New RFunction
                clsRibFunction.SetPackageName("ggplot2")
                clsRibFunction.SetRCommand("geom_ribbon")
                clsRibFunction.AddParameter("mapping", clsRFunctionParameter:=clsRaesRibFunction, iPosition:=1)

                clsBaseOperator.AddParameter(strgeomRibbonParameterName0 & i, clsRFunctionParameter:=clsRibFunction, iPosition:=1)
            End If
        Next
        AddRemoveFacets()
        AddRemoveGroupBy()
    End Sub


    Private Sub UpdateParameters()
        clsFacetOperator.RemoveParameterByName("wrap" & ucrInputStation.Name)
        clsFacetColOp.RemoveParameterByName("col" & ucrInputStation.Name)
        clsFacetRowOp.RemoveParameterByName("row" & ucrInputStation.Name)

        clsBaseOperator.RemoveParameterByName("facets")
        bUpdatingParameters = True
        ucrReceiverFacetBy.SetRCode(Nothing)
        Select Case ucrInputStation.GetText()
            Case strFacetWrap
                ucrReceiverFacetBy.ChangeParameterName("wrap" & ucrInputStation.Name)
                ucrReceiverFacetBy.SetRCode(clsFacetOperator)
            Case strFacetCol
                ucrReceiverFacetBy.ChangeParameterName("col" & ucrInputStation.Name)
                ucrReceiverFacetBy.SetRCode(clsFacetColOp)
            Case strFacetRow
                ucrReceiverFacetBy.ChangeParameterName("row" & ucrInputStation.Name)
                ucrReceiverFacetBy.SetRCode(clsFacetRowOp)
        End Select
        If Not clsRaesFunction.ContainsParameter("x") Then
            clsRaesFunction.AddParameter("x", Chr(34) & Chr(34), iPosition:=1)
        End If
        bUpdatingParameters = False
    End Sub

    Private Sub AddRemoveFacets()
        Dim bWrap As Boolean = False
        Dim bCol As Boolean = False
        Dim bRow As Boolean = False

        If bUpdatingParameters Then
            Exit Sub
        End If

        clsBaseOperator.RemoveParameterByName("facets")
        If Not ucrReceiverFacetBy.IsEmpty Then
            Select Case ucrInputStation.GetText()
                Case strFacetWrap
                    bWrap = True
                Case strFacetCol
                    bCol = True
                Case strFacetRow
                    bRow = True
            End Select
        End If

        If bWrap OrElse bRow OrElse bCol Then
            clsBaseOperator.AddParameter("facets", clsRFunctionParameter:=clsFacetFunction)
        End If
        If bWrap Then
            clsFacetFunction.SetRCommand("facet_wrap")
        End If
        If bRow OrElse bCol Then
            clsFacetFunction.SetRCommand("facet_grid")
        End If
        If bRow Then
            clsFacetOperator.AddParameter("left", clsROperatorParameter:=clsFacetRowOp, iPosition:=0)
        ElseIf bCol AndAlso bWrap = False Then
            clsFacetOperator.AddParameter("left", ".", iPosition:=0)
        Else
            clsFacetOperator.RemoveParameterByName("left")
        End If
        If bCol Then
            clsFacetOperator.AddParameter("right", clsROperatorParameter:=clsFacetColOp, iPosition:=1)
        ElseIf bRow AndAlso bWrap = False Then
            clsFacetOperator.AddParameter("right", ".", iPosition:=1)
        Else
            clsFacetOperator.RemoveParameterByName("right")
        End If
    End Sub

    Private Sub AddRemoveGroupBy()
        If clsPipeOperator.ContainsParameter("mutate") Then
            clsGroupByFunction.ClearParameters()
            If clsBaseOperator.ContainsParameter("facets") Then
                Select Case ucrInputStation.GetText()
                    Case strFacetWrap
                        GetParameterValue(clsFacetOperator)
                    Case strFacetCol
                        GetParameterValue(clsFacetColOp)
                    Case strFacetRow
                        GetParameterValue(clsFacetRowOp)
                End Select
            End If
            If clsGroupByFunction.iParameterCount > 0 Then
                clsPipeOperator.AddParameter("group_by", clsRFunctionParameter:=clsGroupByFunction, iPosition:=1)
            Else
                clsPipeOperator.RemoveParameterByName("group_by")
            End If
        Else
            clsPipeOperator.RemoveParameterByName("group_by")
        End If
        SetPipeAssignTo()
    End Sub

    Private Sub ucrInput_ControlValueChanged(ucrChangedControl As ucrInputComboBox) Handles ucrInputStation.ControlValueChanged
        If Not bUpdateComboOptions Then
            Exit Sub
        End If
        Dim strChangedText As String = ucrChangedControl.GetText()
        If strChangedText <> strNone Then
            If Not strChangedText = strFacetCol AndAlso Not strChangedText = strFacetRow AndAlso
                    Not ucrInputStation.Equals(ucrChangedControl) AndAlso ucrInputStation.GetText() = strChangedText Then
                bUpdateComboOptions = False
                ucrInputStation.SetName(strNone)
                bUpdateComboOptions = True
            End If
            If (strChangedText = strFacetWrap AndAlso ucrInputStation.GetText = strFacetRow) OrElse (strChangedText = strFacetRow AndAlso
                    ucrInputStation.GetText = strFacetWrap) OrElse (strChangedText = strFacetWrap AndAlso
                    ucrInputStation.GetText = strFacetCol) OrElse (strChangedText = strFacetCol AndAlso ucrInputStation.GetText = strFacetWrap) Then
                ucrInputStation.SetName(strNone)
            End If
        End If
        UpdateParameters()
        AddRemoveFacets()
        AddRemoveGroupBy()
    End Sub

    Private Sub GetParameterValue(clsOperator As ROperator)
        Dim i As Integer = 0
        For Each clsTempParam As RParameter In clsOperator.clsParameters
            If clsTempParam.strArgumentValue <> "" AndAlso clsTempParam.strArgumentValue <> "." Then
                clsGroupByFunction.AddParameter(i, clsTempParam.strArgumentValue, bIncludeArgumentName:=False, iPosition:=i)
                i = i + 1
            End If
        Next
    End Sub

    Private Sub SetPipeAssignTo()
        If ucrSelectorForSeasonalGraph.ucrAvailableDataFrames.cboAvailableDataFrames.Text <> "" AndAlso clsPipeOperator.clsParameters.Count > 1 Then
            clsPipeOperator.SetAssignTo(ucrSelectorForSeasonalGraph.ucrAvailableDataFrames.cboAvailableDataFrames.Text)
        Else
            clsPipeOperator.RemoveAssignTo()
        End If
    End Sub

    Private Sub ucrReceiverFacetBy_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrReceiverFacetBy.ControlValueChanged
        AddRemoveFacets()
        AddRemoveGroupBy()
    End Sub

    Private Sub AutoFacetStation()
        Dim ucrCurrentReceiver As ucrReceiver = Nothing

        If ucrSelectorForSeasonalGraph.CurrentReceiver IsNot Nothing Then
            ucrCurrentReceiver = ucrSelectorForSeasonalGraph.CurrentReceiver
        End If
        ucrReceiverFacetBy.AddItemsWithMetadataProperty(ucrSelectorForSeasonalGraph.ucrAvailableDataFrames.cboAvailableDataFrames.Text, "Climatic_Type", {"station_label"})
        If ucrCurrentReceiver IsNot Nothing Then
            ucrCurrentReceiver.SetMeAsReceiver()
        End If
        AddRemoveGroupBy()
    End Sub

    Private Sub AutoFillmonth()
        Dim ucrCurrentReceiver0 As ucrReceiver = Nothing

        If ucrSelectorForSeasonalGraph.CurrentReceiver IsNot Nothing Then
            ucrCurrentReceiver0 = ucrSelectorForSeasonalGraph.CurrentReceiver
        End If
        ucrReceiverX.AddItemsWithMetadataProperty(ucrSelectorForSeasonalGraph.ucrAvailableDataFrames.cboAvailableDataFrames.Text, "Climatic_Type", {"month_label"})
        If ucrCurrentReceiver0 IsNot Nothing Then
            ucrCurrentReceiver0.SetMeAsReceiver()
        End If
    End Sub

    Private Sub ucrReceiverLines_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrReceiverLines.ControlValueChanged
        ListGeomLine()
    End Sub

    Private Sub AddRemoveTheme()
        If clsThemeFunction.iParameterCount > 0 Then
            clsBaseOperator.AddParameter("theme", clsRFunctionParameter:=clsThemeFunction, iPosition:=15)
        Else
            clsBaseOperator.RemoveParameterByName("theme")
        End If
    End Sub

    Private Sub ucrChkLegend_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrChkLegend.ControlValueChanged, ucrInputLegendPosition.ControlValueChanged
        AddRemoveTheme()
    End Sub

    Private Sub ucrSelectorForSeasonalGraph_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrSelectorForSeasonalGraph.ControlValueChanged
        AutoFacetStation()
        SetPipeAssignTo()
        AutoFillmonth()
    End Sub

    Private Sub ucrBase_Click(sender As Object, e As EventArgs) Handles ucrBase.ClickReset
        SetDefaults()
        SetRCodeForControls(True)
        TestOkEnabled()
    End Sub

    Private Sub cmdOptions_Click(sender As Object, e As EventArgs) Handles cmdOptions.Click ', PlotOptionsToolStripMenuItem.Click
        sdgPlots.SetRCode(clsNewOperator:=ucrBase.clsRsyntax.clsBaseOperator, clsNewYScalecontinuousFunction:=clsYScalecontinuousFunction, clsNewXScalecontinuousFunction:=clsXScalecontinuousFunction,
                                clsNewXLabsTitleFunction:=clsXlabsFunction, clsNewYLabTitleFunction:=clsYlabFunction, clsNewLabsFunction:=clsLabsFunction, clsNewFacetFunction:=clsRFacetFunction,
                                clsNewThemeFunction:=clsThemeFunction, dctNewThemeFunctions:=dctThemeFunctions, clsNewGlobalAesFunction:=clsRaesFunction, ucrNewBaseSelector:=ucrSelectorForSeasonalGraph,
 clsNewCoordPolarFunction:=clsCoordPolarFunction, clsNewCoordPolarStartOperator:=clsCoordPolarStartOperator, clsNewXScaleDateFunction:=clsXScaleDateFunction, clsNewAnnotateFunction:=clsAnnotateFunction,
        clsNewScaleFillViridisFunction:=clsScaleFillViridisFunction, clsNewScaleColourViridisFunction:=clsScaleColourViridisFunction, clsNewYScaleDateFunction:=clsYScaleDateFunction,
                                strMainDialogGeomParameterNames:=strGeomParameterNames, bReset:=bResetSubdialog)
        sdgPlots.ShowDialog()
        bResetSubdialog = False
    End Sub

    Private Sub ucrChkRibbons_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrChkRibbons.ControlValueChanged, ucrReceiverRibbons.ControlValueChanged
        ListGeomLine()
        If ucrChkRibbons.Checked Then
            ucrReceiverRibbons.SetMeAsReceiver()
        Else
            clsBaseOperator.RemoveParameterByName(strgeomRibbonParameterName0)
        End If
    End Sub

    Private Sub AllControl_ControlContentsChanged() Handles ucrReceiverX.ControlContentsChanged, ucrReceiverRibbons.ControlContentsChanged, ucrReceiverLines.ControlContentsChanged, ucrSave.ControlContentsChanged, ucrReceiverAddPoint.ControlContentsChanged
        TestOkEnabled()
    End Sub

    Private Sub ucrReceiverX_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrReceiverX.ControlValueChanged
        Dim variableNames As String = String.Join(",", ucrReceiverX.GetVariableNames())
        ' Concatenate the variable names with commas
        variableNames = variableNames.Replace("'", "").Replace("""", "")

        If Not ucrReceiverX.IsEmpty AndAlso (ucrReceiverX.strCurrDataType.Contains("factor") OrElse ucrReceiverX.strCurrDataType = "ordered,factor") Then
            clsNumericFunction.AddParameter("x", variableNames, iPosition:=0, bIncludeArgumentName:=False)
            clsRaesFunction.AddParameter("x", clsRFunctionParameter:=clsNumericFunction)
            clsRaesFunction.AddParameter("group", "1", iPosition:=3)
        Else
            clsRaesFunction.RemoveParameterByName("group")
            'clsRaesFunction.AddParameter("x", ucrReceiverX.GetVariableNames().Trim())
            clsRaesFunction.AddParameter("x", variableNames)
        End If
    End Sub

    Private Sub ucrChkAddPoint_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrChkAddPoint.ControlValueChanged, ucrReceiverAddPoint.ControlValueChanged
        If ucrChkAddPoint.Checked Then
            ucrReceiverAddPoint.SetMeAsReceiver()
        Else
            clsBaseOperator.RemoveParameterByName(strGeompointParameterName)
        End If
        ListGeomLine()
    End Sub

    Private Sub ucrChkFill_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrChkFill.ControlValueChanged, ucrChkColour.ControlValueChanged, ucrInputFill.ControlValueChanged, ucrInputColour.ControlValueChanged
        If ucrChkFill.Checked AndAlso Not ucrInputFill.IsEmpty Then
            clsScalefillidentityFunction.AddParameter("labels", ucrInputFill.clsRList.ToScript(), iPosition:=2)
            clsBaseOperator.AddParameter("scale_fill_identity", clsRFunctionParameter:=clsScalefillidentityFunction, iPosition:=12)
        Else
            clsBaseOperator.RemoveParameterByName("scale_fill_identity")
        End If
        If ucrChkColour.Checked AndAlso Not ucrInputColour.IsEmpty Then
            clsScalecolouridentityFunction.AddParameter("labels", ucrInputColour.clsRList.ToScript(), iPosition:=2)
            clsBaseOperator.AddParameter("scale_colour_identity", clsRFunctionParameter:=clsScalecolouridentityFunction, iPosition:=13)
        Else
            clsBaseOperator.RemoveParameterByName("scale_colour_identity")
        End If
    End Sub
End Class