Imports Hinshlabs.CommandLineParser
Imports MetaWebLogEdit.MetaWeblogApi
Imports System.Collections.ObjectModel
Imports System.Net

Public MustInherit Class MetaweblogCommandBase
    Inherits CommandBase(Of AuthenticatedBlog)

    Private _Server As GwbMetaWeblog

    Friend ReadOnly Property Server As GwbMetaWeblog
        Get
            If _Server Is Nothing Then
                _Server = New GwbMetaWeblog
                _Server.Credentials = New NetworkCredential(Me.CommandLine.Username, Me.CommandLine.Password)
            End If
            Return _Server
        End Get
    End Property

    Public Sub New(ByVal parent As CommandCollection)
        MyBase.New(parent)
    End Sub

    Public Overrides ReadOnly Property Switches As ReadOnlyCollection(Of Hinshlabs.CommandLineParser.SwitchInfo)
        Get
            Return Me.CommandLine.Switches
        End Get
    End Property

End Class
