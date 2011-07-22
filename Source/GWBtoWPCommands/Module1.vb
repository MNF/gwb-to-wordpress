
Imports System.Net
Imports MetaWebLogEdit.MetaWeblogApi
Imports CookComputing.XmlRpc
Imports HtmlAgilityPack
Imports Hinshlabs.CommandLineParser

Module Module1



    Sub Main(ByVal args() As String)

        ' Create root command collection
        Dim root As New CommandCollection("root", Nothing)
        ' Add "demo1" fixed command
        root.AddCommand(New RewriteTechnoratiCommand(root))
        root.AddCommand(New FindEmptyCatagorysCommand(root))
        root.AddCommand(New RewriteCatagorysCommand(root))
        root.AddCommand(New RewriteUrlCommand(root))
        root.AddCommand(New CleanerTagsCommand(root))
        root.AddCommand(New ExportCommand(root))
        root.AddCommand(New ExportImagesCommand(root))
        root.AddCommand(New RewritePostsCommand(root))
        root.AddCommand(New PortImagesCommand(root))
        ' Start run
        root.Run(args)

        If Debugger.IsAttached Then
            Console.WriteLine("Debugger is Attached: Press any key to exit")
            Console.ReadKey()
        End If

    End Sub



End Module
