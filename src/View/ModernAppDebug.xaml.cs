using Atomus.Control;
using Atomus.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Atomus.Windows.Browser
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModernAppDebug : Application, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;

        public ModernAppDebug()
        {
            this.InitializeComponent();
        }

        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private bool IsLoginExcute = false;
        private bool IsAddUserControlExcute = false;
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            try
            {
                if (e.Action != "AddUserControl" && e.Action != "Login")
                    this.beforeActionEventHandler?.Invoke(this, e);

                switch (e.Action)
                {
                    case "Login":
                        if (!this.IsLoginExcute)
                        {
                            this.IsLoginExcute = true;
                            beforeActionEventHandler.Invoke(this, new AtomusControlEventArgs() { Action = "Login" });
                        }
                        else
                            (this.MainWindow as IAction).ControlAction(sender, "Login", e.Value);


                        return true;

                    case "AddUserControl":
                        if (!this.IsAddUserControlExcute)
                        {
                            this.IsAddUserControlExcute = true;
                            beforeActionEventHandler.Invoke(this, new AtomusControlEventArgs() { Action = "AddUserControl" });
                        }
                        else
                            (this.MainWindow as IAction).ControlAction(sender, "AddUserControl", e.Value);

                        return true;

                    default:
                        throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            finally
            {
                if (e.Action != "AddUserControl" && e.Action != "Login")
                    this.afterActionEventHandler?.Invoke(this, e);
            }

            return true;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as Button).WindowState = WindowState.Minimized;
        }
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as Button).WindowState = WindowState.Normal;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as Button).WindowState = WindowState.Maximized;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(sender as Button).Close();
        }
    }
}
