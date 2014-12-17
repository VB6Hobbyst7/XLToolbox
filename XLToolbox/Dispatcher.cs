﻿using System;
using System.Windows;
using Bovender.Mvvm;
using Bovender.Mvvm.Messaging;
using XLToolbox.ExceptionHandler;
using XLToolbox.Excel.Instance;
using XLToolbox.Excel.ViewModels;
using XLToolbox.About;
using XLToolbox.Versioning;
using XLToolbox.SheetManager;

namespace XLToolbox
{
    /// <summary>
    /// Central dispatcher for all UI-initiated XL Toolbox commands.
    /// </summary>
    public static class Dispatcher
    {
        #region Public method

        /// <summary>
        /// Central command dispatcher. This public method also contains
        /// the central error handler for user-friendly error messages.
        /// </summary>
        /// <remarks>
        /// An enum-based approach was choosen in favor of publicly
        /// accessible static methods to enable listing of commands,
        /// e.g. for key bindings.
        /// </remarks>
        /// <param name="cmd">XL Toolbox command to execute.</param>
        public static void Execute(Command cmd)
        {
            try
            {
                switch (cmd)
                {
                    case Command.About: About(); break;
                    case Command.CheckForUpdates: CheckForUpdates(); break;
                    case Command.SheetManager: SheetManager(); break;
                    case Command.ExportSelection: ExportSelection(); break;
                    case Command.ThrowError: throw new InsufficientMemoryException();
                }
            }
            catch (Exception e)
            {
                ExceptionViewModel vm = new ExceptionViewModel(e);
                vm.InjectInto<ExceptionView>().ShowDialog();
            }
        }

        #endregion

        #region Private dispatching methods

        static void About()
        {
            AboutViewModel avm = new AboutViewModel();
            avm.InjectInto<AboutView>().ShowDialog();
        }

        static void CheckForUpdates()
        {
            Bovender.Versioning.UpdaterViewModel uvm = new Bovender.Versioning.UpdaterViewModel(
                new Updater()
                );
            System.Windows.Threading.Dispatcher d = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            uvm.CheckForUpdateMessage.Sent += (object sender, MessageArgs<ProcessMessageContent> args) =>
            {
                Window view = args.Content.InjectInto<UpdaterProcessView>();
                args.Content.ViewModel.ViewDispatcher = view.Dispatcher;
                view.Show();
            };
            uvm.CheckForUpdateCommand.Execute(null);
        }

        static void SheetManager()
        {
            WorkbookViewModel wvm = new WorkbookViewModel(ExcelInstance.Application.ActiveWorkbook);
            Workarounds.ShowModelessInExcel<WorkbookView>(wvm);
        }

        static void ExportSelection()
        {
            Export.SingleExportSettingsViewModel vm = new Export.SingleExportSettingsViewModel();
            vm.InjectInto<Export.SingleExportSettingsView>().ShowDialog();
        }

        #endregion
    }
}
