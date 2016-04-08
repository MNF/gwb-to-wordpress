Imports Hinshlabs.CommandLineParser
Imports MetaWebLogEdit.MetaWeblogApi
Imports System.Collections.ObjectModel
Imports System.Net

Public Class FindEmptyCatagorysCommand
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
            Return "Find all posts with no catagory and add one"
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "EmptyCat"
        End Get
    End Property

    Public Overrides ReadOnly Property Qualifications As String
        Get
            Return "You must specify a valid username and password"
        End Get
    End Property

    Protected Overrides Function RunCommand() As Integer
        Dim folderPath As String = "c:\temp\"
        Dim logPath = String.Format(folderPath + "ConsoleLog-EmptyCat-{0}-{1}.log", Me.CommandLine.Username, Now.ToString("yyyy-MM-dd-HH-mm-ss"))
        Dim consoleLog As New CsUtilities.ConsoleCopy(logPath)

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
            Dim ca() As CookComputing.MovableType.Category = Server.getCategoryList(b.blogName, Me.CommandLine.Username, Me.CommandLine.Password)
            Dim defaultCategory As String = "None" 'TODO parameter that could be changed 
            Dim nullCat As CookComputing.MovableType.Category = (From c In ca Where c.categoryName = defaultCategory).SingleOrDefault
            If nullCat.categoryId Is Nothing Then
                Console.WriteLine(String.Format("No category with name '{0}' found, you should create the category or change the name of defaultCategory ",
                                  defaultCategory))
                Return -1
            End If
            Dim nullList As New List(Of CookComputing.MovableType.Category)
            nullList.Add(nullCat)

            Dim number As Integer = 0



            For Each p In posts
                Dim postDescription As String = String.Format("[{0}] - {1} - {2} ", p.postid, p.dateCreated.ToShortDateString, p.title)
                Try

                    If p.categories Is Nothing Then
                        If Server.setPostCategories(p.postid, Me.CommandLine.Username, Me.CommandLine.Password, nullList.ToArray) Then
                            Console.ForegroundColor = ConsoleColor.Green
                            Console.WriteLine(postDescription + defaultCategory + " category added")
                        Else
                            Console.ForegroundColor = ConsoleColor.Yellow
                            Console.WriteLine(postDescription + defaultCategory + " category not added")
                        End If
                    Else
                        Console.ForegroundColor = ConsoleColor.Blue
                        Console.Write(String.Format("[{0}]", p.postid))
                    End If

                Catch ex As Exception
                    Console.ForegroundColor = ConsoleColor.Red
                    Console.WriteLine("")
                    Console.WriteLine(postDescription)
                    Console.WriteLine(ex.ToString)
                End Try

                Console.ResetColor()
                number = number + 1
                If number Mod 5 = 0 Then
                    Console.WriteLine()
                End If
            Next
        Next
        consoleLog.Dispose()
        Return 0
    End Function



    Public Overrides ReadOnly Property Synopsis As String
        Get
            Return "Rewrites Technoraty tags"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "EmptyCat"
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
