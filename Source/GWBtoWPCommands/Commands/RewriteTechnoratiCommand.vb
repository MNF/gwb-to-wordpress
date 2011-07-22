Imports Hinshlabs.CommandLineParser
Imports MetaWebLogEdit.MetaWeblogApi
Imports System.Collections.ObjectModel
Imports System.Net

Public Class RewriteTechnoratiCommand
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

    Public Overrides ReadOnly Property Switches As ReadOnlyCollection(Of Hinshlabs.CommandLineParser.SwitchInfo)
        Get
            Return Me.CommandLine.Switches
        End Get
    End Property

    Public Sub New(ByVal parent As CommandCollection)
        MyBase.New(parent)
    End Sub

    Public Overrides ReadOnly Property Description As String
        Get
            Return "Deletes all Tags and replaces with catagories"
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "retech"
        End Get
    End Property

    Public Overrides ReadOnly Property Qualifications As String
        Get
            Return "You must specify a valid username and password"
        End Get
    End Property

    Protected Overrides Function RunCommand() As Integer
        '  get info on user's blogs */ 
        Dim blogs() As CookComputing.Blogger.BlogInfo = Server.getUsersBlogs(String.Empty, Me.CommandLine.Username, Me.CommandLine.Password)
        Dim b As CookComputing.Blogger.BlogInfo
        For Each b In blogs
            Console.WriteLine("The URL of '{0}' is {1}", b.blogName, b.url)
            '  display title of the ten most recent posts. */ 
            Dim rawPosts() As CookComputing.MetaWeblog.Post = Server.getRecentPosts(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password, 800)
            Dim posts As List(Of CookComputing.MetaWeblog.Post) = rawPosts.ToList.ToList
            Console.WriteLine(String.Format("Processing {0} posts>", posts.Count))
            Dim p As CookComputing.MetaWeblog.Post
            '-------------------------------------------------
            Dim number As Integer = 0
            For Each p In posts
                Try

                    'p = GWBProcedures.StripScriptTags(p, "SCRIPT")
                    'p = GWBProcedures.StripScriptTags(p, "script")
                    p = GWBProcedures.StripWlWriterTags(p)
                    p = GWBProcedures.InsertWlWriterTags(p)

                    If Server.editPost(p.postid, Me.CommandLine.Username, Me.CommandLine.Password, p, True) Then
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.Write(String.Format("[{0}]", p.postid))
                    Else
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write(String.Format("[{0}]", p.postid))
                    End If

                Catch ex As Exception
                    Console.ForegroundColor = ConsoleColor.Red
                    Console.WriteLine()
                    Console.WriteLine(String.Format("[{0}] - {1} - {2} - {3}", p.postid, p.dateCreated.ToShortDateString, p.title, ex.Message))

                End Try

                Console.ResetColor()
                number = number + 1
                If number Mod 5 = 0 Then
                    Console.WriteLine()
                End If
            Next
        Next
        Return 0
    End Function



    Public Overrides ReadOnly Property Synopsis As String
        Get
            Return "Rewrites Technoraty tags"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Rewrite Technorati"
        End Get
    End Property

    Protected Overrides Function ValidateCommand() As Boolean
        Dim isValid As Boolean = True
        If Me.CommandLine.Username = String.Empty Then
            isValid = isValid & False
        End If
        If Me.CommandLine.Password = String.Empty Then
            isValid = isValid & False
        End If
        If Not isValid Then
            Me.CommandLine.WriteHelp(Console.Out)
        End If
        Return isValid
    End Function


End Class
