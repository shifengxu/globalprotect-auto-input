// See https://aka.ms/new-console-template for more information
using ConsoleApp1;

Console.WriteLine("============================================================");
Console.WriteLine("               NTU VPN GlobalProtect Trigger                ");
Console.WriteLine("This is for and only for NTU VPN GlobalProtect tool. And it ");
Console.WriteLine("can input the email and password automatically. But it won't");
Console.WriteLine("bypass the 2FA authentication, and you still need to confirm");
Console.WriteLine("on your phone.");
Console.WriteLine("============================================================");

InputManager im = new InputManager();

//im.InputString("!@#$%^&*()");

//im.ListAll();

im.Run(@".\email_pword.txt");

//im.ShowWindowByTitle();
