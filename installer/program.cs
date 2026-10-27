using System;
using System.Windows.Forms;

public static class program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new window());
    }
}