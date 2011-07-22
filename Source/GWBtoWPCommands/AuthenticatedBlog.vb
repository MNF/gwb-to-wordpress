Imports Hinshlabs.CommandLineParser

Public Class AuthenticatedBlog
    Inherits CommandLineBase

    Private _username As String
    Private _password As String

    <CommandLineSwitch("Username", "username for connecting to your blog")> _
    <CommandLineAlias("u")> _
    Public Property Username As String
        Get
            Return _username
        End Get
        Set(ByVal value As String)
            _username = value
        End Set
    End Property

    <CommandLineSwitch("Password", "Password for connecting to your blog")> _
     <CommandLineAlias("p")> _
    Public Property Password As String
        Get
            Return _password
        End Get
        Set(ByVal value As String)
            _password = value
        End Set
    End Property



End Class
