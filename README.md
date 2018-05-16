Telnet
======

The 9swampy/Telnet didn't work for me at all the first time I tested it. I tried a few other options before coming back and deciding to just update this one. I felt like it was the best code base for me to start with. I learned a lot and I eventually got it working for my application. I made quite a few improvements along the way so I decided to submit those as pull requests to the upstream master. This repository is for beta testing my code and submitting pull requests to Swampy.

Basic Usage:
```VB.NET
    Private Async Function RunRemoteScript(commandLine As String) As Task(Of Boolean)
        Using telnet = New Client("HostName", 23, _cancellationSource.Token)
            If Not telnet.IsConnected Then Return False
            Dim loggedOn = Await telnet.TryLoginAsync("username", "password", SocketTimeout, "#"))
            If Not loggedOn Then Return False
            Await telnet.WriteLine(commandLine)
            Dim serverResponse = Await telnet.TerminatedReadAsync("#", TimeSpan.FromMilliseconds(SocketTimeout))
            Debug.Print(serverResponse)
            Await telnet.WriteLine("exit")
            Dim logoutMessage = Await telnet.ReadAsync(New TimeSpan(100))
            Debug.Print(logoutMessage)
        End Using
        Return True  ' If we got this far; celebrate
    End Function
