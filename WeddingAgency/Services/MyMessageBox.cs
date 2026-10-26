using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeddingAgency.Views.Windows;

namespace WeddingAgency.Services
{
    public static class MyMessageBox
    {
        public static bool? Show(string title, string message, bool isConfirmation = false)
        {
            MyMessageBoxWindow msgBox = new MyMessageBoxWindow(title, message, isConfirmation);
            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}
