using Atomus.Control;
using Atomus.Diagnostics;
using Atomus.Windows.Browser.Controllers;
using Atomus.Windows.Browser.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Atomus.Windows.Browser.ViewModel
{
    public class ModernBrowserDebugViewModel : Atomus.MVVM.ViewModel
    {
        #region Declare

        private string title;

        private bool isShutdown;
        private bool isEnabledControl;
        #endregion

        #region Property
        public ICore Core { get; set; }

        public string UserID
        {
            get
            {
                return string.Format("ID : {0}", Config.Client.GetAttribute("Account.EMAIL"));
                //return (string)Config.Client.GetAttribute("Account.USER_ID");
            }
            set
            {
                this.NotifyPropertyChanged();
            }
        }
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool IsShutdown
        {
            get
            {
                return this.isShutdown;
            }
            set
            {
                if (this.isShutdown != value)
                {
                    this.isShutdown = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool IsEnabledControl
        {
            get
            {
                return this.isEnabledControl;
            }
            set
            {
                if (this.isEnabledControl != value)
                {
                    this.isEnabledControl = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ImageSource ICon
        {
            get
            {
                return (this.Core as Window).Icon;
            }
            set
            {
                if ((this.Core as Window).Icon != value)
                {
                    (this.Core as Window).Icon = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ICommand ShutdownCommand { get; set; }
        public ICommand OpenControlCommand { get; set; }        
        #endregion

        #region INIT
        public ModernBrowserDebugViewModel()
        {
            this.ShutdownCommand = new MVVM.DelegateCommand(async () => { await this.ShutdownProcess(); }
                                                            , () => { return !this.isShutdown; });

            this.OpenControlCommand = new MVVM.DelegateCommand(() => { this.OpenControlProcess(null, null,  -1, -1, null); }
                                                            , () => { return this.IsEnabledControl; });        
        }
        public ModernBrowserDebugViewModel(ICore core) : this()
        {
            this.Core = core;
        }
        #endregion

        #region IO
        private async Task ShutdownProcess()
        {
            try
            {
                (this.ShutdownCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();

                this.isShutdown = false;

                if (this.Core.WindowsMessageBoxShow(Application.Current.Windows[0], "종료 하시겠습니까?", "종료", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.isShutdown = true;
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
            }
            finally
            {
                (this.ShutdownCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
            }
        }

        internal bool Login(string DatabaseName, string ProcedureID, string EMAIL, string ACCESS_NUMBER)
        {
            Service.IResponse result;

            try
            {
                result = this.Core.Login(DatabaseName, ProcedureID, EMAIL, ACCESS_NUMBER);

                if (result.Status == Service.Status.OK)
                {
                    if (result.DataSet != null && result.DataSet.Tables.Count >= 1)
                        foreach (DataTable _DataTable in result.DataSet.Tables)
                            for (int i = 1; i < _DataTable.Columns.Count; i++)
                                foreach (DataRow _DataRow in _DataTable.Rows)
                                    Config.Client.SetAttribute(string.Format("{0}.{1}", _DataRow[0].ToString(), _DataTable.Columns[i].ColumnName), _DataRow[i]);

                    return true;
                }
                else
                {
                    this.WindowsMessageBoxShow(Application.Current.Windows[0], result.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
            }

            return false;
        }

        internal IAction OpenControlProcess(string DatabaseName, string ProcedureID, decimal MENU_ID, decimal ASSEMBLY_ID, IAction core)
        {
            Service.IResponse result;
            IAction _Core;

            try
            {
                this.IsEnabledControl = false;
                (this.OpenControlCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();

                if (MENU_ID < 1) return null;
                if (ASSEMBLY_ID < 1) return null;

                result = this.Core.SearchOpenControl(DatabaseName, ProcedureID, MENU_ID, ASSEMBLY_ID);

                if (result.Status == Service.Status.OK)
                {
                    if (result.DataSet.Tables.Count == 2)
                        if (result.DataSet.Tables[0].Rows.Count == 1)
                        {
                            if (core == null)
                                if (result.DataSet.Tables[0].Columns.Contains("FILE_TEXT") && result.DataSet.Tables[0].Rows[0]["FILE_TEXT"] != DBNull.Value)
                                    _Core = (IAction)Factory.CreateInstance(Convert.FromBase64String((string)result.DataSet.Tables[0].Rows[0]["FILE_TEXT"]), result.DataSet.Tables[0].Rows[0]["NAMESPACE"].ToString(), false, false);
                                else
                                    _Core = (IAction)Factory.CreateInstance((byte[])result.DataSet.Tables[0].Rows[0]["FILE"], result.DataSet.Tables[0].Rows[0]["NAMESPACE"].ToString(), false, false);
                            else
                                _Core = core;

                            //_Core.BeforeActionEventHandler += UserControl_BeforeActionEventHandler;
                            //_Core.AfterActionEventHandler += UserControl_AfterActionEventHandler;

                            _Core.SetAttribute("MENU_ID", MENU_ID.ToString());
                            _Core.SetAttribute("ASSEMBLY_ID", ASSEMBLY_ID.ToString());

                            foreach (DataRow _DataRow in result.DataSet.Tables[1].Rows)
                            {
                                _Core.SetAttribute(_DataRow["ATTRIBUTE_NAME"].ToString(), _DataRow["ATTRIBUTE_VALUE"].ToString());
                            }

                            _Core.SetAttribute("NAME", result.DataSet.Tables[0].Rows[0]["NAME"].ToString().Translate());
                            _Core.SetAttribute("DESCRIPTION", string.Format("{0} {1}", (result.DataSet.Tables[0].Rows[0]["DESCRIPTION"] as string).Translate(), _Core.GetType().Assembly.GetName().Version.ToString()));

                            //if (addTabControl)
                            //    this.OpenControl((result.DataSet.Tables[0].Rows[0]["NAME"] as string).Translate(), string.Format("{0} {1}", (result.DataSet.Tables[0].Rows[0]["DESCRIPTION"] as string).Translate(), _Core.GetType().Assembly.GetName().Version.ToString()), (UserControl)_Core);

                            //if (_AtomusControlEventArgs != null)
                            //    _Core.ControlAction(sender, _AtomusControlEventArgs.Action, _AtomusControlEventArgs.Value);

                            return _Core;
                        }

                    return null;
                }
                else
                {
                    this.WindowsMessageBoxShow(Application.Current.Windows[0], result.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

            }
            catch (Exception ex)
            {
                this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
                return null;
            }
            finally
            {
                this.IsEnabledControl = true;
                (this.OpenControlCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region ETC
        #endregion
    }
}