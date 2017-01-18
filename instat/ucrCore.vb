﻿' Instat-R
' Copyright (C) 2015
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
' You should have received a copy of the GNU General Public License k
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports instat

Public Class ucrCore

    'Function or Operator that this control's parameter is added/removed from
    Protected clsRCode As New RCodeStructure
    Protected iParameterPosition As Integer = -1
    'Parameter that this control manages
    'Either by editing its value or adding/removing it from an RCodeStructure
    Protected clsParameter As New RParameter

    'Default value of the control
    'No specific type since it can be interpreted different by each control type
    Protected objDefault As Object = Nothing

    'Protected typControlType As Type = Object

    ''A control it's linked to i.e. dependant on/depends on 
    'Protected ucrLinkedControl As ucrCore
    ''The name of a parameter linked to the control which determines if the control is visible/enabled
    'Protected strLinkedParameterName As String

    'Sets what aspects of clsParameter this control can change
    'e.g. check box may not change parameter value, only add/remove it
    '     For this bAddRemoveParameter = True and bChangeParameterValue = False
    'e.g. nud may not add/remove parameter, only change its value
    Protected bAddRemoveParameter As Boolean = True
    Protected bChangeParameterValue As Boolean = True

    'Optional value
    'If parameter has this value then it will be removed from RCodeStructure 
    Public objValueToRemoveParameter As Object

    'ValueChanged is raised when a new value has been set in the control
    Public Event ControlValueChanged(ucrChangedControl As ucrCore)

    'ContentsChanged is raised when the content of the control has changed, but possibly the value has not been set
    'e.g. text in a textbox changes, but the value is not changed until the user leaves the text box
    'For some controls these two events will be equivalent
    'When ValueChanged is raised, so is ContentsChanged
    'ContentsChanged is probably only needed for TestOK
    Public Event ControlContentsChanged(ucrChangedControl As ucrCore)

    'List of controls that this control links to
    'Used when this control determines aspects of other controls
    'e.g. add/remove the parameter of other controls
    '     set the visible/enabled property of other controls
    'e.g. a checkbox that shows/hides set of controls
    Protected lstValuesAndControl As List(Of KeyValuePair(Of Object(), ucrCore))

    'If this control is in another controls lstLinkedControls
    'These values specifiy how that control can modify this control
    Public bLinkedAddRemoveParameter As Boolean = False
    Public bLinkedUpdateFunction As Boolean = False
    Public bLinkedDisabledIfParameterMissing As Boolean = False
    Public bLinkedHideIfParameterMissing As Boolean = False
    Public bLinkedChangeParameterToDefault As Boolean = False

    'Update the control based on the the code in RCodeStructure
    Public Overridable Sub UpdateControl()
        If clsRCode IsNot Nothing Then
            If Not clsRCode.ContainsParameter(clsParameter) Then
                If clsRCode.ContainsParameter(clsParameter.strArgumentName) Then
                    clsParameter = clsRCode.GetParameter(clsParameter.strArgumentName)
                End If
            End If
        Else
            clsRCode = New RCodeStructure
        End If
    End Sub

    Public Overridable Sub UpdateLinkedControls()
        Dim ucrControl As ucrCore
        Dim lstValues As Object()
        Dim bTemp As Boolean

        For Each kvpTemp As KeyValuePair(Of Object(), ucrCore) In lstValuesAndControl
            lstValues = kvpTemp.Key
            ucrControl = kvpTemp.Value
            bTemp = ValueContainedIn(lstValues)
            If ucrControl.bLinkedUpdateFunction AndAlso bTemp Then
                ucrControl.UpdateRCode(clsRCode)
            End If
            If ucrControl.bLinkedAddRemoveParameter Then
                ucrControl.AddOrRemoveParameter(bTemp)
            End If
            If ucrControl.bLinkedChangeParameterToDefault AndAlso bTemp Then
                ucrControl.SetToDefault()
            End If
            If ucrControl.bLinkedHideIfParameterMissing Then
                ucrControl.Visible = bTemp
            End If
            If ucrControl.bLinkedDisabledIfParameterMissing Then
                ucrControl.Enabled = bTemp
            End If
        Next
    End Sub

    'Update the RCode based on the contents of the control (reverse of above)
    Public Overridable Sub UpdateRCode(clsRCodeObject As RCodeStructure)
    End Sub

    Public Overridable Sub SetRCode(clsNewCodeStructure As RCodeStructure)
        If clsRCode Is Nothing OrElse Not clsRCode.Equals(clsNewCodeStructure) Then
            clsRCode = clsNewCodeStructure
            UpdateControl()
        End If
    End Sub

    Public Overridable Sub SetDefault(objNewDefault As Object)
        objDefault = objNewDefault
    End Sub

    Public Overridable Sub SetValueToRemoveParameter(objNewValue As Object)
        objValueToRemoveParameter = objNewValue
    End Sub

    Public Overridable Sub SetToDefault()
    End Sub

    ''Set a linked paramter name and what the control should do when the parameter is not in the R code
    'Public Sub SetLinkedParameterName(strNewLinkedParameterName As String, Optional bNewHideIfLinkedParameterMissing As Boolean = False, Optional bNewDisableIfLinkedParameterMissing As Boolean = False)
    '    strLinkedParameterName = strNewLinkedParameterName
    '    bHideIfParameterMissing = bNewHideIfLinkedParameterMissing
    '    bDisabledIfParameterMissing = bNewDisableIfLinkedParameterMissing
    'End Sub

    'Set the Text property of the control(s) inside this control (should only be one). Implemented different by each VB control.
    Public Overridable Sub SetText(strNewText As String)
        For Each ctrTemp In Controls
            ctrTemp.Text = strNewText
        Next
    End Sub

    Public Sub OnControlContentsChanged()
        RaiseEvent ControlContentsChanged(Me)
    End Sub

    Public Sub OnControlValueChanged()
        OnControlContentsChanged()
        RaiseEvent ControlValueChanged(Me)
    End Sub

    Public Property strParameterName As String
        Get
            If clsParameter IsNot Nothing Then
                Return clsParameter.strArgumentName
            Else
                Return ""
            End If
        End Get
        Set(bNewName As String)
            clsParameter.strArgumentName = bNewName
        End Set
    End Property

    Public Sub AddParameterToStructure(clsRCodeObject As RCodeStructure, Optional clsParm As RParameter = Nothing)
        If clsParm Is Nothing Then
            clsParm = clsParameter
        End If
        clsRCodeObject.AddParameterWithCodeStructure(strParameterName:=clsParm.strArgumentName, strParameterValue:=clsParm.strArgumentValue, clsRCodeObject:=clsParm.clsArgumentCodeStructure)
    End Sub

    Public Overridable Function GetDefault() As Object
        Return objDefault
    End Function

    Public Overridable Function ValueContainedIn(lstTemp As Object()) As Boolean
        Return False
    End Function

    Public Overridable Sub AddOrRemoveParameter(bAdd As Boolean)
        If clsRCode IsNot Nothing AndAlso clsParameter IsNot Nothing Then
            If bAdd Then
                clsRCode.AddParameter(clsParameter, iParameterPosition)
            Else
                clsRCode.RemoveParameter(clsParameter)
            End If
        End If
    End Sub

    Public Sub AddToLinkedControls(lstLinked As ucrCore(), objValues As Object, Optional bNewLinkedAddRemoveParameter As Boolean = False, Optional bNewLinkedUpdateFunction As Boolean = False, Optional bNewLinkedDisabledIfParameterMissing As Boolean = False, Optional bNewLinkedHideIfParameterMissing As Boolean = False, Optional bNewLinkedChangeParameterToDefault As Boolean = False)
        For Each ucrLinked As ucrCore In lstLinked
            AddToLinkedControls(ucrLinked:=ucrLinked, objValues:=objValues, bNewLinkedAddRemoveParameter:=bNewLinkedAddRemoveParameter, bNewLinkedUpdateFunction:=bNewLinkedUpdateFunction, bNewLinkedDisabledIfParameterMissing:=bNewLinkedDisabledIfParameterMissing, bNewLinkedHideIfParameterMissing:=bNewLinkedHideIfParameterMissing, bNewLinkedChangeParameterToDefault:=bNewLinkedChangeParameterToDefault)
        Next
    End Sub

    Public Sub AddToLinkedControls(ucrLinked As ucrCore, objValues As Object(), Optional bNewLinkedAddRemoveParameter As Boolean = False, Optional bNewLinkedUpdateFunction As Boolean = False, Optional bNewLinkedDisabledIfParameterMissing As Boolean = False, Optional bNewLinkedHideIfParameterMissing As Boolean = False, Optional bNewLinkedChangeParameterToDefault As Boolean = False)
        If Not IsLinkedTo(ucrLinked) Then
            ucrLinked.bLinkedAddRemoveParameter = bNewLinkedAddRemoveParameter
            ucrLinked.bLinkedChangeParameterToDefault = bNewLinkedChangeParameterToDefault
            ucrLinked.bLinkedDisabledIfParameterMissing = bNewLinkedDisabledIfParameterMissing
            ucrLinked.bLinkedHideIfParameterMissing = bNewLinkedHideIfParameterMissing
            ucrLinked.bLinkedUpdateFunction = bNewLinkedUpdateFunction
            lstValuesAndControl.Add(New KeyValuePair(Of Object(), ucrCore)(objValues, ucrLinked))
        End If
    End Sub

    Public Function IsLinkedTo(ucrControl) As Boolean
        Dim bTemp As Boolean = False

        For Each kvpTemp As KeyValuePair(Of Object(), ucrCore) In lstValuesAndControl
            If kvpTemp.Value.Equals(ucrControl) Then
                bTemp = True
                Exit For
            End If
        Next
        Return bTemp
    End Function
End Class